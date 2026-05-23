//using UnityEngine;

//public class Pickup : MonoBehaviour
//{
//    public enum PickupType { Gun, Ammo, Flashlight }
//    public PickupType type;
//    public int ammoAmount;

//    private void OnTriggerEnter(Collider other)
//    {
//        Inventory inv = other.GetComponent<Inventory>();
//        if (inv != null)
//        {
//            switch (type)
//            {
//                case PickupType.Gun:
//                    inv.AddGun(gameObject);
//                    break;
//                case PickupType.Ammo:
//                    inv.AddAmmo(ammoAmount);
//                    break;
//                case PickupType.Flashlight:
//                    inv.AddFlashlight(gameObject);
//                    break;
//            }

//            Destroy(gameObject); // remove pickup from scene
//        }
//    }
//}
