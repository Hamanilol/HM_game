using UnityEngine;

namespace Abdulrahman.PlayerSystem
{
    public class WeaponAnimationEventBridge : MonoBehaviour
    {
        private BaseWeapon _weapon;

        private void Awake()
        {
            _weapon = GetComponentInParent<BaseWeapon>();
        }

        public void Shoot()
        {
            if (_weapon != null) _weapon.OnAnimationShoot();
        }

        public void CasingRelease()
        {
            if (_weapon != null) _weapon.OnAnimationCasingRelease();
        }
    }
}