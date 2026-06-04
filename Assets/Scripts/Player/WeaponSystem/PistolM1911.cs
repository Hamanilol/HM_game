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

            // Bullet visual (optional, since PerformRaycast handles actual damage)
            if (bulletPrefab != null && muzzlePoint != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(muzzlePoint.forward * shotPower);
                }
                Destroy(bullet, 2f);
            }
        }
    }
}
