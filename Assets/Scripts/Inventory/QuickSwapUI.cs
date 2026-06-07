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

        [Header("Slot Background")]
        public Color selectedColor = new Color(1f, 0.3f, 0f, 1f);
        public Color normalColor = new Color(0.02f, 0f, 0f, 1f);

        [Header("Slot Border (GothicBorder)")]
        public Color selectedBorderColor = new Color(1f, 0.85f, 0.2f, 1f);
        public Color normalBorderColor = new Color(0.35f, 0.1f, 0.1f, 0.6f);

        [Header("Slot Label")]
        public Color selectedLabelColor = Color.white;
        public Color normalLabelColor = new Color(0.7f, 0.6f, 0.55f, 1f);

        [Header("Animation")]
        [Tooltip("Scale applied to the currently selected slot.")]
        public float selectedScale = 1.2f;
        [Tooltip("How quickly slots animate towards their target highlight state.")]
        public float animationSpeed = 12f;

        // Per-slot cached references so we can animate every visual element of a slot.
        private class SlotVisual
        {
            public RectTransform rect;
            public Image background;
            public Image border;
            public TMPro.TextMeshProUGUI label;
        }

        private readonly List<SlotVisual> _slots = new List<SlotVisual>();
        private int _selectedIndex = -1;

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

            _slots.Clear();

            for (int i = 0; i < inventory.slotCount; i++)
            {
                GameObject slot = Instantiate(slotPrefab, slotContainer);
                slot.name = $"Slot_{i + 1}";
                slot.SetActive(true); // Ensure spawned slots are active

                var visual = new SlotVisual
                {
                    rect = slot.GetComponent<RectTransform>(),
                    background = slot.GetComponent<UnityEngine.UI.Image>()
                };

                // The border is a child Image named "GothicBorder" in the slot prefab.
                Transform borderTf = slot.transform.Find("GothicBorder");
                if (borderTf != null) visual.border = borderTf.GetComponent<UnityEngine.UI.Image>();

                visual.label = slot.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (visual.label != null) visual.label.text = (i + 1).ToString();

                _slots.Add(visual);
            }

            // Snap to the inventory's current selection immediately (no first-frame pop).
            _selectedIndex = inventory.GetCurrentSlot();
            ApplyImmediate();
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

            _selectedIndex = inventory.GetCurrentSlot();
            AnimateSelection();
        }

        // Smoothly drive every slot toward its target highlight state each frame.
        private void AnimateSelection()
        {
            float t = 1f - Mathf.Exp(-animationSpeed * Time.unscaledDeltaTime);

            for (int i = 0; i < _slots.Count; i++)
            {
                bool isSelected = i == _selectedIndex;
                SlotVisual s = _slots[i];

                Vector3 targetScale = isSelected ? Vector3.one * selectedScale : Vector3.one;
                if (s.rect != null)
                    s.rect.localScale = Vector3.Lerp(s.rect.localScale, targetScale, t);

                if (s.background != null)
                    s.background.color = Color.Lerp(s.background.color, isSelected ? selectedColor : normalColor, t);

                if (s.border != null)
                    s.border.color = Color.Lerp(s.border.color, isSelected ? selectedBorderColor : normalBorderColor, t);

                if (s.label != null)
                    s.label.color = Color.Lerp(s.label.color, isSelected ? selectedLabelColor : normalLabelColor, t);
            }
        }

        // Set the final highlight state instantly (used on (re)initialize).
        private void ApplyImmediate()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                bool isSelected = i == _selectedIndex;
                SlotVisual s = _slots[i];

                if (s.rect != null) s.rect.localScale = isSelected ? Vector3.one * selectedScale : Vector3.one;
                if (s.background != null) s.background.color = isSelected ? selectedColor : normalColor;
                if (s.border != null) s.border.color = isSelected ? selectedBorderColor : normalBorderColor;
                if (s.label != null) s.label.color = isSelected ? selectedLabelColor : normalLabelColor;
            }
        }
    }
}
