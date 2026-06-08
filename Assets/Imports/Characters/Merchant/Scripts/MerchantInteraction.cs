using UnityEngine;
using System.Collections.Generic;
using Abdulrahman.PlayerSystem;

namespace Abdulrahman.NPC
{
    public class MerchantInteraction : MonoBehaviour
    {
        public float interactionDistance = 5f;
        public string animatorParameter = "IsPlayerClose";

        [Header("Store")]
        public StoreUI storeUI;

        private Animator _animator;
        private PlayerController[] _players;
        private bool _anyoneClose = false;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            if (storeUI == null) storeUI = FindFirstObjectByType<StoreUI>(FindObjectsInactive.Include);
            
            RefreshPlayers();
        }

        private void RefreshPlayers()
        {
            _players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        }

        private void Update()
        {
            if (_players == null || _players.Length == 0)
            {
                RefreshPlayers();
                if (_players == null || _players.Length == 0) return;
            }

            _anyoneClose = false;

            foreach (var player in _players)
            {
                if (player == null) continue;

                float distance = Vector3.Distance(transform.position, player.transform.position);
                bool isClose = distance <= interactionDistance;

                if (isClose) _anyoneClose = true;

                // Handle player-specific prompt
                if (player.interactPrompt != null)
                {
                    // Hide prompt if store is already open (assuming shared store for now, or check per player if storeUI supports it)
                    bool storeOpen = storeUI != null && storeUI.storePanel != null && storeUI.storePanel.activeSelf;
                    player.interactPrompt.SetActive(isClose && !storeOpen);
                }

                // Handle interaction
                if (isClose && player.InteractInput)
                {
                    if (storeUI != null)
                    {
                        storeUI.Open();
                    }
                    else if (ShopManager.Instance != null)
                    {
                        ShopManager.Instance.OpenShop(player);
                    }
                }
}

            if (_animator != null)
            {
                _animator.SetBool(animatorParameter, _anyoneClose);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}
