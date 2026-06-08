using UnityEngine;
using UnityEngine.InputSystem;
using Abdulrahman.InventorySystem;

/// <summary>
/// A high-performance, stable first-person camera for single player and co-op.
/// Uses local-space positioning to eliminate jitter and rotation stacking.
/// </summary>
[DefaultExecutionOrder(-100)]
public class StableFirstPersonCamera : MonoBehaviour
{
    [Header("Co-Op Settings")]
    public bool isPlayer2 = false;

    [Header("Player 2 Gamepad Look")]
    public float gamepadLookSpeed = 150f;
    public bool gamepadInvertLookY = false;
    [Range(0f, 0.6f)] public float gamepadLookDeadzone = 0.15f;

    [Header("Sensitivity & Limits")]
    public float mouseSensitivity = 2f;
    public float pitchMin = -80f;
    public float pitchMax = 80f;
    public float recoilRecoverySpeed = 10f;

    [Header("References")]
    public Transform characterRoot;          // The object that rotates left/right (Yaw)
    public Animator characterAnimator;       // The animator providing head/spine bones
    public Transform cameraTransform;        // The actual Camera object
    public QuickSwapInventory inventory;

    [Header("Eye & Bob Settings")]
    [Tooltip("Default eye height if bones are missing.")]
    public float defaultEyeHeight = 1.6f;
    [Tooltip("How much the camera follows the animated head bone (0 = stable, 1 = full bob).")]
    [Range(0f, 1f)] public float bobStrength = 0.3f;
    [Tooltip("Smoothing applied to the bobbing motion to prevent jitter.")]
    public float bobSmoothing = 15f;

    private float _pitch = 0f;
    private float _yaw = 0f;
    private Vector3 _accumulatedRecoil;
    private Vector3 _currentBobOffset;
    
    private Transform _headBone;
    private Transform _spineBone;
    private Vector3 _initialHeadLocalPos;

    private void Start()
    {
        // Initial locks
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Auto-assign components
        if (characterRoot == null) characterRoot = transform;
        if (cameraTransform == null) cameraTransform = GetComponentInChildren<Camera>()?.transform;
        
        // Setup character root for rotation
        if (characterRoot != null)
        {
            _yaw = characterRoot.eulerAngles.y;
        }

        InitializeBones();
    }

    private void InitializeBones()
    {
        if (characterAnimator == null) return;

        // Ensure we point to the animator with the avatar (usually on a child model)
        if (characterAnimator.avatar == null)
        {
            var childAnim = characterAnimator.GetComponentInChildren<Animator>();
            if (childAnim != null && childAnim.avatar != null) characterAnimator = childAnim;
        }

        if (characterAnimator.avatar != null)
        {
            _headBone = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
            _spineBone = characterAnimator.GetBoneTransform(HumanBodyBones.Chest);
            if (_spineBone == null) _spineBone = characterAnimator.GetBoneTransform(HumanBodyBones.Spine);
            
            if (_headBone != null)
            {
                // Capture initial local position of the head relative to the root
                _initialHeadLocalPos = characterRoot.InverseTransformPoint(_headBone.position);
            }
        }
    }

    private void Update()
    {
        // Global freeze if game paused or shop open
        if (PauseMenu.GameIsPaused) return;
        if (Abdulrahman.NPC.ShopManager.Instance != null && 
            Abdulrahman.NPC.ShopManager.Instance.shopUI != null && 
            Abdulrahman.NPC.ShopManager.Instance.shopUI.activeSelf) return;

        HandleInput();
        ApplyRotations();
    }

    private void HandleInput()
    {
        float lookX = 0f;
        float lookY = 0f;

        if (isPlayer2)
        {
            Gamepad gp = Gamepad.current;
            if (gp != null)
            {
                Vector2 stick = ApplyRadialDeadzone(gp.rightStick.ReadValue(), gamepadLookDeadzone);
                float ySign = gamepadInvertLookY ? -1f : 1f;
                lookX = stick.x * gamepadLookSpeed * Time.deltaTime;
                lookY = stick.y * gamepadLookSpeed * Time.deltaTime * ySign;
            }
        }
        else
        {
            // Only process mouse when locked
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                float sens = PlayerPrefs.GetFloat("MouseSensitivity", mouseSensitivity);
                // Normalized scaling: 0.05f provides a good range for 1-10 sensitivity
                lookX = Input.GetAxis("Mouse X") * sens * 0.05f;
                lookY = Input.GetAxis("Mouse Y") * sens * 0.05f;
}
        }

        _yaw += lookX;
        _pitch -= lookY;
        _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);
    }

    private void ApplyRotations()
    {
        // Apply Horizontal (Yaw) to Character Root
        if (characterRoot != null)
        {
            characterRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }

        // Apply Vertical (Pitch) + Recoil to Camera locally
        if (cameraTransform != null)
        {
            // Handle recoil logic
            BaseWeapon currentWeapon = inventory != null ? inventory.GetCurrentWeapon() : null;
            if (currentWeapon != null)
            {
                _accumulatedRecoil += currentWeapon.recoilRequest;
                currentWeapon.recoilRequest = Vector3.zero;
            }
            _accumulatedRecoil = Vector3.Lerp(_accumulatedRecoil, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);

            cameraTransform.localRotation = Quaternion.Euler(_pitch + _accumulatedRecoil.x, _accumulatedRecoil.y, 0f);
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Handle Position (Local Bobbing)
        Vector3 targetLocalPos = new Vector3(0, defaultEyeHeight, 0);

        if (_headBone != null && bobStrength > 0)
        {
            // Get current head local position relative to character root
            Vector3 currentHeadLocal = characterRoot.InverseTransformPoint(_headBone.position);
            // We only want the DELTA from the initial pose to isolate bobbing from scaling/base height
            Vector3 bobDelta = currentHeadLocal - _initialHeadLocalPos;
            
            // Add bobbing to the target height
            targetLocalPos += bobDelta * bobStrength;
        }

        // Smoothly interpolate local position to prevent micro-stutters from bone animations
        _currentBobOffset = Vector3.Lerp(_currentBobOffset, targetLocalPos, Time.deltaTime * bobSmoothing);
        cameraTransform.localPosition = _currentBobOffset;

        // Visual Upper-Body rotation for co-op (other players see you look up/down)
        if (_spineBone != null && !PauseMenu.GameIsPaused)
        {
            // This is additive to the animation state
            // We use a safe method that doesn't stack: calculate the desired local rotation relative to root
            Quaternion pitchRot = Quaternion.Euler(_pitch, 0, 0);
            // We don't overwrite .rotation here because it's inconsistent across characters, 
            // instead we use a common bone-rotation pattern if applicable, but for simplicity:
            // Just let the animator handle the body and camera handle the eyes for now to maximize stability.
        }
    }

    private static Vector2 ApplyRadialDeadzone(Vector2 stick, float deadzone)
    {
        float magnitude = stick.magnitude;
        if (magnitude <= deadzone || magnitude <= 0.0001f) return Vector2.zero;
        float scaled = Mathf.Clamp01((magnitude - deadzone) / (1f - deadzone));
        return (stick / magnitude) * scaled;
    }

    private Vector3 GetStableEyePosition()
    {
        return (characterRoot != null ? characterRoot.position : transform.position) + Vector3.up * defaultEyeHeight;
    }
}

