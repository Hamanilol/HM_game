using UnityEngine;

namespace Abdulrahman.NPC
{
    public class MerchantInteraction : MonoBehaviour
    {
        public float interactionDistance = 5f;
        public string playerTag = "Player";
        public string animatorParameter = "IsPlayerClose";
        public GameObject interactionUI;

        [Header("Store")]
        [Tooltip("The StoreUI to open when the player presses E nearby. If left empty it will be found automatically.")]
        public StoreUI storeUI;
        public KeyCode interactKey = KeyCode.E;

        private Animator _animator;
        private Transform _player;
        private bool _isClose = false;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            if (interactionUI != null) interactionUI.SetActive(false);

            if (storeUI == null) storeUI = FindFirstObjectByType<StoreUI>(FindObjectsInactive.Include);

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

            bool storeOpen = storeUI != null && storeUI.storePanel != null && storeUI.storePanel.activeSelf;

            if (interactionUI != null)
            {
                interactionUI.SetActive(_isClose && !storeOpen);
            }

            if (_isClose && Input.GetKeyDown(interactKey))
            {
                if (storeUI != null)
                {
                    storeUI.Open();
                }
                else if (ShopManager.Instance != null)
                {
                    ShopManager.Instance.OpenShop();
                }
                else
                {
                    Debug.LogWarning("[MerchantInteraction] No StoreUI or ShopManager available to open.");
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}
