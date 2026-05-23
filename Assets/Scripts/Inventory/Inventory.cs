// using NUnit.Framework;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class Inventory : MonoBehaviour
// {
//     public ItemSO flashItem;
//     public ItemSO GunItem;

//     public GameObject hotbarObj;
//     public GameObject inventorySlotParent;
//     public GameObject container;

//     public Image dragIcon;

//     public float pickupRange = 2f;
//     private Item lookedAtItem = null;
//     public Material highlightMaterial;
//     private Material originalMaterial;
//     private Renderer lookedAtRenderer = null;

//     private int equippedHotbarIndex = 0; //0-5
//     public float equippedOpacity = 0.9f;
//     public float normalOpacity = 0.58f;
//     public Transform hand;
//     private GameObject currentHandItem;
//     private List<Slot> hotbarSlots = new List<Slot>();
//     private List<Slot> inventorySlots = new List<Slot>();
//     private List<Slot> allSlots = new List<Slot>();

//     private Slot draggedSlot = null;
//     private bool isDragging = false;

//     private void Awake()
//     {
//         inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
//         hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());

//         allSlots.AddRange(inventorySlots);
//         allSlots.AddRange(hotbarSlots);
//     }

//     private void Update()
//     {
//         if(Input.GetKeyDown(KeyCode.Tab))
//         {
//             bool isOpen = !inventorySlotParent.activeSelf;
//             inventorySlotParent.SetActive(isOpen);

//             Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
//             Cursor.visible = isOpen;

//             // Stop rotation when inventory is open
//             StarterAssets.StarterAssetsInputs inputs = FindObjectOfType<StarterAssets.StarterAssetsInputs>();
//             if (inputs != null)
//             {
//                 inputs.cursorInputForLook = !isOpen; // disable look when inventory is open
//             }
//         }

//         DetectLookedAtItem();
//         Pickup();

//         StartDrag();
//         UpdateDragItemPosition();
//         EndDrag();

//         HandleHotBarSelection();
//         HandleDropedEquippedItem();
//         UpdateHotBarOpacity();
//     }

//     public void AddItem(ItemSO itemToAdd, int amount)
//     {
//         int remainingAmount = amount;

//         foreach (Slot slot in allSlots)
//         {
//             if (slot.HasItem() && slot.GetHeldItem() == itemToAdd)
//             {
//                 int currentAmount = slot.GetItemAmount();
//                 int maxStackSize = itemToAdd.maxStackSize;

//                 if (currentAmount < maxStackSize)
//                 {
//                     int spaceLeft = maxStackSize - currentAmount;
//                     int amountToAdd = Mathf.Min(spaceLeft, remainingAmount);

//                     slot.SetItem(itemToAdd, currentAmount + amountToAdd);
//                     remainingAmount -= amountToAdd;

//                     if (remainingAmount <= 0)
//                     {
//                         return;
//                     }
//                 }
//             }
//         }
//         foreach (Slot slot in allSlots)
//         {
//             if (!slot.HasItem())
//             {
//                 int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remainingAmount);
//                 slot.SetItem(itemToAdd, amountToPlace);
//                 remainingAmount -= amountToPlace;
//                 if (remainingAmount <= 0)
//                 {
//                     return;
//                 }
//             }
//         }
//         if (remainingAmount > 0)
//         {
//             Debug.LogWarning("Not enough space to add all items. Remaining amount: " + remainingAmount);
//         }
//     }
//     private void StartDrag()
//     {
//         if(Input.GetMouseButtonDown(0))
//         {
//             Slot hovered = GetHoveredSlot();

//             if(hovered != null && hovered.HasItem())
//             {
//                 draggedSlot = hovered;
//                 isDragging = true;
//                 //show drag item
//                 dragIcon.sprite = draggedSlot.GetHeldItem().itemIcon;
//                 dragIcon.color = new Color(1, 1, 1, 0.8f);
//                 dragIcon.enabled = true;
//             }
//         }
//     }

//     private void EndDrag()
//     {
//         if(Input.GetMouseButtonUp(0) && isDragging)
//         {
//             Slot hovered = GetHoveredSlot();
//             if(hovered != null)
//             {
//                 HandleDrop(draggedSlot, hovered);

//                 dragIcon.enabled = false;

//                 draggedSlot = null;
//                 isDragging = false;
//             }
//             //hide drag item
//             dragIcon.enabled = false;
//             draggedSlot = null;
//             isDragging = false;
//         }
//     }
//     private Slot GetHoveredSlot()
//     {
//         foreach (var s in allSlots)
//         {
//             if(s.hovering)
//             {
//                 return s;
//             }
//         }
//         return null;
//     }

//     private void HandleDrop(Slot from, Slot to)
//     {
//         if(from == to)
//         {
//             return;
//         }

