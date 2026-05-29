using UnityEngine;

public class StableFirstPersonCamera : MonoBehaviour
{
    [Header("Sensitivity")]
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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        headBone = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
        if (headBone == null)
            Debug.LogError("Head bone not found!");

        // Initialize yaw from current character rotation
        yaw = characterRoot.eulerAngles.y;
    }

    void LateUpdate()
    {
        // --- Mouse input ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw   += mouseX;
        pitch -= mouseY;
        pitch  = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // --- Apply rotation ---
        // Yaw: rotate only the character root around world up
        characterRoot.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Camera rotation: purely from mouse input, ignoring any animation
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0f);

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