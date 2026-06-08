using UnityEngine;
using UnityEngine.InputSystem;
using Abdulrahman.InventorySystem;

[DefaultExecutionOrder(-100)]
public class StableFirstPersonCamera : MonoBehaviour
{
    [Header("Co-Op Settings")]
    public bool isPlayer2 = false;

    [Header("Player 2 Gamepad Look (New Input System)")]
    [Tooltip("Right-stick look speed in degrees per second for Player 2's gamepad.")]
    public float gamepadLookSpeed = 200f;
    [Tooltip("Invert the vertical look axis for Player 2's gamepad.")]
    public bool gamepadInvertLookY = false;
    [Range(0f, 0.6f)]
    [Tooltip("Right-stick magnitude below this is treated as zero. Prevents the camera drifting when the stick is centered/idle.")]
    public float gamepadLookDeadzone = 0.2f;

    [Header("Sensitivity")]
    
    public QuickSwapInventory inventory;   // Assign in inspector
    private Vector3 accumulatedRecoil;
    private float recoilRecoverySpeed = 8f;
    public float mouseSensitivity = 2f;
    public float pitchMin = -85f;
    public float pitchMax = 85f;

    [Header("References")]
    public Transform characterRoot;          // Rotates for yaw
    public Animator characterAnimator;       // To get head bone
    public Transform cameraTransform;        // The main camera

    [Header("Eye Offset")]
    public Vector3 eyeOffset = new Vector3(0f, 0.08f, 0.05f); // in head bone local space

    private float pitch = 0f;  // current up/down angle
    private float yaw = 0f;    // we'll sync to characterRoot.rotation, but keep for clarity

    private Transform headBone;
    private Transform spineBone;

    /// <summary>
    /// Radial deadzone with rescaling. Returns Vector2.zero while the stick is
    /// inside the deadzone (eliminating idle drift), then ramps the magnitude
    /// smoothly from 0 once the stick moves beyond the threshold.
    /// </summary>
    private static Vector2 ApplyRadialDeadzone(Vector2 stick, float deadzone)
    {
        float magnitude = stick.magnitude;
        if (magnitude <= deadzone || magnitude <= 0.0001f) return Vector2.zero;
        float scaled = Mathf.Clamp01((magnitude - deadzone) / (1f - deadzone));
        return (stick / magnitude) * scaled;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (characterAnimator != null)
        {
            // If the assigned animator doesn't have an avatar, check its children 
            // (common if assigned to the root player object instead of the model)
            if (characterAnimator.avatar == null)
            {
                var childAnimator = characterAnimator.GetComponentInChildren<Animator>();
                if (childAnimator != null && childAnimator.avatar != null)
                {
                    characterAnimator = childAnimator;
                }
            }

            if (characterAnimator.avatar != null && characterAnimator.isHuman)
            {
                headBone = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
                // Also get Spine2 for upper body rotation (pitch)
                spineBone = characterAnimator.GetBoneTransform(HumanBodyBones.Chest);
                if (spineBone == null) spineBone = characterAnimator.GetBoneTransform(HumanBodyBones.Spine);
            }
            else
            {
                Debug.LogWarning($"Animator on {characterAnimator.name} has no Humanoid Avatar. The camera will use a fixed height fallback (1.7m).", this);
            }

            if (headBone == null && characterAnimator.avatar != null)
            {
                Debug.LogWarning($"Head bone not found on {characterAnimator.name}. The camera will use a fixed height fallback (1.7m). Ensure the Animator has a Humanoid Avatar and the model bones are children of the Animator.", this);
            }
        }
        else
        {
            Debug.LogWarning($"characterAnimator is not assigned on {gameObject.name}. The camera will use a fixed height fallback.", this);
        }

        // Initialize yaw from current character rotation
        if (characterRoot != null)
        {
            yaw = characterRoot.eulerAngles.y;
        }
        else
        {
            Debug.LogError($"characterRoot is not assigned on {gameObject.name}! Camera rotation will not work correctly.");
        }
        
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
    }

    void Update()
    {
        if (PauseMenu.GameIsPaused)
        {
            return;
        }

        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);

        // --- Mouse input ---
        float mouseX = 0f;
        float mouseY = 0f;

        if (isPlayer2)
        {
            Gamepad gp = Gamepad.current;
            if (gp != null)
            {
                Vector2 look = ApplyRadialDeadzone(gp.rightStick.ReadValue(), gamepadLookDeadzone);
                float lookYSign = gamepadInvertLookY ? -1f : 1f;
                mouseX = look.x * gamepadLookSpeed * Time.deltaTime;
                mouseY = look.y * gamepadLookSpeed * Time.deltaTime * lookYSign;
            }
        }
        else
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }

        yaw   += mouseX;
        pitch -= mouseY;
        pitch  = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // --- Apply root rotation in Update for consistency with movement ---
        if (characterRoot != null)
        {
            characterRoot.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }

    void LateUpdate()
    {
        if (PauseMenu.GameIsPaused)
        {
            return;
        }

        // --- Apply rotation overrides to bones and camera ---
        
        // Apply Pitch to Spine/UpperBody so arms/hands move with camera
        if (spineBone != null)
        {
            spineBone.localRotation = Quaternion.Euler(pitch, 0f, 0f) * spineBone.localRotation;
        }

        // Camera rotation
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Recoil
        BaseWeapon currentWeapon = inventory != null ? inventory.GetCurrentWeapon() : null;
        if (currentWeapon != null)
        {
            accumulatedRecoil += currentWeapon.recoilRequest;
            currentWeapon.recoilRequest = Vector3.zero;   // Consumed
        }

        // Smooth recovery
        accumulatedRecoil = Vector3.Lerp(accumulatedRecoil, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);

        // Apply final rotation to camera
        cameraTransform.rotation = Quaternion.Euler(
            pitch + accumulatedRecoil.x,
            yaw + accumulatedRecoil.y,
            0f
        );

        // --- Position: follow the head bone's world position ---
        if (headBone != null)
        {
            Vector3 worldEyePosition = headBone.position + headBone.TransformDirection(eyeOffset);
            cameraTransform.position = worldEyePosition;
        }
        else
        {
            cameraTransform.position = characterRoot.position + Vector3.up * 1.7f;
        }
    }
}