using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Abdulrahman.InventorySystem
{
    public class QuickSwapUI : MonoBehaviour
    {
        public QuickSwapInventory inventory;
        public GameObject slotPrefab;
        public Transform slotContainer;
        
        public Color selectedColor = Color.yellow;
        public Color normalColor = Color.white;

        private List<Image> _slotImages = new List<Image>();
        private int _lastIndex = -1;

        private void Start()
        {
            if (inventory == null) inventory = FindObjectOfType<QuickSwapInventory>();
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (inventory == null) return;

            // Clear container except the prefab itself if it's a child
            foreach (Transform child in slotContainer)
            {
                if (child.gameObject == slotPrefab) continue;
                Destroy(child.gameObject);
            }

            _slotImages.Clear();

            for (int i = 0; i < inventory.slotCount; i++)
            {
                GameObject slot = Instantiate(slotPrefab, slotContainer);
                slot.name = $"Slot_{i + 1}";
                slot.SetActive(true); // Ensure spawned slots are active
                
                UnityEngine.UI.Image img = slot.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    _slotImages.Add(img);
                    img.color = normalColor;
                }
                
                // Set text if exists
                var text = slot.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null) text.text = (i + 1).ToString();
            }
        }

        private void Update()
        {
            if (inventory == null || !inventory.gameObject.activeInHierarchy)
            {
                inventory = FindFirstObjectByType<QuickSwapInventory>();
                
                if (inventory == null)
                {
                    // Hide UI if no inventory found
                    if (slotContainer.gameObject.activeSelf) slotContainer.gameObject.SetActive(false);
                    return;
                }
                
                // Show UI and Re-initialize if the inventory changed
                slotContainer.gameObject.SetActive(true);
                InitializeUI();
            }

            int currentIndex = inventory.GetCurrentSlot();
            if (currentIndex != _lastIndex)
            {
                UpdateSelection(currentIndex);
                _lastIndex = currentIndex;
            }
        }

        private void UpdateSelection(int index)
        {
            for (int i = 0; i < _slotImages.Count; i++)
            {
                if (i == index)
                    _slotImages[i].color = selectedColor;
                else
                    _slotImages[i].color = normalColor;
            }
        }
    }
}
