using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Abdulrahman.PlayerSystem;

namespace Abdulrahman.InventorySystem
{
    public class QuickSwapInventory : MonoBehaviour
    {
        [Header("Settings")]
        [Header("Animation")]
        public Transform hand;                // Parent for all held items (child of camera)
        public int slotCount = 5;
        public Animator characterAnimator;

        public enum WeaponCategory
        {
            Unarmed = 0,
            Pistol = 1
        }

        [Header("Items")]
        public List<GameObject> itemPrefabs = new List<GameObject>();

        // Runtime
        private int _currentSlotIndex = -1;
        private GameObject _currentItemInstance;
        private BaseWeapon _currentWeapon;    // The weapon component on the current item
        private List<GameObject> _slots = new List<GameObject>();

        // Weapon input state
        private bool _fireButtonHeld;
        private bool _aimButtonHeld;

        // Player identity (drives keyboard/mouse vs gamepad input for co-op)
        private PlayerController _owner;
        private bool _isPlayer2;

        private void Start()
        {
            // Auto-assign animator if missing
            if (characterAnimator == null)
            {
                characterAnimator = GetComponentInChildren<Animator>();
            }

            // Determine which player owns this inventory so input is routed correctly.
            _owner = GetComponent<PlayerController>();
            if (_owner == null) _owner = GetComponentInParent<PlayerController>();
            _isPlayer2 = _owner != null && _owner.isPlayer2;

            for (int i = 0; i < slotCount; i++)
{
                if (i < itemPrefabs.Count)
                    _slots.Add(itemPrefabs[i]);
                else
                    _slots.Add(null);
            }
            SwapToSlot(0);
        }

        private void Update()
        {
            HandleSlotInput();
            HandleWeaponInput();
        }

        // ----- Slot Switching -----
        private void HandleSlotInput()
        {
            if (!_isPlayer2)
            {
                // PLAYER 1: number keys + scroll wheel
                for (int i = 0; i < slotCount && i < 9; i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    {
                        SwapToSlot(i);
                        break;
                    }
                }

                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll > 0f)
                    SwapToSlot((_currentSlotIndex + 1) % slotCount);
                else if (scroll < 0f)
                    SwapToSlot((_currentSlotIndex - 1 + slotCount) % slotCount);
            }
            else
            {
                // PLAYER 2: gamepad shoulder buttons cycle weapons
                Gamepad gp = Gamepad.current;
                if (gp == null) return;
                if (gp.rightShoulder.wasPressedThisFrame)
                    SwapToSlot((_currentSlotIndex + 1) % slotCount);
                else if (gp.leftShoulder.wasPressedThisFrame)
                    SwapToSlot((_currentSlotIndex - 1 + slotCount) % slotCount);
            }
        }

        public void SwapToSlot(int index)
        {
            if (index < 0 || index >= _slots.Count) return;
            if (index == _currentSlotIndex) return;

            _currentSlotIndex = index;
            EquipItem(_slots[_currentSlotIndex]);
        }

        private void EquipItem(GameObject prefab)
        {
            if (_currentItemInstance != null)
                Destroy(_currentItemInstance);
            _currentWeapon = null;

            if (prefab != null && hand != null)
            {
                _currentItemInstance = Instantiate(prefab, hand);
                _currentItemInstance.transform.localPosition = Vector3.zero;
                _currentItemInstance.transform.localRotation = Quaternion.identity;

                // Try to get the weapon component
                _currentWeapon = _currentItemInstance.GetComponent<BaseWeapon>();
            }
            if (_currentWeapon != null)
                UpdateWeaponPose();
            else if (characterAnimator != null)
                characterAnimator.SetInteger("WeaponPose", 0);
        }

        private void UpdateWeaponPose()
        {
            if (characterAnimator == null) return;
            WeaponCategory category = GetWeaponCategory();
            characterAnimator.SetInteger("WeaponPose", (int)category);
        }

        private WeaponCategory GetWeaponCategory()
        {
            if (_currentWeapon == null) return WeaponCategory.Unarmed;

            // Pistols and SMGs -> Pistol pose
            if (_currentWeapon is Pistol || _currentWeapon is SMG)
                return WeaponCategory.Pistol;

            // All other weapons now return to Unarmed/Normal Idle as requested
            return WeaponCategory.Unarmed;
        }

        // ----- Weapon Input -----
        private void HandleWeaponInput()
        {
            if (_currentWeapon == null) return;

            // Gather input from the correct device for this player.
            bool fireDown, fireHeld, aimHeld, reloadDown;

            if (!_isPlayer2)
            {
                // PLAYER 1: keyboard + mouse.
                // Use the explicit left mouse button instead of the "Fire1" axis,
                // because the default "Fire1" binding also includes Left Ctrl, which
                // is the crouch key — that caused crouching to fire the weapon.
                fireDown   = Input.GetMouseButtonDown(0);
                fireHeld   = Input.GetMouseButton(0);
                aimHeld    = Input.GetMouseButton(1);   // right mouse to aim
                reloadDown = Input.GetKeyDown(KeyCode.R);
            }
            else
            {
                // PLAYER 2: gamepad. Right trigger = fire, left trigger = aim,
                // Square/X (buttonWest) = reload.
                Gamepad gp = Gamepad.current;
                if (gp != null)
                {
                    fireDown   = gp.rightTrigger.wasPressedThisFrame;
                    fireHeld   = gp.rightTrigger.isPressed;
                    aimHeld    = gp.leftTrigger.isPressed;
                    reloadDown = gp.buttonWest.wasPressedThisFrame;
                }
                else
                {
                    fireDown = fireHeld = aimHeld = reloadDown = false;
                }
            }

            // Fire (semi-auto: on press; automatic: while held — weapon gates by fire rate)
            if (fireDown)
            {
                _fireButtonHeld = true;

                // For pump‑action: if needsPump, pump instead of firing
                if (_currentWeapon.isPumpAction && _currentWeapon.needsPump)
                    _currentWeapon.Pump();
                else
                    _currentWeapon.TryFire();
            }
            else if (fireHeld && _currentWeapon.isAutomatic)
            {
                _currentWeapon.TryFire();
            }
            if (!fireHeld)
                _fireButtonHeld = false;

            // Reload
            if (reloadDown)
            {
                StartCoroutine(_currentWeapon.Reload());
            }

            // Aim (hold)
            if (aimHeld)
            {
                _currentWeapon.SetAiming(true);
                _aimButtonHeld = true;
            }
            else if (_aimButtonHeld)
            {
                _currentWeapon.SetAiming(false);
                _aimButtonHeld = false;
            }
        }

        // Public accessors for camera and UI
        public BaseWeapon GetCurrentWeapon() => _currentWeapon;
        public int GetCurrentSlot() => _currentSlotIndex;
    }
}