//         //Stacking
//         if(to.HasItem() && from.GetHeldItem() == to.GetHeldItem())
//         {
//             int max = to.GetHeldItem().maxStackSize;
//             int space = max - to.GetItemAmount();
//             if(space > 0)
//             {
//                 int move = Mathf.Min(space, from.GetItemAmount());

//                 to.SetItem(to.GetHeldItem(), to.GetItemAmount() + move);
//                 from.SetItem(from.GetHeldItem(), from.GetItemAmount() - move);

//                 if(from.GetItemAmount() <= 0)
//                 {
//                     from.ClearSlot();
//                 }
//                 return;
//             }
//         }
        
//         //Different Item
//         if(to.HasItem())
//         {
//             ItemSO tempItem = to.GetHeldItem();
//             int tempAmount = to.GetItemAmount();

//             to.SetItem(from.GetHeldItem(), from.GetItemAmount());
//             from.SetItem(tempItem, tempAmount);
//             return;
//         }

//         //Empty Slot
//         to.SetItem(from.GetHeldItem(), from.GetItemAmount());
//         from.ClearSlot();
//     }
//     private void UpdateDragItemPosition()
//     {
//         if(isDragging)
//         {
//             dragIcon.transform.position = Input.mousePosition;
//         }
//     }

//     private void Pickup()
//     {
//         if(lookedAtRenderer != null && Input.GetKeyDown(KeyCode.E))
//         {
//             Item item = lookedAtRenderer.GetComponent<Item>();
//             if (item != null)
//             {
//             AddItem(item.item, item.amount);
//             Destroy(item.gameObject);
//             EquipHandItem();
//             }
//         }
//     }
//     private void DetectLookedAtItem()
//     {
//         if(lookedAtRenderer != null)
//         {
//             lookedAtRenderer.material = originalMaterial;
//             lookedAtRenderer = null;
//             lookedAtItem = null;
//         }

//         Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
//         if(Physics.Raycast(ray, out RaycastHit hit, pickupRange))
//         {
//             Item item = hit.collider.GetComponent<Item>();
//             if(item != null)
//             {
//                 Renderer rend = item.GetComponent<Renderer>();
//                 if(rend != null)
//                 {
//                     originalMaterial = rend.material;
//                     rend.material = highlightMaterial;
//                     lookedAtRenderer = rend;
//                 }
//                 //lookedAtItem = item;  
//                 //lookedAtRenderer = item.GetComponent<Renderer>();
//                 //originalMaterial = lookedAtRenderer.material;
//                 //lookedAtRenderer.material = highlightMaterial;
//             }
//         }
//     }
//     private void UpdateHotBarOpacity()
//     {
//         for(int i = 0; i < hotbarSlots.Count; i++)
//         {
//             Image icon = hotbarSlots[i].GetComponentInChildren<Image>();
//             if(icon != null)
//             {
//                 icon.color = (i == equippedHotbarIndex) ? new Color(1, 1, 1, equippedOpacity) : new Color(1, 1, 1, normalOpacity);
//             }
//         }
//     }
//     private void HandleHotBarSelection()
//     {
//         for(int i = 0; i < 6; i++)
//         {
//             if(Input.GetKeyDown((i + 1).ToString()))
//             {
//                 equippedHotbarIndex = i;
//                 UpdateHotBarOpacity();
//                 EquipHandItem();
//                 break;
//             }
//         }
//     }
//     private void HandleDropedEquippedItem()
//     {
//         if(!Input.GetKeyDown(KeyCode.Q)) return;

//         Slot equippedSlot = hotbarSlots[equippedHotbarIndex];

//         if(!equippedSlot.HasItem()) return;

//         ItemSO itemSO = equippedSlot.GetHeldItem();
//         GameObject prefab = itemSO.itemPrefab;

//         if(prefab == null) return;

//         GameObject dropped = Instantiate(prefab, Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity);

//         Item item = dropped.GetComponent<Item>();
//         item.item = itemSO;
//         item.amount = equippedSlot.GetItemAmount();

//         equippedSlot.ClearSlot();
//         EquipHandItem();
//     }
//     private void EquipHandItem()
//     {
//         if(currentHandItem != null)
//         {
//             Destroy(currentHandItem);
//         }

//         Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
//         if(!equippedSlot.HasItem())
//         {
//             return;
//         }

//         ItemSO item = equippedSlot.GetHeldItem();
//         if(item.handItemPrefab == null)
//         {
//             return;
//         }
//         currentHandItem = Instantiate(item.handItemPrefab, hand);
//         currentHandItem.transform.localPosition = Vector3.zero;
//         currentHandItem.transform.localRotation = Quaternion.identity;
//     }
// }
