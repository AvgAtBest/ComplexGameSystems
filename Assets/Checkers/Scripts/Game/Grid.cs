using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers2
{
    using ForcedMoves = Dictionary<Piece, List<Vector2Int>>;
    public class Grid : MonoBehaviour
    {
        public GameObject redPiecePrefab, whitePiecePrefab;
        public Vector3 boardOffset = new Vector3(-2.0f, 0.0f, -4.0f);
        public Vector3 pieceOffset = new Vector3(.5f, 0, .5f);
        public Piece[,] pieces = new Piece[8, 8];
        #region Drag and Drop References
        private Vector2Int mouseOver; // grid coordinates the mouse is over
        private Piece selectedPiece;//the piece that has been clicked and dragged
        #endregion

        private ForcedMoves forcedMoves = new ForcedMoves();
        private void Start()
        {
            GenerateBoard();
        }
        //Converts array coordinates to world position
        Vector3 GetWorldPosition(Vector2Int cell)
        {
            return new Vector3(cell.x, 0, cell.y) + boardOffset + pieceOffset;
        }

        //Moves a piece to another coordinate on a 2D grid
        void MovePiece(Piece piece, Vector2Int newCell)
        {
            Vector2Int oldCell = piece.cell;
            //update array
            pieces[oldCell.x, oldCell.y] = null;
            pieces[newCell.x, newCell.y] = piece;
            //update data on piece

            piece.oldCell = oldCell;
            piece.cell = newCell;
            //translate the new piece to another location
            piece.transform.localPosition = GetWorldPosition(newCell);

        }
        //Generate the pieces in specified cooridnates
        void GeneratePiece(GameObject prefab, Vector2Int desiredCell)
        {
            //Generate instance of prefab
            GameObject clone = Instantiate(prefab, transform);
            //Get the piece component
            Piece piece = clone.GetComponent<Piece>();
            //Set the cell data for the first time
            piece.oldCell = desiredCell;
            piece.cell = desiredCell;
            //Reposition piece
            MovePiece(piece, desiredCell);


        }
        void GenerateBoard()
        {
            Vector2Int desiredCell = Vector2Int.zero;
            //Generate White Team
            #region White Piece Gen
            for (int y = 0; y < 3; y++)
            {
                //loop through columns
                bool oddRow = y % 2 == 0;
                for (int x = 0; x < 8; x += 2)
                {
                    desiredCell.x = oddRow ? x : x + 1;

                    desiredCell.y = y;
                    //GeneratePiece
                    GeneratePiece(whitePiecePrefab, desiredCell);
                }
            }
            #endregion
            #region Red Piece Gen
            for (int y = 5; y < 8; y++)
            {
                bool oddRow = y % 2 == 0;
                for (int x = 0; x < 8; x += 2)
                {
                    desiredCell.x = oddRow ? x : x + 1;
                    desiredCell.y = y;

                    GeneratePiece(redPiecePrefab, desiredCell);
                }
            }
            #endregion
        }
        //grabs a piece at coordinates
        Piece GetPiece(Vector2Int cell)
        {
            return pieces[cell.x, cell.y];
        }
        //out of bounds check for pieces
        bool IsOutOfBounds(Vector2Int cell)
        {
            //makes a piece unable to be moved further out of the grid columns or rows
            return cell.x < 0 || cell.x >= 8 ||
                   cell.y < 0 || cell.y >= 8;
        }

        //selects a piece on the 2d grid and returns it
        Piece SelectPiece(Vector2Int cell)
        {
            //check if x and y is out of bounds
            if (IsOutOfBounds(cell))
            {
                //return the result early
                return null;
            }
            //grabs piece at x and y location (cell)
            Piece piece = GetPiece(cell);
            //if there is a piece
            if (piece)
            {

                return piece;
            }
            return null;
        }
        void MouseOver()
        {
            //shoots a ray from the camera to where the mouse position is on screen
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //if the ray hits something
            if (Physics.Raycast(camRay, out hit))
            {
                //converts mouse coordinates to 2d array coordinates
                mouseOver.x = (int)(hit.point.x - boardOffset.x);
                mouseOver.y = (int)(hit.point.z - boardOffset.z);
            }
            else
            {
                mouseOver = new Vector2Int(-1, -1);
            }
        }
        void DragPiece(Piece selected)
        {
            //shoots a ray from the camera to where the mouse position is on screen
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //if the ray hits something
            if (Physics.Raycast(camRay, out hit))
            {
                //updates position of selected piece to hit point + offset
                selected.transform.position = hit.point + Vector3.up;
            }
        }
        private void Update()
        {
            //TEMP PLACED
            DetectForcedMoves();
            MouseOver();
            //if left mouse button is held down
            if (Input.GetMouseButtonDown(0))
            {
                //hold the selected piece 
                selectedPiece = SelectPiece(mouseOver);
            }
            //if a piece has been selected
            if (selectedPiece)
            {
                //drag it/move it around
                DragPiece(selectedPiece);
                //if the left mouse button is released
                if (Input.GetMouseButtonUp(0))
                {
                    //move piece to end position
                    TryMove(selectedPiece, mouseOver);
                    //let go of the freaking piece
                    selectedPiece = null;
                }
            }
        }
        bool TryMove(Piece selected, Vector2Int desiredCell)
        {
            //Get the selected Pieces cell
            Vector2Int startCell = selected.cell;

            if (IsOutOfBounds(desiredCell))
            {
                MovePiece(selected, startCell);
                Debug.Log("<color=red>Invalid - You cannot move outside the map</color>");
                return false;
            }
            //is it not a valid move
            if (!ValidMove(selected, desiredCell))
            {
                //move back to original
                MovePiece(selected, startCell);

                //exit function
                return false;
            }
            //it isnt out of bounds, you can move here bud
            MovePiece(selected, desiredCell);
            return true;
        }
        //check if the start and end drag is a valid move
        bool ValidMove(Piece selected, Vector2Int desiredCell)
        {
            #region Rule #1 = is piece out of bounds?
            //is the desired cell out of bounds
            if (IsOutOfBounds(desiredCell))
            {

                Debug.Log("<color=red>Invalid - You cannot move outside of the board</color>");
                return false;
                //cant freaking move there bud
            }
            #endregion
            #region Rule #02 - Is the selected cell the same as desired
            if (selected.cell == desiredCell)
            {
                Debug.Log("<color=red>Invalid - Putting pieces back dont count as a valid move</color>");
                return false;
            }
            #endregion
            #region Rule #03 - Is the piece at the desired cell not empty?
            if (GetPiece(desiredCell))
            {
                Debug.Log("<color=red>Invalid - You can't go on top of other pieces</color>");
                return false;
            }
            #endregion
            #region Rule #04 - Is there any forced moves
            //is there any forced moves?
            if (HasForcedMoves(selected))
            {
                if (!IsForcedMove(selected, desiredCell))
                {
                    Debug.Log("<color=red>Invalid - You have to use forced moves!</color>");
                    return false;
                }
            }
            #endregion
            //get direction of movement for some of the next few rules
            Vector2Int direction = selected.cell - desiredCell;
            #region Rule #05 - Is the selected cell being dragged two cells over?
            //is the piece moved two spaces?
            if (direction.magnitude > 2)
            {
                if (forcedMoves.Count == 0)
                {
                    Debug.Log("<color=red>Invalid - You can only move two spaces if there are forced moves on selected piece</color>");

                    return false;
                }
            }
            #endregion
            #region Rule #06 - Is the piece not getting in a diagonal Cell
            //is the playert not moving diagonally
            if (Mathf.Abs(direction.x) != Mathf.Abs(direction.y))
            {
                Debug.Log("<color=red>Invalid - You have to be moving diagonally</color>");
                return false;
            }
            #endregion
            #region Rule #07 - Is the piece moving in the right direction?
            //if the selected Piece isnt kinged
            if (!selectedPiece.isKing)
            {
                //if the selected piece is white
                if (selectedPiece.isWhite)
                {
                    //is it moving up
                    if (direction.y > 0)
                    {
                        Debug.Log("<color=red>Invalid - Cant move a white piece backwards</color>");
                        return false;
                    }
                }
                //is the selected Piece red?
                else
                {

                    //if it is going down
                    if (direction.y < 0)
                    {
                        Debug.Log("<color=red>Invalid - Cant move a red piece backwards</color>");
                        return false;
                    }
                }
            }
            #endregion
            return true;

        }
        void CheckForcedMove(Piece piece)
        {
            //Get the cell location of piece
            Vector2Int cell = piece.cell;
            //Loop through adjacent cells of cell
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    //Create offset cell from index
                    Vector2Int offset = new Vector2Int(x, y);
                    //Create a new x from piece coordinates using offset
                    Vector2Int desiredCell = cell + offset;

                    #region Check #01 - Correct Direction?
                    //Is the piece not king?
                    if (!piece.isKing)
                    {
                        //If the piece white?
                        if (piece.isWhite)
                        {
                            //Is the piece moving backwards?
                            if (desiredCell.y < cell.y)
                            {
                                //Invalid - Check next one
                                continue;
                            }
                        }
                        //If the piece red?
                        else
                        {
                            //Is the piece moving backwards?
                            if (desiredCell.y > cell.y)
                            {
                                //Invalid move, check the next one 
                                continue;
                            }
                        }
                    }
                    #endregion
                    #region Check #02 - Is the adjacent cell out of bounds?
                    //Is desired cell out of bounds?
                    if (IsOutOfBounds(desiredCell))
                    {
                        //Invalid- Check next one
                        continue;
                    }
                    #endregion
                    Piece detectedPiece = GetPiece(desiredCell);
                    #region Check #03 - Is the desired cell empty?
                    //Is there a detected piece?
                    if (detectedPiece == null)
                    {
                        //Invalid, check again
                        continue;
                    }
                    #endregion
                    #region Check #04 - Is the detected piece the same color?
                    if (detectedPiece.isWhite == piece.isWhite)
                    {
                        //Invalid - Check the next one
                        continue;
                    }
                    #endregion
                    Vector2Int jumpCell = cell + (offset * 2);
                    #region Check #05 - Is the jump cell out of bounds?
                    if (IsOutOfBounds(jumpCell))
                    {
                        continue;
                    }
                    #endregion
                    #region Check #06 - Is there a piece at the jump cell?
                    //Get piece next to the one we want to jump
                    detectedPiece = GetPiece(jumpCell);
                    if (detectedPiece)
                    {
                        continue;
                    }
                    #endregion

                    #region Store Forced Move
                    //Check if forced moves contains the piece we're currently checking
                    if (!forcedMoves.ContainsKey(piece))
                    {
                        //Add it to list of forced moves
                        forcedMoves.Add(piece, new List<Vector2Int>());
                    }
                    forcedMoves[piece].Add(jumpCell);
                    #endregion
                }
            }


        }
        void DetectForcedMoves()
        {
            forcedMoves = new ForcedMoves();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Piece pieceToCheck = pieces[x, y];

                    if (pieceToCheck)
                    {
                        CheckForcedMove(pieceToCheck);
                    }
                }
            }
        }
        //Check if a piece has forced pieces
        bool HasForcedMoves(Piece selected)
        {
            //Loop through all forced moved
            foreach (var move in forcedMoves)
            {
                //Get piece for forced move
                Piece piece = move.Key;

                //Is the piece being forced to move the same color as selected Piece?
                if (piece.isWhite == selected.isWhite)
                {
                    //Our selected piece has forced moves
                    return true;

                }
            }
            //Does not have any forced moves
            return false;
        }
        //Checks if the selected piece has forced moves
        bool IsForcedMove(Piece selected, Vector2Int desiredCell)
        {
            //Does the selected piece have a forced move
            if (forcedMoves.ContainsKey(selected))
            {
                //is there any forced moves for this piece
                if (forcedMoves[selected].Contains(desiredCell))
                {
                    //it is a forced move
                    return true;
                }
            }//it is a not forced move
            return false;
        }

        //remove Piece
        void RemovePiece(Piece pieceToRemove)
        {

            Vector2Int cell = pieceToRemove.cell;
            //Clear cell in 2d Array
            pieces[cell.x, cell.y] = null;
            //Destroy the piece immediately
            DestroyImmediate(pieceToRemove.gameObject);
        }
        //Calculates & returns the piece between start and end locations
        Piece GetPieceBetween(Vector2Int start, Vector2Int end)
        {

            Vector2Int cell = Vector2Int.zero;
            cell.x = (start.x + end.x) / 2;
            cell.y = (start.y + end.y) / 2;
            return GetPiece(cell);
        }
        bool IsPieceTaken(Piece selected)
        {
            //Get the piece in between move
            Piece pieceBetween = GetPieceBetween(selected.oldCell, selected.cell);
            //if there is a piece between and the piece isnt the same color
            if(pieceBetween != null && pieceBetween.isWhite != selected.isWhite)
            {
                //Destroy the piece between
                RemovePiece(pieceBetween);
                //piece taken
                return true;

            }
            //piece not taken
            return false;
        }
    }
}
