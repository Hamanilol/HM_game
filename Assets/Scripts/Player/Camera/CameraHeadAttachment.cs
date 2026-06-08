using UnityEngine;

public class CameraHeadAttachment : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;        // your first‑person camera
    public Animator characterAnimator; // the Animator on your character

    [Header("Eye Offset")]
    [Tooltip("Local position relative to the head bone. X=right, Y=up, Z=forward.")]
    public Vector3 eyeOffset = new Vector3(0f, 0.08f, 0.05f);

    void Start()
    {
        AttachCameraToHead();
    }

    void AttachCameraToHead()
    {
        if (characterAnimator == null)
        {
            Debug.LogWarning("Character Animator not assigned on " + gameObject.name + ". Camera attachment skipped.", this);
            return;
        }

        Transform headBone = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
        if (headBone == null)
        {
            Debug.LogWarning("Head bone not found on " + characterAnimator.name + "! Make sure the avatar is Humanoid and properly configured. Camera will remain at its default position.", this);
            return;
        }

        // Now set the local offset so the camera sits exactly at the desired eye position.
        playerCamera.transform.localPosition = eyeOffset;
        // Reset local rotation so it looks forward with the head.
        playerCamera.transform.localRotation = Quaternion.identity;
    }

    // Optional: draw the eye position in the Scene view for fine‑tuning
    void OnDrawGizmosSelected()
    {
        if (characterAnimator == null) return;
        Transform head = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
        if (head != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(head.TransformPoint(eyeOffset), 0.01f);
        }
    }
}