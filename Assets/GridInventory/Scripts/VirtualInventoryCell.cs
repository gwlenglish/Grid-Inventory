using UnityEngine;

namespace GWLPXL.InventoryGrid
{


    /// <summary>
    /// defines an inventory cell/slot
    /// </summary>
    [System.Serializable]
    public class VirtualInventoryCell
    {
        public Vector2Int Cell;
        public bool Occupied;
        public bool Preview;
        public VirtualInventoryCell(Vector2Int cell, bool occupied)
        {
            Cell = cell;
            Occupied = occupied;
        }

    }
}