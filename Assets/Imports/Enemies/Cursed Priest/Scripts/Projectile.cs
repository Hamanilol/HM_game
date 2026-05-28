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
        public float verticalKnockback = 5f;
        public float lifetime = 5f;
public GameObject hitEffectPrefab;

        private Transform _target;
        private Vector3 _direction;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            gameObject.SetActive(true);
            
            // Ensure particle systems play if this was instantiated from an inactive prefab
            var ps = GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            foreach (var childPs in GetComponentsInChildren<ParticleSystem>())
            {
                childPs.Play();
            }

            if (_rb != null)
            {
                _rb.linearVelocity = _direction * speed;
            }

            Destroy(gameObject, lifetime);
        }

        public void Initialize(Transform target)
        {
            _target = target;
            _direction = (target.position + Vector3.up * 1f - transform.position).normalized;
            
            if (_rb != null)
            {
                _rb.linearVelocity = _direction * speed;
            }
        }

        private void Update()
        {
            // Velocity is handled by Rigidbody
        }

        private void FixedUpdate()
        {
            // Manual check to ensure we don't miss the player
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.7f);
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
            if (other.CompareTag("Player"))
            {
                HandleHit(other);
            }
            else
            {
                // Debug.Log("[Projectile] Trigger with non-player: " + other.gameObject.name);
            }
        }

        private bool _hasHit = false;

        private void HandleHit(Collider other)
        {
            if (_hasHit) return;
            _hasHit = true;

            Debug.Log("[Projectile] Hit Player: " + other.gameObject.name);

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null) playerHealth = other.GetComponentInParent<PlayerHealth>();
            
            if (playerHealth == null) 
            {
                _hasHit = false; // Reset so it can try hitting another part if this one lacks health script
                return;
            }

            Vector3 knockbackDir = (_target.position - transform.position).normalized;
            knockbackDir.y = verticalKnockback / 10f;
            knockbackDir *= knockbackForce / 10f;

            playerHealth.TakeDamage(damage, knockbackDir);

            if (hitEffectPrefab != null)
            {
                Vector3 effectPos = other.transform.position; 
                GameObject effect = Instantiate(hitEffectPrefab, effectPos, Quaternion.identity);
                effect.SetActive(true);
                Destroy(effect, 2f);
            }

            Destroy(gameObject);
        }
    }
}