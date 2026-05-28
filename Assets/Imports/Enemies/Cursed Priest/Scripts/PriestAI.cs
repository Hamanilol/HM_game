using UnityEngine;

namespace Abdulrahman.EnemySystem
{
    public class PriestAI : BaseEnemyAI
    {
        [Header("PRIEST SETTINGS")]
        public float meleeRange = 2f;
        public float rangedRange = 10f;
        public GameObject projectilePrefab;
        public Transform projectileSpawnPoint;

        protected override void Start()
        {
            attackRange = meleeRange;
            attackRange2 = rangedRange;
            base.Start();
        }

        protected override void HandleAttack(float distance)
        {
            if (_isAttacking)
            {
                AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                bool inAttack = stateInfo.IsName("MeleeAttack") || stateInfo.IsName("RangedAttack");
                if (!inAttack) _isAttacking = false;
            }

            if (_attackTimer <= 0f && !_isAttacking)
            {
                _attackTimer = attackCooldown;
                _isAttacking = true;

                Debug.Log("Distance: " + distance + " meleeRange: " + meleeRange + " rangedRange: " + rangedRange);

                if (distance <= meleeRange)
                {
                    Debug.Log("Triggering Melee");
                    _animator.SetTrigger("MeleeAttackTrigger");
                }
                else if (distance <= rangedRange)
                {
                    Debug.Log("Triggering Ranged");
                    _animator.SetTrigger("RangedAttackTrigger");
                }
            }
        }

        protected override void UpdateAnimator()
        {
            base.UpdateAnimator();

            Vector3 localVelocity = transform.InverseTransformDirection(_agent.velocity);
            _animator.SetFloat("DirectionX", localVelocity.x, 0.1f, Time.deltaTime);
            _animator.SetFloat("DirectionY", localVelocity.z, 0.1f, Time.deltaTime);
        }

        public void ThrowProjectile()
        {
            if (projectilePrefab == null || projectileSpawnPoint == null) return;
 
            GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            proj.SetActive(true);
            Projectile projectile = proj.GetComponent<Projectile>();
if (projectile != null)
                projectile.Initialize(player);
        }

        private void OnDestroy()
        {
            Debug.Log("Priest was destroyed!");
        }
    }
}