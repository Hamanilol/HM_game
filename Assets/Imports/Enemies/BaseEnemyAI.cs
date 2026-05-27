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
                player = GameObject.FindWithTag("Player").transform;

            if (player != null)
                _playerHealth = player.GetComponent<PlayerHealth>();
        }

        protected virtual void Update()
        {
            if (_isDead) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            UpdateState(distanceToPlayer);
            HandleState(distanceToPlayer);
            UpdateAnimator();

            _attackTimer -= Time.deltaTime;
        }

        protected virtual void UpdateState(float distance)
        {
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
                    break;

                case AIState.Chase:
                    _agent.isStopped = false;
                    _agent.speed = distance > attackRange * 2 ? runSpeed : walkSpeed;
                    _agent.SetDestination(player.position);
                    break;

                case AIState.Attack:
                    _agent.isStopped = true;
                    transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
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
            if (_playerHealth == null) return;
            Vector3 knockbackDir = (player.position - transform.position).normalized;
            knockbackDir.y = 0.3f;
            _playerHealth.TakeDamage(damageAmount, knockbackDir);
        }

        public virtual void Die()
        {
            _isDead = true;
            _currentState = AIState.Dead;
            _agent.isStopped = true;
            _animator.SetBool("IsDead", true);
        }
    }
}