using UnityEngine;
using UnityEngine.AI;
using Abdulrahman.PlayerSystem;

namespace Abdulrahman.EnemySystem
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public abstract class BaseEnemyAI : MonoBehaviour
    {
        [Header("REFERENCES")]
        public Transform player;
        protected NavMeshAgent _agent;
        protected Animator _animator;
        protected PlayerHealth _playerHealth;

        [Header("DETECTION")]
        public float detectionRange = 15f;
        public float attackRange = 2f;
        public float attackRange2 = 0f;

        [Header("MOVEMENT")]
        public float walkSpeed = 3.5f;
        public float runSpeed = 7f;

        [Header("ATTACK")]
        public float attackCooldown = 1.5f;
        public float damageAmount = 5f;
        public float knockbackForce = 5f;
        public float verticalKnockback = 0.5f;
        protected float _attackTimer = 0f;
protected bool _isAttacking = false;

        protected bool _isDead = false;

        protected enum AIState { Idle, Chase, Attack, Dead }
        protected AIState _currentState = AIState.Idle;

        protected virtual void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            if (player == null)
            {
                var healths = Object.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
                if (healths.Length > 0)
                {
                    player = healths[0].transform;
                }
                else
                {
                    GameObject pObj = GameObject.FindWithTag("Player");
                    if (pObj != null) player = pObj.transform;
                }
            }

            if (player != null)
                _playerHealth = player.GetComponent<PlayerHealth>();
            
            if (_playerHealth == null && player != null)
                _playerHealth = player.GetComponentInParent<PlayerHealth>();
        }

        protected virtual void Update()
        {
            if (_isDead) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            UpdateState(distanceToPlayer);
            HandleState(distanceToPlayer);
            
            // Ensure we face the player during any attack sequence
            if (_isAttacking && player != null)
            {
                FaceTarget(player.position);
            }

            UpdateAnimator();
            Debug.Log("State: " + _currentState + " Distance: " + distanceToPlayer);
            _attackTimer -= Time.deltaTime;
        }

        protected virtual void FaceTarget(Vector3 targetPos)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }

        protected virtual void UpdateState(float distance)
        {
            // Don't switch states while in the middle of an attack animation
            if (_isAttacking)
            {
                _currentState = AIState.Attack;
                return;
            }

            bool inAttackRange = distance <= attackRange || (attackRange2 > 0f && distance <= attackRange2);

            if (inAttackRange)
                _currentState = AIState.Attack;
            else if (distance <= detectionRange)
                _currentState = AIState.Chase;
            else
                _currentState = AIState.Idle;
        }

        protected virtual void HandleState(float distance)
        {
            switch (_currentState)
            {
                case AIState.Idle:
                    _agent.isStopped = true;
                    _agent.updateRotation = true;
                    break;

                case AIState.Chase:
                    _agent.isStopped = false;
                    _agent.updateRotation = true;
                    _agent.speed = distance > attackRange * 2 ? runSpeed : walkSpeed;
                    _agent.SetDestination(player.position);
                    break;

                case AIState.Attack:
                    _agent.isStopped = true;
                    // Disable agent auto-rotation so we can rotate manually towards player
                    _agent.updateRotation = false; 
                    HandleAttack(distance);
                    break;
            }
        }

        protected abstract void HandleAttack(float distance);

        protected virtual void UpdateAnimator()
        {
            _animator.SetFloat("Speed", _agent.velocity.magnitude, 0.1f, Time.deltaTime);
            _animator.SetBool("IsDead", _isDead);
        }

        public virtual void DealDamageToPlayer()
        {
            if (_playerHealth == null)
            {
                Debug.LogWarning("[BaseEnemyAI] Cannot deal damage: _playerHealth is null!");
                return;
            }
            
            Debug.Log($"[BaseEnemyAI] Dealing {damageAmount} damage to player.");
            
            // Calculate direction away from enemy
            Vector3 knockbackDir = (player.position - transform.position);
            knockbackDir.y = 0; // Flatten for horizontal direction
            if (knockbackDir.magnitude < 0.1f) knockbackDir = transform.forward; // Fallback
            
            knockbackDir.Normalize();
            // Bosses or heavy hitters should have significant vertical knockback (e.g. 5.0+)
            knockbackDir.y = verticalKnockback * 2f; 
            knockbackDir *= knockbackForce;
            
            _playerHealth.TakeDamage(damageAmount, knockbackDir);
        }

        public virtual void Die()
        {
            if (_isDead) return;
            _isDead = true;
            _currentState = AIState.Dead;
            
            if (_agent != null && _agent.isOnNavMesh)
                _agent.isStopped = true;
                
            if (_animator != null)
                _animator.SetBool("IsDead", true);

            // Notify spawner if possible (optional, the notifier script still exists for spawned enemies)
            // But for scene enemies, we need to ensure they are destroyed.
            Destroy(gameObject, 5f);
        }
    }
}