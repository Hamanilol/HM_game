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
        public float lifetime = 5f;

        private Transform _target;
        private Vector3 _direction;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        public void Initialize(Transform target)
        {
            _target = target;
            _direction = (target.position + Vector3.up * 1f - transform.position).normalized;
        }

        private void Update()
        {
            transform.position += _direction * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null) return;

            Vector3 knockbackDir = (_target.position - transform.position).normalized;
            knockbackDir.y = 0.4f;
            knockbackDir *= knockbackForce / 10f;

            playerHealth.TakeDamage(damage, knockbackDir);
            Destroy(gameObject);
        }
    }
}