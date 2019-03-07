using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        return cell.x < 0 || cell.x >= 8 || cell.y < 0 || cell.x >= 8;
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
        if(Physics.Raycast(camRay, out hit))
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

        //is it not a valid move
        if(!ValidMove(selected, desiredCell))
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
        //get direction of movement for some of the next few rules
        Vector2Int direction = selected.cell - desiredCell;        
        //is the desired cell out of bounds
        if (IsOutOfBounds(desiredCell))
        {

            Debug.Log("<color=red>Invalid - You cannot move outside of the board</color>");
            return false;
            //cant freaking move there bud
        }
        #endregion
        return true;
    }
}
