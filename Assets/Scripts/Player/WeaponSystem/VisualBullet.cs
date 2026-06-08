using UnityEngine;

namespace Abdulrahman.PlayerSystem
{
    public class VisualBullet : MonoBehaviour
    {
        public GameObject hitEffectPrefab;
        private bool _hasHit = false;

        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit) return;
            if (other.isTrigger) return; // Don't hit other triggers
            if (other.CompareTag("Player")) return; // Don't hit self

            _hasHit = true;
            Debug.Log($"[VisualBullet] Hit: {other.gameObject.name} (Tag: {other.tag})");
            
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}