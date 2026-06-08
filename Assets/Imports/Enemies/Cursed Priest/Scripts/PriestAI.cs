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
            
            if (player == null)
            {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) player = p.transform;
            }
        }

        protected override void HandleAttack(float distance)
        {
            if (player == null)
            {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) player = p.transform;
                if (player == null) return;
            }

            // The reset logic for _isAttacking is moved to UpdateAnimator to ensure it runs even if state changes
            if (_attackTimer <= 0f && !_isAttacking)
            {
                if (distance <= meleeRange)
                {
                    _attackTimer = attackCooldown;
                    _isAttacking = true;
                    _animator.SetTrigger("MeleeAttackTrigger");
                }
                else if (distance <= rangedRange)
                {
                    _attackTimer = attackCooldown;
                    _isAttacking = true;
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

        public void ThrowProjectile_Old()
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
            public void ThrowProjectile()
        {
            Debug.Log("[PriestAI] ThrowProjectile Animation Event Received!");
            if (projectilePrefab == null || projectileSpawnPoint == null) 
            {
                Debug.LogError("[PriestAI] Missing projectilePrefab or projectileSpawnPoint");
                return;
            }
 
            GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            proj.SetActive(true);
            Projectile projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(player);
                Debug.Log("[PriestAI] Projectile initialized towards player");
            }
        }

}
}