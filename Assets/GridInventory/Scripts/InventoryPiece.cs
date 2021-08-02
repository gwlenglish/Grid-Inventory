using UnityEngine;

namespace GWLPXL.InventoryGrid
{


    /// <summary>
    /// defines an inventory piece
    /// </summary>
    [System.Serializable]
    public class InventoryPiece
    {
        public GameObject Instance;
        public GameObject PreviewInstance;
        public Pattern Pattern;
        public int EquipmentIdentifier = -1;
        public InventoryPiece(GameObject instance, GameObject previewInstance, Pattern pattern, int equipID = -1)
        {
            EquipmentIdentifier = equipID;
            Pattern = pattern;
            Instance = instance;
            PreviewInstance = previewInstance;
            previewInstance.SetActive(false);
        }
    }
}