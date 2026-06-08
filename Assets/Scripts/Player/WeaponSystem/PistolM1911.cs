using UnityEngine;

namespace Abdulrahman.PlayerSystem
{
    public class PistolM1911 : Pistol
    {
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
            SpawnVisualBullet();
        }

        public override void OnAnimationCasingRelease()
        {
            // Casing ejection logic from Nokobot SimpleShoot
            EjectCasing();
        }
    }
}
