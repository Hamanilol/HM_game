using UnityEngine;

public class WeaponAlignment : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform muzzlePoint;   // optional, determines which axis is forward

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Align the weapon's muzzle direction to the camera forward
        Vector3 camForward = cameraTransform.forward;
        Vector3 camUp = cameraTransform.up;

        if (muzzlePoint != null)
        {
            // Use the muzzle to define the weapon's forward axis
            Vector3 weaponForward = muzzlePoint.forward;
            Quaternion correction = Quaternion.FromToRotation(weaponForward, camForward);
            transform.rotation = correction * transform.rotation;

            // Also ensure the weapon's up aligns with camera up
            Vector3 weaponUp = transform.up;
            Quaternion upCorrection = Quaternion.FromToRotation(weaponUp, camUp);
            transform.rotation = upCorrection * transform.rotation;
        }
        else
        {
            // Fallback: just point the weapon's Z-axis to camera forward
            transform.rotation = Quaternion.LookRotation(camForward, camUp);
        }
    }
}