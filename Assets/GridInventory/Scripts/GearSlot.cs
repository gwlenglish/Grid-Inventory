using UnityEngine;

namespace GWLPXL.InventoryGrid
{


    /// <summary>
    /// defines a gear slot
    /// </summary>
    [System.Serializable]
    public class GearSlot
    {
        public GameObject Object = default;
        public int Identifier = 0;
        public InventoryPiece Piece = null;
        public GearSlot(GameObject iteminstance, int identifier)
        {
            Object = iteminstance;
            Identifier = identifier;
        }
    }
}