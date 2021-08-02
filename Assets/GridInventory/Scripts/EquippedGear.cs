using System.Collections.Generic;
using UnityEngine;

namespace GWLPXL.InventoryGrid
{


    /// <summary>
    /// manager for the equipment
    /// </summary>
    [System.Serializable]
    public class EquippedGear
    {
        public System.Action<InventoryPiece> OnGearRemoved;
        public System.Action<InventoryPiece> OnGearPlaced;
        public System.Action<InventoryPiece, InventoryPiece> OnGearSwap;
        public System.Action<GearSlot> OnSlotCreated;
        public System.Action<GearSlot> OnSlotRemoved;

        public Dictionary<GameObject, GearSlot> GearSlotDic => gearSlotDic;
        public Dictionary<int, GearSlot> GearSlotIDDic => gearSlotIDDic;
        /// <summary>
        /// design time, set in the editor
        /// </summary>
        [Tooltip("Set in the editor")]
        public List<GearSlot> Slots = new List<GearSlot>();

        /// <summary>
        /// values used at runtime
        /// </summary>
        [SerializeField]
        [Tooltip("Runtime values")]
        protected List<GearSlot> registeredSlots = new List<GearSlot>();

        protected Dictionary<GameObject, GearSlot> gearSlotDic = new Dictionary<GameObject, GearSlot>();
        protected Dictionary<int, GearSlot> gearSlotIDDic = new Dictionary<int, GearSlot>();

        #region public virtual
        /// <summary>
        /// call to initialize the equipment manager
        /// </summary>
        public virtual void Setup()
        {
            registeredSlots.Clear();
            gearSlotDic.Clear();
            gearSlotIDDic.Clear();
            for (int i = 0; i < Slots.Count; i++)
            {
                AddEquippedGearSlot(Slots[i].Object, Slots[i].Identifier);
            }
        }

        /// <summary>
        /// checks if piece has same ID as slot
        /// </summary>
        /// <param name="slotInstance"></param>
        /// <param name="piece"></param>
        /// <returns></returns>
        public virtual bool CanFitSlot(GameObject slotInstance, InventoryPiece piece)
        {
            if (gearSlotDic.ContainsKey(slotInstance))
            {
                GearSlot slot = gearSlotDic[slotInstance];
                if (slot.Identifier == piece.EquipmentIdentifier)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// swaps newpiece with piece already in slot, used for swapping dragged equipment
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="newpiece"></param>
        /// <returns></returns>
        public virtual bool Swap(GearSlot slot, InventoryPiece newpiece)
        {
            if (CanFitSlot(slot.Object, newpiece))
            {
                Swap(newpiece, slot);
                return true;
            }
            return false;

        }

        /// <summary>
        /// will place if nothing in slot, will try to swap if something is in.
        /// </summary>
        /// <param name="slotInstance"></param>
        /// <param name="piece"></param>
        /// <returns></returns>
        public virtual bool PlaceInSlot(GameObject slotInstance, InventoryPiece piece)
        {

            if (CanFitSlot(slotInstance, piece))
            {
                GearSlot slot = gearSlotDic[slotInstance];
                if (slot.Piece == null || slot.Piece.Instance == null)
                {
                    Place(piece, slot);
                }
                else if (slot.Piece.Instance != null)
                {
                    Swap(piece, slot);
                    return true;
                }
            }
            return false;

        }



        /// <summary>
        /// will remove if piece is present
        /// </summary>
        /// <param name="slotInstance"></param>
        /// <param name="piece"></param>
        /// <returns></returns>
        public virtual bool RemoveFromSlot(GameObject slotInstance, InventoryPiece piece)
        {
            if (gearSlotDic.ContainsKey(slotInstance))
            {
                GearSlot slot = gearSlotDic[slotInstance];
                if (slot.Piece != null)
                {
                    Remove(slot);
                    return true;
                }

            }
            return false;
        }




        /// <summary>
        /// remove an existing slot completely
        /// </summary>
        /// <param name="instance"></param>
        public virtual void RemoveEquippedGearSlot(GameObject instance)
        {
            if (gearSlotDic.ContainsKey(instance))
            {
                int id = gearSlotDic[instance].Identifier;
                if (gearSlotIDDic.ContainsKey(id))
                {
                    gearSlotIDDic.Remove(id);
                }
                GearSlot slot = gearSlotDic[instance];
                gearSlotDic.Remove(instance);
                registeredSlots.Remove(slot);
                OnSlotRemoved?.Invoke(slot);

            }
        }

        /// <summary>
        /// add a new gear slot
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="identifier"></param>
        public virtual void AddEquippedGearSlot(GameObject instance, int identifier)
        {
            if (gearSlotDic.ContainsKey(instance) == false)
            {
                GearSlot slot = new GearSlot(instance, identifier);
                gearSlotDic.Add(instance, slot);
                gearSlotIDDic.Add(slot.Identifier, slot);
                registeredSlots.Add(slot);
                OnSlotCreated?.Invoke(slot);
            }


        }

        #endregion

        #region protected virtual

        /// <summary>
        /// place piece in slot, no checks
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="slot"></param>
        protected virtual void Place(InventoryPiece piece, GearSlot slot)
        {
            slot.Piece = piece;
            slot.Piece.Instance.transform.position = slot.Object.transform.position;
            OnGearPlaced?.Invoke(slot.Piece);
        }

        /// <summary>
        /// swap with piece already in slot, no checks
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="slot"></param>
        protected virtual void Swap(InventoryPiece piece, GearSlot slot)
        {
            InventoryPiece oldpiece = slot.Piece;
            slot.Piece = piece;
            piece.Instance.transform.position = slot.Object.transform.position;
            OnGearSwap?.Invoke(oldpiece, piece);
        }
        /// <summary>
        /// remove piece from slot, no checks
        /// </summary>
        /// <param name="slot"></param>
        protected virtual void Remove(GearSlot slot)
        {
            InventoryPiece removedPiece = slot.Piece;
            slot.Piece = null;
            OnGearRemoved?.Invoke(removedPiece);
        }

        #endregion
    }
}