using UnityEngine;
using Abdulrahman.PlayerSystem;

namespace Abdulrahman.EnemySystem
{
    public class Projectile : MonoBehaviour
    {
        [Header("PROJECTILE SETTINGS")]
        public float speed = 10f;
        public float damage = 25f;
        public float knockbackForce = 15f;
        public float verticalKnockback = 2f;
        public float lifetime = 5f;
        public GameObject hitEffectPrefab;

        private Transform _target;
        private Vector3 _direction;
        private Rigidbody _rb;
        private bool _hasHit = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private Vector3 GetTargetPoint()
        {
            if (_target == null) return transform.position + transform.forward;

            Collider col = _target.GetComponent<Collider>();
            if (col != null)
            {
                return col.bounds.center;
            }
            return _target.position + Vector3.up * 0.5f;
        }

        private void Start()
        {
            gameObject.SetActive(true);
            
            // Ensure particle systems play
            var ps = GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            foreach (var childPs in GetComponentsInChildren<ParticleSystem>())
            {
                childPs.Play();
            }

            // Fallback for direction if Initialize wasn't called
            if (_direction == Vector3.zero)
            {
                if (_target == null)
                {
                    GameObject p = GameObject.FindWithTag("Player");
                    if (p != null) _target = p.transform;
                }

                if (_target != null)
                {
                    _direction = (GetTargetPoint() - transform.position).normalized;
                }
            }

            if (_rb != null && _direction != Vector3.zero)
            {
                _rb.linearVelocity = _direction * speed;
            }

            Destroy(gameObject, lifetime);
        }

        public void Initialize(Transform target)
        {
            if (target == null) return;
            _target = target;
            _direction = (GetTargetPoint() - transform.position).normalized;
            
            // Align projectile with direction
            if (_direction != Vector3.zero)
            {
                transform.forward = _direction;
            }

            if (_rb != null && _direction != Vector3.zero)
            {
                _rb.linearVelocity = _direction * speed;
            }
        }

        private void FixedUpdate()
        {
            if (_hasHit) return;

            // Manual check for collision
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.8f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    HandleHit(hitCollider);
                    break;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit) return;

            if (other.CompareTag("Player"))
            {
                HandleHit(other);
            }
            else if (!other.CompareTag("Enemy") && !other.isTrigger)
            {
                // Hit a wall or obstacle
                _hasHit = true;
                Debug.Log("[Projectile] Hit Obstacle: " + other.gameObject.name);
                
                if (hitEffectPrefab != null)
                {
                    GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                    effect.SetActive(true);
                    Destroy(effect, 2f);
                }
                Destroy(gameObject);
            }
        }

        private void HandleHit(Collider other)
        {
            if (_hasHit) return;
            if (other == null) return;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null) playerHealth = other.GetComponentInParent<PlayerHealth>();
            
            if (playerHealth == null) return;

            _hasHit = true;
            Debug.Log("[Projectile] Hit Player: " + other.gameObject.name);

            // Use projectile movement direction for knockback
            Vector3 knockbackDir = _direction;
            if (knockbackDir == Vector3.zero) knockbackDir = (other.transform.position - transform.position).normalized;
            
            knockbackDir.y = 0;
            knockbackDir.Normalize();
            
            // Apply dramatic vertical lift and magnitude for boss projectile
            knockbackDir.y = verticalKnockback * 2f; 
            knockbackDir *= knockbackForce;

            playerHealth.TakeDamage(damage, knockbackDir);

            if (hitEffectPrefab != null)
            {
                Vector3 effectPos = other.transform.position; 
                GameObject effect = Instantiate(hitEffectPrefab, effectPos, Quaternion.identity);
                if (effect != null)
                {
                    effect.SetActive(true);
                    Destroy(effect, 2f);
                }
            }

            Destroy(gameObject);
        }
    }
}