using System.Collections.Generic;
using UnityEngine;


namespace GWLPXL.InventoryGrid
{


    /// <summary>
    /// manager for the inventory board/grid
    /// </summary>
    [System.Serializable]
    public class Board
    {
        /// <summary>
        /// piece and slots occupied
        /// </summary>
        public System.Action<InventoryPiece, List<BoardSlot>> OnNewPiecePlaced;

        public System.Action<InventoryPiece, InventoryPiece> OnPieceSwapped;
        /// <summary>
        /// piece and slots removed
        /// </summary>
        public System.Action<InventoryPiece, List<BoardSlot>> OnPieceRemoved;
        /// <summary>
        /// new slot created, sub for your own creation
        /// </summary>
        public System.Action<BoardSlot> OnBoardSlotCreated;
        /// <summary>
        /// instance and slot id 
        /// </summary>
        public Dictionary<GameObject, Vector2Int> IDS => ui;
        /// <summary>
        /// preview highlight, preview intance and preview status
        /// </summary>
        public BoardPreview Preview => preview;

        protected Dictionary<GameObject, Vector2Int> ui = new Dictionary<GameObject, Vector2Int>();
        protected Dictionary<InventoryPiece, List<BoardSlot>> SlotsOccupied = new Dictionary<InventoryPiece, List<BoardSlot>>();
        [SerializeField]
        [Tooltip("Runtime slots")]
        protected List<BoardSlot> Slots = new List<BoardSlot>();

        protected BoardPreview preview = new BoardPreview();
        [SerializeField]
        protected int Rows = 6;
        [SerializeField]
        protected int Columns = 9;

        /// <summary>
        /// remove piece from board
        /// </summary>
        /// <param name="origin"></param>
        public virtual void RemovePiece(InventoryPiece origin)
        {
            if (SlotsOccupied.ContainsKey(origin))
            {
                List<BoardSlot> slots = SlotsOccupied[origin];
                RemovePiece(slots);
            }


        }
        /// <summary>
        /// remove piece placed at coordinates
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public virtual InventoryPiece RemovePiece(Vector2Int origin)
        {
            BoardSlot cell = Slots.Find(x => x.Cell.Cell == origin);
            InventoryPiece piece = cell.PieceOnBoard;
            if (piece != null)
            {
                if (SlotsOccupied.ContainsKey(piece))
                {
                    List<BoardSlot> slots = SlotsOccupied[piece];
                    return RemovePiece(slots);

                }
            }


            return cell.PieceOnBoard;

        }

        /// <summary>
        /// used for preview
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public virtual List<BoardSlot> TryPlace(InventoryPiece piece, Vector2Int origin)
        {
            BoardSlot cell = Slots.Find(x => x.Cell.Cell == origin);
            List<BoardSlot> placed = new List<BoardSlot>();
            List<Vector2Int> pattern = piece.Pattern.GetCurrentPattern();
            placed.Add(cell);
            for (int i = 0; i < pattern.Count; i++)
            {
                Vector2Int local = origin + pattern[i];
                BoardSlot newcell = Slots.Find(x => x.Cell.Cell == local);
                placed.Add(newcell);

            }
            return placed;
        }

