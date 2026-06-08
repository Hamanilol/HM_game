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
            // Ensure ranges are initialized from priest-specific settings
            attackRange = meleeRange;
            attackRange2 = rangedRange;
            base.Start();
        }

        protected override void HandleAttack(float distance)
        {
            // The reset logic for _isAttacking is moved to UpdateAnimator to ensure it runs even if state changes
            if (_attackTimer <= 0f && !_isAttacking)
            {
                if (distance <= meleeRange)
                {
                    _attackTimer = attackCooldown;
                    _isAttacking = true;
                    Debug.Log("[PriestAI] Triggering Melee Attack (Distance: " + distance + ")");
                    _animator.SetTrigger("MeleeAttackTrigger");
                }
                else if (distance <= rangedRange)
                {
                    _attackTimer = attackCooldown;
                    _isAttacking = true;
                    Debug.Log("[PriestAI] Triggering Ranged Attack (Distance: " + distance + ")");
                    _animator.SetTrigger("RangedAttackTrigger");
                }
            }
        }

        protected override void UpdateAnimator()
        {
            base.UpdateAnimator();

            if (_isAttacking)
            {
                AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                bool inAttack = stateInfo.IsName("MeleeAttack") || stateInfo.IsName("RA");
                bool inTransition = _animator.IsInTransition(0);

                // Only reset if we are NOT in an attack state AND not transitioning into one
                // This ensures the animation plays fully before allowing another attack trigger
                if (!inAttack && !inTransition)
                {
                    _isAttacking = false;
                    // Debug.Log("[PriestAI] Attack animation finished, ready for next.");
                }
            }

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