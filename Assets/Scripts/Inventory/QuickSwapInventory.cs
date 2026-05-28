using UnityEngine;
using System.Collections.Generic;

namespace Abdulrahman.InventorySystem
{
    public class QuickSwapInventory : MonoBehaviour
    {
        [Header("Settings")]
        public Transform hand;
        public int slotCount = 5;
        
        [Header("Items")]
        public List<GameObject> itemPrefabs = new List<GameObject>();
        
        private int _currentSlotIndex = -1;
        private GameObject _currentItemInstance;
        private List<GameObject> _slots = new List<GameObject>();

        private void Start()
        {
            // Initialize slots
            for (int i = 0; i < slotCount; i++)
            {
                if (i < itemPrefabs.Count)
                    _slots.Add(itemPrefabs[i]);
                else
                    _slots.Add(null);
            }

            // Default to first slot if possible
            SwapToSlot(0);
        }

        private void Update()
        {
            HandleInput();
            HandleScroll();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwapToSlot(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwapToSlot(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwapToSlot(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SwapToSlot(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SwapToSlot(4);
            if (Input.GetKeyDown(KeyCode.Alpha6)) SwapToSlot(5);
            if (Input.GetKeyDown(KeyCode.Alpha7)) SwapToSlot(6);
            if (Input.GetKeyDown(KeyCode.Alpha8)) SwapToSlot(7);
            if (Input.GetKeyDown(KeyCode.Alpha9)) SwapToSlot(8);
        }

        private void HandleScroll()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                int next = (_currentSlotIndex + 1) % slotCount;
                SwapToSlot(next);
            }
            else if (scroll < 0f)
            {
                int prev = (_currentSlotIndex - 1 + slotCount) % slotCount;
                SwapToSlot(prev);
            }
        }

        public void SwapToSlot(int index)
        {
            if (index < 0 || index >= _slots.Count) return;
            if (index == _currentSlotIndex) return;

            _currentSlotIndex = index;
            EquipItem(_slots[_currentSlotIndex]);
            
            Debug.Log($"Swapped to slot {index + 1}");
        }

        private void EquipItem(GameObject prefab)
        {
            if (_currentItemInstance != null)
            {
                Destroy(_currentItemInstance);
            }

            if (prefab != null && hand != null)
            {
                _currentItemInstance = Instantiate(prefab, hand);
                _currentItemInstance.transform.localPosition = Vector3.zero;
                _currentItemInstance.transform.localRotation = Quaternion.identity;
            }
        }

        public int GetCurrentSlot() => _currentSlotIndex;
    }
}
