using UnityEngine;

namespace Abdulrahman.PlayerSystem
{
    public class PistolM1911 : Pistol
    {
[Header("M1911 Specific")]
        public GameObject bulletPrefab;
        public GameObject casingPrefab;
        public Transform casingExitLocation;
        public float shotPower = 500f;
        public float ejectPower = 150f;

        protected override void Start()
        {
            base.Start();
            weaponName = "M1911 Handgun";
        }

        protected override void Fire()
        {
            base.Fire();
            // Logic is now driven by BaseWeapon.Fire which calls PlayFireEffects (triggering animation)
            // Visuals are handled by OnAnimationShoot and OnAnimationCasingRelease
        }

        protected override void PlayFireEffects()
        {
            // We only trigger the animation here. 
            // Sound and Muzzle Flash are handled in the event hooks for sync.
            if (weaponAnimator != null)
                weaponAnimator.SetTrigger("Fire");
        }

        public override void OnAnimationShoot()
        {
            // Muzzle flash sync
            if (muzzleFlashPrefab != null && muzzlePoint != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
                Destroy(flash, 1f);
            }

            // Sound sync
            if (fireSound != null)
            {
                AudioSource.PlayClipAtPoint(fireSound, muzzlePoint != null ? muzzlePoint.position : transform.position);
            }

            // Bullet visual
            if (bulletPrefab != null && muzzlePoint != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Calculate direction towards the point we actually hit with the raycast
                    Vector3 bulletDir = (lastHitPoint - muzzlePoint.position).normalized;
                    if (bulletDir == Vector3.zero) bulletDir = muzzlePoint.forward;

                    // Align bullet with direction
                    bullet.transform.forward = bulletDir;

                    // Use linearVelocity for a snappy, consistent bullet path.
                    rb.linearVelocity = bulletDir * 350f;
                }
                // Increase lifetime slightly to ensure it can cover the full 100m range
                Destroy(bullet, 3f);
            }
        }

        public override void OnAnimationCasingRelease()
        {
            // Casing ejection logic from Nokobot SimpleShoot
            if (casingExitLocation != null && casingPrefab != null)
            {
                GameObject tempCasing = Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation);
                Rigidbody rb = tempCasing.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(Random.Range(ejectPower * 0.7f, ejectPower), 
                        (casingExitLocation.position - casingExitLocation.right * 0.3f - casingExitLocation.up * 0.6f), 1f);
                    rb.AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(100f, 1000f)), ForceMode.Impulse);
                }
                Destroy(tempCasing, 2f);
            }
        }
    }
}
