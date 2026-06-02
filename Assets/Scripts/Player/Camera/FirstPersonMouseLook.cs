using UnityEngine;

public class FirstPersonMouseLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 2f;

    [Header("References")]
    public Transform characterRoot; // The transform that rotates for yaw
    public Transform headBone;      // Direct reference, or assign via code

    [Header("Clamp")]
    public float minPitch = -85f;
    public float maxPitch = 85f;

    private float pitch = 0f; // current vertical angle

    void Start()
    {
        // If you already have the Animator, fetch the head bone automatically
        if (headBone == null)
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null)
                headBone = anim.GetBoneTransform(HumanBodyBones.Head);
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Yaw – rotate the whole character around the world up axis
        characterRoot.Rotate(Vector3.up * mouseX);

        // Pitch – rotate only the head bone locally (up/down)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        // Apply local rotation: we set the local rotation directly to avoid accumulation issues
        // Note: we need to combine with any existing head bone animation (if any).
        // For a static head, this is fine:
        if (headBone != null)
        {
            // We'll use localEulerAngles, but be careful with axis order.
            // A robust way is to store the desired pitch and rebuild the head's local rotation.
            Quaternion targetRotation = Quaternion.Euler(pitch, 0f, 0f);
            headBone.localRotation = targetRotation;
        }
    }
}