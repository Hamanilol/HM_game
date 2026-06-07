using UnityEngine;
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

        private void Start()
        {
            // Auto-assign animator if missing
            if (characterAnimator == null)
            {
                characterAnimator = GetComponentInChildren<Animator>();
            }

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

        // ----- Slot Switching (unchanged logic) -----
        private void HandleSlotInput()
        {
            // Number keys 1‑9
            for (int i = 0; i < slotCount && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SwapToSlot(i);
                    break;
                }
            }

            // Scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
                SwapToSlot((_currentSlotIndex + 1) % slotCount);
            else if (scroll < 0f)
                SwapToSlot((_currentSlotIndex - 1 + slotCount) % slotCount);
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
            else
                characterAnimator?.SetInteger("WeaponPose", 0);
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

            // Fire input (semi‑auto and automatic).
            // Use the explicit left mouse button instead of the "Fire1" axis,
            // because the default "Fire1" binding also includes Left Ctrl, which
            // is the crouch key — that caused crouching to fire the weapon.
            if (Input.GetMouseButtonDown(0))
            {
                _fireButtonHeld = true;

                // For pump‑action: if needsPump, pump instead of firing
                if (_currentWeapon.isPumpAction && _currentWeapon.needsPump)
                {
                    _currentWeapon.Pump();
                }
                else
                {
                    // Try to fire (works for semi‑auto and the first shot of auto)
                    _currentWeapon.TryFire();
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                _fireButtonHeld = false;
            }

            // Automatic fire handled inside the weapon’s Update (BaseWeapon already checks GetButton)
            // But we can also call TryFire continuously for automatics – the weapon handles fire rate.
            // The weapon's Update already does that because it checks Input.GetButton.
            // Since we instantiate weapons as children of the hand, their Update runs normally.

            // Reload
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(_currentWeapon.Reload());
            }

            // Aim (hold) — explicit right mouse button (avoids legacy axis conflicts)
            if (Input.GetMouseButton(1))
            {
                _currentWeapon.SetAiming(true);
                _aimButtonHeld = true;
            }
            else if (_aimButtonHeld)
            {
                _currentWeapon.SetAiming(false);
                _aimButtonHeld = false;
            }

            // Pump action (alternative: dedicated key, or we already handled via Fire button above)
            // If you prefer a separate pump key, add: if (Input.GetKeyDown(KeyCode.???)) _currentWeapon.Pump();
        }

        // Public accessors for camera and UI
        public BaseWeapon GetCurrentWeapon() => _currentWeapon;
        public int GetCurrentSlot() => _currentSlotIndex;
    }
}