using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace GWLPXL.InventoryGrid
{


    /// <summary>
    /// ui example using Board class to generate the inventory grid/board
    /// </summary>
    public partial class GridInventory_UI : MonoBehaviour
    {
        public System.Action<List<RaycastResult>> OnTryRemove;
        public System.Action<InventoryPiece> OnDraggingPiece;
        public System.Action OnStopDragging;
        public System.Action<List<RaycastResult>, InventoryPiece> OnTryPlace;
        public enum InteractState
        {
            Empty = 0,//no dragging
            HasDraggable = 1,//dragging piece not yet placed on the board
            HasInventoryPiece = 2//dragging piece that was placed on the board
        }

        [Tooltip("Responsible for the inventory")]
        public Board TheBoard;
        [Tooltip("Responsible for the equipment")]
        public EquippedGear_UI GearUI;
        [Header("Inventory UI")]
        public Transform MainPanel;//on ui
        public RectTransform GridTransform;//on ui
        public GameObject CellPrefab;//on ui
        public Transform PanelGrid;//on ui


        protected EventSystem eventSystem;
        protected InteractState state = InteractState.Empty;
        protected GraphicRaycaster m_Raycaster;
        protected PointerEventData m_PointerEventData;
        protected InventoryPiece draginstance = null;
        protected InventoryPiece inventorydragging = null;
        protected List<RaycastResult> results = new List<RaycastResult>();

        #region virtual unity calls
        /// <summary>
        /// initialization
        /// </summary>
        protected virtual void Awake()
        {
            eventSystem = FindObjectOfType<EventSystem>();
            m_Raycaster = GetComponent<GraphicRaycaster>();
            m_PointerEventData = new PointerEventData(eventSystem);
        }
        /// <summary>
        /// sub events
        /// </summary>
        protected virtual void OnEnable()
        {
            TheBoard.OnBoardSlotCreated += CreateBoardSlotInstance;
            TheBoard.OnPieceRemoved += Removed;
            TheBoard.OnNewPiecePlaced += Placed;
            GearUI.OnEquippedPiece += EquippedPiece;
            GearUI.OnUnEquipPiece += CarryRemovedPiece;
            GearUI.OnSwappedPiece += SwappedEquipmentPiece;
            TheBoard.OnPieceSwapped += SwappedInventory;
        }


        /// <summary>
        /// unsub events
        /// </summary>
        protected virtual void OnDisable()
        {
            TheBoard.OnBoardSlotCreated -= CreateBoardSlotInstance;
            TheBoard.OnPieceSwapped -= SwappedInventory;
            TheBoard.OnPieceRemoved -= Removed;
            GearUI.OnEquippedPiece -= EquippedPiece;
            GearUI.OnUnEquipPiece -= CarryRemovedPiece;
            GearUI.OnSwappedPiece -= SwappedEquipmentPiece;
            TheBoard.OnNewPiecePlaced -= Placed;
        }
        /// <summary>
        /// creation
        /// </summary>
        protected virtual void Start()
        {
            CreateGrid();

        }


        /// <summary>
        /// mouse navigation
        /// </summary>
        protected virtual void LateUpdate()
        {

            //need to set the pointer data.
            m_PointerEventData.position = Input.mousePosition;

            switch (state)
            {
                case InteractState.Empty:
                    TryRemoveDraggableFromGrid();
                    break;
                case InteractState.HasDraggable:
                    MoveDraggable(draginstance);
                    PreviewHighlight(draginstance);
                    TryPlaceDraggable(draginstance);
                    break;
                case InteractState.HasInventoryPiece:
                    MoveDraggable(inventorydragging);
                    PreviewHighlight(inventorydragging);
                    TryPlaceDraggable(inventorydragging);
                    break;
            }




        }
        #endregion

        #region public virtual
        public virtual void CreateGrid()
        {
            TheBoard.CreateGrid();

            //just resets the color
            foreach (var kvp in TheBoard.IDS)
            {
                kvp.Key.GetComponent<Image>().color = new Color(255, 255, 255, 0);
            }
        }
        /// <summary>
        /// clear state
        /// </summary>
        public virtual void NoPieces()
        {
            draginstance = null;
            inventorydragging = null;
            state = InteractState.Empty;
            OnStopDragging?.Invoke();
        }
        /// <summary>
        /// create draggable piece that isn't already on the grid
        /// </summary>
        /// <param name="pattern"></param>
        public virtual void CreateNewInventoryPiece(PatternHolder pattern, int equipID = -1)
        {
            draginstance = new InventoryPiece(Instantiate(pattern.Pattern.PatternPrefab, MainPanel), Instantiate(pattern.Pattern.PatternPrefab, MainPanel), pattern.Pattern, equipID);
            draginstance.PreviewInstance.SetActive(false);
            state = InteractState.HasDraggable;
        }


        /// <summary>
        /// remove draggable piece that is already on the grid
        /// </summary>
        /// <param name="piece"></param>
        public virtual void CarryRemovedPiece(InventoryPiece piece)
        {
            if (piece.Instance == null) NoPieces();
            inventorydragging = piece;
            piece.PreviewInstance.SetActive(false);
            state = InteractState.HasInventoryPiece;
            OnDraggingPiece?.Invoke(piece);
        }

        #endregion

        #region protected virtual
        protected virtual void Removed(InventoryPiece piece, List<BoardSlot> slots)
        {
            CarryRemovedPiece(piece);
        }
        protected virtual void Placed(InventoryPiece piece, List<BoardSlot> slots)
        {
            NoPieces();
        }
        protected virtual void SwappedEquipmentPiece(InventoryPiece old, InventoryPiece newpiece)
        {
            CarryRemovedPiece(old);
        }
        protected virtual void EquippedPiece(InventoryPiece piece)
        {
            NoPieces();
        }
        protected virtual void SwappedInventory(InventoryPiece old, InventoryPiece newpiece)
        {
            Debug.Log("Swapped Inventory");
            CarryRemovedPiece(old);
        }
        /// <summary>
        /// creates the gameobjects that represent the grid slots
        /// </summary>
        /// <param name="slot"></param>
        protected virtual void CreateBoardSlotInstance(BoardSlot slot)
        {
            if (slot.Object != null)
            {
                Destroy(slot.Object);
            }
            GameObject instance = UnityEngine.GameObject.Instantiate(CellPrefab, PanelGrid);
            instance.name = "Cell " + "X:" + slot.Cell.Cell.x + " Y:" + slot.Cell.Cell.y;
            TMPro.TextMeshProUGUI text = instance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null) text.SetText((slot.Cell.Cell.x + " , " + slot.Cell.Cell.y));
            TheBoard.IDS.Add(instance, slot.Cell.Cell);
            TheBoard.Preview.CellDictionary[slot] = instance;
            slot.Object = instance;
        }


        /// <summary>
        /// draggable movement
        /// </summary>
        /// <param name="dragginginstance"></param>
        protected virtual void MoveDraggable(InventoryPiece dragginginstance)
        {

            Vector3 screenPoint = (Input.mousePosition);
            screenPoint.z = GetComponent<Canvas>().planeDistance; //distance of the plane from the camera

            float rot = 0;
            if (dragginginstance.Pattern.CurrentRotation == PatternAlignment.Horizontal)
            {
                rot = -90;

            }

            dragginginstance.Instance.transform.position = Camera.main.ScreenToWorldPoint(screenPoint);// + new Vector3(xoffset, 0)) ;
            dragginginstance.Instance.transform.rotation = Quaternion.Euler(0, 0, rot);//dont rotate, just width and height...
            dragginginstance.PreviewInstance.transform.rotation = Quaternion.Euler(0, 0, rot);
        }


        /// <summary>
        /// try place in grid slot
        /// </summary>
        /// <param name="piece"></param>
        protected virtual void TryPlaceDraggable(InventoryPiece piece)
        {
            if (Input.GetMouseButtonDown(0) == false) return;
            if (m_PointerEventData == null) return;//nothing clicked, nothing ventured

            //Create a list of Raycast Results
            results.Clear();

            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);//can be null...

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                Debug.Log("Hit " + result.gameObject.name);
                if (TheBoard.IDS.ContainsKey(result.gameObject) == false) continue;

                bool placed = TryPlaceOnBoard(result.gameObject, piece);
                if (placed)
                {
                    return;
                }
            }

            //if didn't place on bard, try passing to other systems
            OnTryPlace?.Invoke(results, piece);

        }


        /// <summary>
        /// try remove from grid
        /// </summary>
        protected virtual void TryRemoveDraggableFromGrid()
        {
            if (Input.GetMouseButtonDown(0) == false) return;

            if (m_PointerEventData == null) return;//nothing clicked, nothing ventured
            Debug.Log("Try Remove");
            //Create a list of Raycast Results
            results.Clear();

            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);//can be null...
            Debug.Log(results.Count);

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                Debug.Log("Hit " + result.gameObject.name, result.gameObject);
                if (TheBoard.IDS.ContainsKey(result.gameObject) == false) continue;
                Debug.Log("Hit " + result.gameObject.name, result.gameObject);
                InventoryPiece removed = TryRemoveDraggableFromGrid(result.gameObject);
                if (removed != null)
                {
                    return;
                }

            }

            OnTryRemove?.Invoke(results);

        }

        protected virtual InventoryPiece TryRemoveDraggableFromGrid(GameObject key)
        {
            return TheBoard.RemovePiece(TheBoard.IDS[key]);

        }


        /// <summary>
        /// try place draggable piece on board
        /// </summary>
        /// <param name="slotKey"></param>
        /// <param name="newPiece"></param>
        /// <returns></returns>
        protected virtual bool TryPlaceOnBoard(GameObject slotKey, InventoryPiece newPiece)
        {
            List<BoardSlot> placed = TheBoard.PlaceOnBoard(newPiece, TheBoard.IDS[slotKey]);
            if (placed.Count > 0)//means success
            {
                newPiece.Instance.transform.position = newPiece.PreviewInstance.transform.position;
                newPiece.PreviewInstance.SetActive(false);
                return true;
            }
            return false;
        }


        /// <summary>
        /// preview stuff could use a cell class that we can enable/disable preview stuff, clean up later
        /// </summary>
        /// <param name="piece"></param>
        protected virtual void PreviewHighlight(InventoryPiece piece)
        {
            if (piece == null) return;



            for (int i = 0; i < TheBoard.Preview.PreviewList.Count; i++)
            {
                TheBoard.Preview.PreviewList[i].Cell.Preview = false;
                TheBoard.Preview.CellDictionary[TheBoard.Preview.PreviewList[i]].GetComponent<Image>().color = new Color(255, 255, 255, 0);
            }


            List<RaycastResult> preview = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, preview);
            piece.PreviewInstance.SetActive(false);
            foreach (RaycastResult result in preview)
            {
                if (TheBoard.IDS.ContainsKey(result.gameObject) == false) continue;
                //else, show preview

                TheBoard.Preview.PreviewList = TheBoard.TryPlace(piece, TheBoard.IDS[result.gameObject]);
                for (int i = 0; i < TheBoard.Preview.PreviewList.Count; i++)
                {
                    BoardSlot slot = TheBoard.Preview.PreviewList[i];
                    if (slot == null)
                    {
                        TheBoard.Preview.PreviewList.RemoveAt(i);
                        continue;
                    }
                    else if (slot.Cell.Occupied)
                    {
                        piece.PreviewInstance.GetComponentInChildren<Image>().color = Color.red;


                    }
                    else
                    {
                        piece.PreviewInstance.GetComponentInChildren<Image>().color = Color.green;

                    }
                }

                Vector3 newpos = result.gameObject.transform.position;
                piece.PreviewInstance.transform.position = newpos;
                piece.PreviewInstance.SetActive(true);




                for (int i = 0; i < TheBoard.Preview.PreviewList.Count; i++)
                {
                    if (TheBoard.Preview.PreviewList[i] == null)
                    {
                        TheBoard.Preview.PreviewList.RemoveAt(i);
                        continue;
                    }
                    TheBoard.Preview.PreviewList[i].Cell.Preview = true;

                }
            }

            for (int i = 0; i < TheBoard.Preview.PreviewList.Count; i++)
            {
                if (TheBoard.Preview.PreviewList[i].Cell.Preview == false)
                {

                    TheBoard.Preview.CellDictionary[TheBoard.Preview.PreviewList[i]].GetComponent<Image>().color = new Color(255, 255, 255, 0);
                    TheBoard.Preview.PreviewList.RemoveAt(i);
                }
                else
                {
                    TheBoard.Preview.CellDictionary[TheBoard.Preview.PreviewList[i]].GetComponent<Image>().color = Color.green;
                }
            }

        }

        #endregion


    }
}