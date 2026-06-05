using UnityEngine;
using UnityEngine.InputSystem;
using Abdulrahman.InventorySystem;

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

        headBone = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
        if (headBone == null)
            Debug.LogError("Head bone not found!");

        // Initialize yaw from current character rotation
        yaw = characterRoot.eulerAngles.y;
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
    }

    void LateUpdate()
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
            // Player 2: read the gamepad's RIGHT STICK via the new Input System.
            // The old code read legacy "Joystick X/Y" (axes 3 & 4), which on a
            // DualShock 4 over HID are the L2/R2 TRIGGERS, not the stick — that is
            // why the triggers were controlling the look. The Gamepad abstraction
            // maps rightStick correctly across controllers/platforms.
            Gamepad gp = Gamepad.current;
            if (gp != null)
            {
                Vector2 look = ApplyRadialDeadzone(gp.rightStick.ReadValue(), gamepadLookDeadzone);
                // Sticks return a sustained value, so scale by deltaTime for
                // frame-rate-independent rotation (degrees per second).
                float lookYSign = gamepadInvertLookY ? -1f : 1f;
                mouseX = look.x * gamepadLookSpeed * Time.deltaTime;
                mouseY = look.y * gamepadLookSpeed * Time.deltaTime * lookYSign;
            }
            else
            {
                mouseX = 0f;
                mouseY = 0f;
            }
        }
        else
        {
            // If this is player 1, read the mouse
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }

        yaw   += mouseX;
        pitch -= mouseY;
        pitch  = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // --- Apply rotation ---
        // Yaw: rotate only the character root around world up
        characterRoot.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Camera rotation: purely from mouse input, ignoring any animation
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Recoil
        BaseWeapon currentWeapon = inventory.GetCurrentWeapon();
        if (currentWeapon != null)
        {
            accumulatedRecoil += currentWeapon.recoilRequest;
            currentWeapon.recoilRequest = Vector3.zero;   // Consumed
        }

        // Smooth recovery
        accumulatedRecoil = Vector3.Lerp(accumulatedRecoil, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);

        // Apply final rotation
        cameraTransform.rotation = Quaternion.Euler(
            pitch + accumulatedRecoil.x,
            yaw + accumulatedRecoil.y,
            0f
        );

        // --- Position: follow the head bone's world position, apply eye offset in head's local space ---
        if (headBone != null)
        {
            // Transform the local eye offset to world space using the head bone's rotation.
            Vector3 worldEyePosition = headBone.position + headBone.TransformDirection(eyeOffset);
            cameraTransform.position = worldEyePosition;
        }
        else
        {
            // Fallback: use root position + fixed height
            cameraTransform.position = characterRoot.position + Vector3.up * 1.7f;
        }
    }
}