        /// <summary>
        /// check to see if we can swap piece, if so swap it
        /// returns count > 0 if successful, count == 0 if not successful
        /// </summary>
        /// <param name="currentOnBoard"></param>
        /// <param name="newpiece"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public virtual List<BoardSlot> TrySwapWithBoard(InventoryPiece currentOnBoard, InventoryPiece newpiece, Vector2Int origin)
        {
            BoardSlot cell = Slots.Find(x => x.Cell.Cell == origin);
            List<BoardSlot> placed = new List<BoardSlot>();
            placed.Add(cell);
            List<Vector2Int> pattern = newpiece.Pattern.GetCurrentPattern();
            for (int i = 0; i < pattern.Count; i++)
            {
                Vector2Int local = origin + pattern[i];
                BoardSlot newcell = Slots.Find(x => x.Cell.Cell == local);

                if (newcell == null || newcell.Cell.Occupied && newcell.PieceOnBoard != currentOnBoard)
                {
                    return new List<BoardSlot>();//fail condition
                }
                else
                {
                    placed.Add(newcell);
                }
            }

            //this is success
            RemovePiece(currentOnBoard);
            PlaceOnBoard(newpiece, origin);

            OnPieceSwapped?.Invoke(currentOnBoard, newpiece);
            return placed;
        }
        /// <summary>
        /// place on board at origin, returns board slots occupied if successful and list returns count 0 if not successful
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public virtual List<BoardSlot> PlaceOnBoard(InventoryPiece piece, Vector2Int origin)
        {
            BoardSlot cell = Slots.Find(x => x.Cell.Cell == origin);
            List<BoardSlot> placed = new List<BoardSlot>();
            placed.Add(cell);
            List<Vector2Int> pattern = piece.Pattern.GetCurrentPattern();
            for (int i = 0; i < pattern.Count; i++)
            {
                Vector2Int local = origin + pattern[i];//doesn't seem to be working?
                BoardSlot newcell = Slots.Find(x => x.Cell.Cell == local);
                if (newcell == null)
                {
                    return new List<BoardSlot>();//fail
                }
                else if (newcell.Cell.Occupied)
                {
                    return TrySwapWithBoard(newcell.PieceOnBoard, piece, origin);
                }
                else
                {
                    placed.Add(newcell);
                }
            }


            for (int i = 0; i < placed.Count; i++)
            {
                placed[i].Cell.Occupied = true;
                placed[i].PieceOnBoard = piece;
            }

            SlotsOccupied[piece] = placed;
            OnNewPiecePlaced?.Invoke(piece, placed);

            return placed;
            // Debug.Log("Success");

        }



        /// <summary>
        /// create the grid, also calls the OnBoardSlotCreated
        /// </summary>
        /// <param name="GridTransform"></param>
        /// <param name="CellPrefab"></param>
        /// <param name="PanelGrid"></param>
        public virtual void CreateGrid()
        {

            for (int i = 0; i < Rows; i++)
            {

                for (int j = 0; j < Columns; j++)
                {
                    Vector2Int newID = new Vector2Int(j, i);
                    VirtualInventoryCell cell = new VirtualInventoryCell(newID, false);
                    BoardSlot newslot = new BoardSlot(null, cell, null);
                    preview.CellDictionary.Add(newslot, null);
                    Slots.Add(newslot);
                    OnBoardSlotCreated?.Invoke(newslot);

                }

            }

        }
        /// <summary>
        /// helper to remove piece based on board slots
        /// </summary>
        /// <param name="removed"></param>
        /// <returns></returns>
        protected InventoryPiece RemovePiece(List<BoardSlot> removed)
        {
            if (removed.Count == 0) return null;
            InventoryPiece obj = removed[0].PieceOnBoard;//should be the same for all and not null...
            for (int i = 0; i < removed.Count; i++)
            {
                removed[i].Cell.Occupied = false;
                removed[i].PieceOnBoard = null;
            }
            SlotsOccupied[obj] = new List<BoardSlot>();
            OnPieceRemoved?.Invoke(obj, removed);
            return obj;

        }

    }

    /// <summary>
    /// defines a board/grid slot
    /// </summary>
    [System.Serializable]
    public class BoardSlot
    {
        [Tooltip("the Slot itself")]
        public GameObject Object = default;
        [Tooltip("the thing the slot holds")]
        public InventoryPiece PieceOnBoard = default;
        [Tooltip("the data")]
        public VirtualInventoryCell Cell = default;

        public BoardSlot(GameObject obj, VirtualInventoryCell cell, InventoryPiece piece)
        {
            Cell = cell;
            Object = obj;
            PieceOnBoard = piece;
        }


    }

    /// <summary>
    /// helper for preview
    /// </summary>
    [System.Serializable]
    public class BoardPreview
    {
        public List<BoardSlot> PreviewList = new List<BoardSlot>();
        public Dictionary<BoardSlot, GameObject> CellDictionary = new Dictionary<BoardSlot, GameObject>();
    }

    /// <summary>
    /// the board/grid data
    /// </summary>
    [System.Serializable]
    public class InventoryGrid
    {
        public List<VirtualInventoryCell> Cells = new List<VirtualInventoryCell>();
        public int Rows = 6;
        public int Columns = 9;

    }

}