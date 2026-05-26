using UnityEngine;
using UnityEngine.AI;
using Abdulrahman.PlayerSystem;

namespace Abdulrahman.EnemySystem
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class ZombieAI : MonoBehaviour
    {
        [Header("REFERENCES")]
        public Transform player;
        private NavMeshAgent _agent;
        private Animator _animator;

        [Header("DETECTION")]
        public float detectionRange = 15f;
        public float attackRange1 = 2f;
        public float attackRange2 = 4f;

        [Header("MOVEMENT")]
        public float walkSpeed = 3.5f;
        public float runSpeed = 7f;

        [Header("ATTACK")]
        public float attackCooldown = 1.5f;
        private float _attackTimer = 0f;
        private bool _isAttacking = false;

        private bool _isDead = false;
        private PlayerHealth _playerHealth;


        private enum AIState { Idle, Chase, Attack, Dead }
        private AIState _currentState = AIState.Idle;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            if (player == null)
                player = GameObject.FindWithTag("Player").transform;
                _playerHealth = player.GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            if (_isDead) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            UpdateState(distanceToPlayer);
            HandleState(distanceToPlayer);
            UpdateAnimator();

            _attackTimer -= Time.deltaTime;
        }

        private void UpdateState(float distance)
        {
            if (distance <= attackRange1 || distance <= attackRange2)
                _currentState = AIState.Attack;
            else if (distance <= detectionRange)
                _currentState = AIState.Chase;
            else
                _currentState = AIState.Idle;
        }
        public void DealDamageToPlayer()
        {
            if (_playerHealth == null) return;

            Vector3 knockbackDir = (player.position - transform.position).normalized;
            knockbackDir.y = 0.3f;
            _playerHealth.TakeDamage(5f, knockbackDir);
        }
        

        private void HandleState(float distance)
        {
            switch (_currentState)
            {
                case AIState.Idle:
                    _agent.isStopped = true;
                    break;

                case AIState.Chase:
                    _agent.isStopped = false;
                    _agent.speed = distance > attackRange2 * 2 ? runSpeed : walkSpeed;
                    _agent.SetDestination(player.position);
                    break;

                case AIState.Attack:
                    _agent.isStopped = true;
                    transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

                    if (_attackTimer <= 0f && !_isAttacking)
                    {
                        _attackTimer = attackCooldown;
                        _isAttacking = true;

                        int randomAttack = Random.Range(0, 2);
                        if (randomAttack == 0)
                        _animator.SetTrigger("Attack1Trigger");
                    else
                        _animator.SetTrigger("Attack2Trigger");
                }
                    break;
            }
        }

        private void UpdateAnimator()
        {
            _animator.SetFloat("Speed", _agent.velocity.magnitude, 0.1f, Time.deltaTime);
            _animator.SetBool("IsDead", _isDead);

            if (_isAttacking)
            {
                AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                bool attackDone = (stateInfo.IsName("Attack1") || stateInfo.IsName("Attack2"))
                                  && stateInfo.normalizedTime >= 0.9f;

                if (attackDone)
                    _isAttacking = false;
            }
        }

        public void Die()
        {
            _isDead = true;
            _currentState = AIState.Dead;
            _agent.isStopped = true;
            _animator.SetBool("IsDead", true);
        }
    }
}