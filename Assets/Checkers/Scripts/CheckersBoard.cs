using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class CheckersBoard : MonoBehaviour
    {
        #region Variables
        [Tooltip("Prefab for Checker Pieces")]
        public GameObject whitePiecePrefab, blackPiecePrefab;
        [Tooltip("Where to attach the spawned pieces in the hierarchy")]
        public Vector3 boardOffset = Vector3.zero;
        public Vector3 pieceOffset = new Vector3(.5f, 0, .5f);
        public Transform checkersParent;
        public float rayDistance = 1000f;
        public LayerMask hitLayers;
        public Piece[,] pieces = new Piece[8, 8];

        private bool isWhiteTurn = true, hasKilled;
        private Vector2 mouseOver, startDrag, endDrag;
        private Piece selectedPiece = null;
        #endregion
        #region start
        void Start()
        {
            GenerateBoard();
        }
        #endregion
        #region update
        private void Update()
        {
            //updates the mouse over information
            MouseOver();
            //is it currently white's turn?
            if (isWhiteTurn)
            {
                //Get x and Y coordinates of selected mouse over
                int x = (int)mouseOver.x;
                int y = (int)mouseOver.y;
                // if the mouse is pressed
                if (Input.GetMouseButtonDown(0))
                {
                    //try selecting piece
                    selectedPiece = SelectPiece(x, y);
                    startDrag = new Vector2(x, y);
                }
                //if there is a selected piece
                if (selectedPiece)
                {
                    // move the piece with mouse
                    DragPiece(selectedPiece);
                }
                //if button is released
                if (Input.GetMouseButtonUp(0))
                {
                    endDrag = new Vector2(x, y); //Record end drag
                    TryMove(startDrag, endDrag); // Try moving the piece
                    selectedPiece = null; // let go of the piece
                }
            }
        }
        #endregion
        #region generate the piece
        public void GeneratePiece(int x, int y, bool isWhite)
        {
            //is the value of the white prefab true?        else default to black prefab
            GameObject prefab = isWhite ? whitePiecePrefab : blackPiecePrefab;
            //create clone of perfab
            GameObject clone = Instantiate(prefab, checkersParent);
            //Reposition clone
            //gets the piece component
            Piece p = clone.GetComponent<Piece>();
            //Updates piece x and y with current location
            p.x = x;
            p.y = y;
            //reposition clone
            MovePiece(p, x, y);

        }
        #endregion
        #region Generate the board
        public void GenerateBoard()
        {
            //Generate White Team
            for (int y = 0; y < 3; y++)
            {
                bool oddRow = y % 2 == 0;
                // Loop through Columns
                for (int x = 0; x < 8; x += 2)
                {
                    //Generate white pieces in the oddrow. Ternary checks the columns + 1. Marks as True for isWhite, so white pieces spawn
                    GeneratePiece(oddRow ? x : x + 1, y, true);
                }
            }
            // Generate Black Team
            for (int y = 5; y < 8; y++)
            {
                bool oddRow = y % 2 == 0;
                //Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    //Generate black pieces in the oddrow. Ternary checks the columns + 1, Marks as false for isWhite, so spawns black pieces
                    GeneratePiece(oddRow ? x : x + 1, y, false);
                }
            }
        }
        #endregion
        #region move piece
        /// <summary>
        /// Moves a piece to another coord on a 2d grid
        /// </summary>
        /// <param name="p">The piece to move</param>
        /// <param name="x">X location</param>
        /// <param name="y">y location</param>
        private void MovePiece(Piece p, int x, int y)
        {
            //updates array
            pieces[p.x, p.y] = null;
            pieces[x, y] = p;
            p.x = x;
            p.y = y;
            //translate the piece to another location
            p.transform.localPosition = new Vector3(x, 0, y) + boardOffset + pieceOffset;
        }
        #endregion
        #region select piece
        /// <summary>
        /// select a piece on 2d grid and returns it
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns></returns>
        private Piece SelectPiece(int x, int y)
        {

            //check if x and Y is out of bounds
            if (OutOfBounds(x, y))
                return null;

            //get piece at x and Y location
            Piece piece = pieces[x, y];
            //check that it isnt null
            if (piece)
                return piece;

            return null;
        }
        #endregion
        #region mouse over
        /// <summary>
        /// Updating whe nthe pieces have been selected
        /// </summary>
        private void MouseOver()
        {
            //perform raycasy from mouse position
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //if the ray hits the board
            if (Physics.Raycast(camRay, out hit, rayDistance, hitLayers))
            {
                //convert mouse coordinates to 2d array coordinates
                mouseOver.x = (int)(hit.point.x - boardOffset.x);
                mouseOver.y = (int)(hit.point.z - boardOffset.z);

            }
            else //otherwise
            {
                //default to error (-1)
                mouseOver.x = -1;
                mouseOver.y = -1;
            }
        }
        #endregion
        #region drag piece
        /// <summary>
        /// Drags the selected piece using raycast location
        /// </summary>
        /// <param name="p"></param>
        private void DragPiece(Piece selected)
        {
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //detects mouse ray hit point
            if (Physics.Raycast(camRay, out hit, rayDistance, hitLayers))
            {
                //updates position of selected piece to hit point + offset
                selected.transform.position = hit.point + Vector3.up;
            }
        }
        #endregion
        #region Try moving
        /// <summary>
        /// Tries to move a piece from Current (x1 + y1) to Desired (x2 + y2) coords
        /// </summary>
        /// <param name="x1">Current X</param>
        /// <param name="y1">Current Y</param>
        /// <param name="x2">Desired X</param>
        /// <param name="y2">Desired Y</param>
        private void TryMove(Vector2 start, Vector2 end)
        {
            int x1 = (int)start.x;
            int y1 = (int)start.y;
            int x2 = (int)end.x;
            int y2 = (int)end.y;

            //Record start drag and end drag
            startDrag = new Vector2(x1, y1);
            endDrag = new Vector2(x2, y2);

            //if there is a selected piece
            if (selectedPiece)
            {
                //check if desired location is out of bounds (x2 is out of bounds)
                if (OutOfBounds(x2, y2))
                {
                    {
                        //move it back to the original
                        MovePiece(selectedPiece, x1, y1);
                        return;
                    }
                }
                if (ValidMove(start, end))
                {
                    MovePiece(selectedPiece, x2, y2);

                }
                else
                {
                    MovePiece(selectedPiece, x1, y1);

                }
                EndTurn();

            }
        }
        #endregion
        #region Out of bounds
        private bool OutOfBounds(int x, int y)
        {
            return x < 0 || x >= 8 || y < 0 || y >= 8;

        }
        #endregion
        #region Valid Move
        private bool ValidMove(Vector2 start, Vector2 end)
        {
            int x1 = (int)start.x;
            int y1 = (int)start.y;
            int x2 = (int)end.x;
            int y2 = (int)end.y;

            //Rule #1 - Is the start the same as the end
            if (start == end)
            {
                return true;
            }
            //Rule #2 - if you are moving on top of another piece
            if (pieces[x2, y2])
            {
                return false;
            }
            //Rule #3 - Detect if we are moving diagonal & forwards / backwards
            #region Old Code
            //int xLocation = x1 - x2;
            //int yLocation = y1 - y2;

            //if(Mathf.Abs(xLocation) == 1 && Mathf.Abs(yLocation) == 1)
            //{
            //    return true;
            //}
            //if(xLocation == 0 || yLocation == 0)
            //{
            //    return false;
            //}
            //if (xLocation <= 1 || yLocation <= 1)
            //{
            //    return false;
            //}
            #endregion
            int locationX = Mathf.Abs(x1 - x2);
            int locationY = y2 - y1;
            //rule for white pieces
            if (selectedPiece.isWhite || selectedPiece.isKing)
            {
                //Check if we're moving diagonally right
                if (locationX == 1 && locationY == 1)
                {
                    //valid move
                    return true;
                }
                //if moving diagonally two spaces
                else if (locationX == 2 && locationY == 2)
                {
                    //get the piece between the move
                    Piece betweenPiece = GetPieceBetween(start, end);
                    //if there is a piece, and the selcted piece  isnt white
                    if (betweenPiece != null && betweenPiece.isWhite != selectedPiece.isWhite)
                    {
                        RemovePiece(betweenPiece);
                        //valid move
                        return true;
                    }
                }

            }
            //Rule for Selecting black pieces
            if (!selectedPiece.isWhite || selectedPiece.isKing)
            {
                //Check if we're moving diagonally right
                if (locationX == 1 && locationY == -1)
                {
                    //this is a valid move
                    return true;
                }
                //if moving two spaces diagonally
                else if (locationX == 2 && locationY == -2)
                {
                    //gets the piece that is between move
                    Piece betweenPiece = GetPieceBetween(start, end);
                    //if there is a piece and the selected piece is not white
                    if (betweenPiece != null && betweenPiece.isWhite != selectedPiece.isWhite)
                    {
                        RemovePiece(betweenPiece);

                        //your allowed to move
                        return true;

                    }
                }

            }


            //Dont do that
            return false;
        }
        #endregion
        private Piece GetPieceBetween(Vector2 start, Vector2 end)
        {
            int xIndex = (int)(start.x + end.x) / 2;
            int yIndex = (int)(start.y + end.y) / 2;
            return pieces[xIndex, yIndex];
        }
        private void RemovePiece(Piece pieceToRemove)
        {
            pieces[pieceToRemove.x, pieceToRemove.y] = null;
            DestroyImmediate(pieceToRemove.gameObject);

        }
        private void EndTurn()
        {

            CheckForKing();

        }
        private void CheckForKing()
        {
            //Gets th end drag location (mouse)
            int x = (int)endDrag.x;
            int y = (int)endDrag.y;
            //check if there is a selected piece

            //if the selected piece is white and reached the end of the board
            if (selectedPiece && !selectedPiece.isKing)
            {
                {
                    bool whiteNeedsKing = selectedPiece.isWhite && y == 7;
                    bool blackNeedsKing = !selectedPiece.isWhite && y == 0;
                    if (whiteNeedsKing || blackNeedsKing)
                    {
                        selectedPiece.isKing = true;
                    }
                }

            }
        }

    }
}
