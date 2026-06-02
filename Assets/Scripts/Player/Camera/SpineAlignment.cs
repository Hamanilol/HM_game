using UnityEngine;

public class SpineAlignment : MonoBehaviour
{
    public Animator characterAnimator;
    public Transform cameraTransform;    // Your stable FPS camera

    private Transform spineBone;

    void Start()
    {
        if (characterAnimator == null)
            characterAnimator = GetComponent<Animator>();
        spineBone = characterAnimator.GetBoneTransform(HumanBodyBones.Spine);
        if (spineBone == null)
            Debug.LogError("Spine bone not found on humanoid rig.");
    }

    void LateUpdate()
    {
        if (spineBone == null || cameraTransform == null) return;

        // Get the yaw (horizontal) rotation from the camera
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f; // Keep only horizontal direction
        if (cameraForward.sqrMagnitude < 0.01f) return;

        Quaternion targetSpineRotation = Quaternion.LookRotation(cameraForward, Vector3.up);

        // Apply only the yaw to the spine, preserving any pitch from the animation
        spineBone.rotation = targetSpineRotation;
    }
}