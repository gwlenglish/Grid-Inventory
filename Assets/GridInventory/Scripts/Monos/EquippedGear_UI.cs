using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GWLPXL.InventoryGrid
{


    /// <summary>
    /// example UI class that uses the EquippedGear class and subs to GridInventory_UI
    /// </summary>
    public class EquippedGear_UI : MonoBehaviour
    {
        public System.Action<InventoryPiece> OnEquippedPiece;
        public System.Action<InventoryPiece> OnUnEquipPiece;
        public System.Action<InventoryPiece, InventoryPiece> OnSwappedPiece;
        public GridInventory_UI InventoryUI;
        [SerializeField]
        protected EquippedGear gear = new EquippedGear();

        #region unity virtual calls
        protected virtual void OnEnable()
        {
            InventoryUI.OnTryPlace += TryPlace;
            InventoryUI.OnTryRemove += TryRemove;
            InventoryUI.OnDraggingPiece += CheckDragging;
            InventoryUI.OnStopDragging += StopCheckDragging;
            gear.OnGearPlaced += GearEquipped;
            gear.OnGearRemoved += GearUnEquipped;
            gear.OnGearSwap += GearSwapped;
        }

        protected virtual void OnDisable()
        {
            InventoryUI.OnTryPlace -= TryPlace;
            InventoryUI.OnTryRemove -= TryRemove;
            InventoryUI.OnDraggingPiece -= CheckDragging;
            InventoryUI.OnStopDragging -= StopCheckDragging;
            gear.OnGearPlaced -= GearEquipped;
            gear.OnGearRemoved -= GearUnEquipped;
            gear.OnGearSwap -= GearSwapped;
        }

        protected virtual void Start()
        {
            CreateGear();
        }

        #endregion

        #region public virtual
        public virtual void CreateGear()
        {
            gear.Setup();
        }
        #endregion

        #region protected virtual

        protected virtual void GearSwapped(InventoryPiece old, InventoryPiece newpiece)
        {

            Debug.Log("Swapped Gear " + old + " and " + newpiece);
            OnSwappedPiece?.Invoke(old, newpiece);
        }
        protected virtual void GearEquipped(InventoryPiece piece)
        {
            Debug.Log("Placed Gear " + piece.Instance.name);
            OnEquippedPiece?.Invoke(piece);
        }

        protected virtual void GearUnEquipped(InventoryPiece piece)
        {
            Debug.Log("Removed Gear " + piece.Instance.name);
            OnUnEquipPiece?.Invoke(piece);

        }

        protected virtual void CheckDragging(InventoryPiece dragging)
        {
            foreach (var kvp in gear.GearSlotIDDic)
            {
                if (dragging.EquipmentIdentifier == kvp.Key)
                {
                    kvp.Value.Object.GetComponent<Image>().color = Color.green;
                }
                else
                {
                    kvp.Value.Object.GetComponent<Image>().color = Color.red;
                }
            }
        }
        protected virtual void StopCheckDragging()
        {
            foreach (var kvp in gear.GearSlotIDDic)
            {
                kvp.Value.Object.GetComponent<Image>().color = Color.white;

            }

        }


        protected virtual void TryRemove(List<RaycastResult> results)
        {
            foreach (RaycastResult result in results)
            {
                if (gear.GearSlotDic.ContainsKey(result.gameObject) == false) continue;
                Debug.Log("Hit Gear " + result.gameObject.name, result.gameObject);
                GearSlot slot = gear.GearSlotDic[result.gameObject];
                InventoryPiece piece = slot.Piece;
                if (piece == null) continue;

                bool unequip = gear.RemoveFromSlot(result.gameObject, piece);

                if (unequip)
                {
                    return;
                }

            }
        }
        protected virtual void TryPlace(List<RaycastResult> results, InventoryPiece piece)
        {
            foreach (RaycastResult result in results)
            {

                if (gear.GearSlotDic.ContainsKey(result.gameObject) == false) continue;

                GearSlot slot = gear.GearSlotDic[result.gameObject];
                if (slot.Piece != null)
                {
                    bool placed = gear.Swap(slot, piece);
                    if (placed)
                    {
                        return;
                    }
                }
                else
                {
                    bool placed = gear.PlaceInSlot(result.gameObject, piece);
                    if (placed)
                    {
                        return;
                    }
                }

            }
        }


        #endregion


    }
}