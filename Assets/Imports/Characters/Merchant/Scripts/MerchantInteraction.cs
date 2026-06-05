using UnityEngine;

namespace Abdulrahman.NPC
{
    public class MerchantInteraction : MonoBehaviour
    {
        public float interactionDistance = 5f;
        public string playerTag = "Player";
        public string animatorParameter = "IsPlayerClose";
        public GameObject interactionUI;

        private Animator _animator;
        private Transform _player;
        private bool _isClose = false;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            if (interactionUI != null) interactionUI.SetActive(false);
            
            FindPlayer();
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                _player = playerObj.transform;
            }
        }

        private void Update()
        {
            if (_player == null)
            {
                FindPlayer();
                if (_player == null) return;
            }

            float distance = Vector3.Distance(transform.position, _player.position);
            _isClose = distance <= interactionDistance;

            if (_animator != null)
            {
                _animator.SetBool(animatorParameter, _isClose);
            }

            if (interactionUI != null)
            {
                interactionUI.SetActive(_isClose);
            }

            if (_isClose && Input.GetKeyDown(KeyCode.E))
            {
                ShopManager.Instance.OpenShop();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}
