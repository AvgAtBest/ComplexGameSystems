using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class CheckersBoard : MonoBehaviour
    {
        [Tooltip("Prefab for Checker Pieces")]
        public GameObject whitePiecePrefab, blackPiecePrefab;
        [Tooltip("Where to attach the spawned pieces in the hierarchy")]
        public Vector3 boardOffset = Vector3.zero;
        public Vector3 pieceOffset = new Vector3(.5f, 0, .5f);
        public Transform checkersParent;

        void Start()
        {
            GenerateBoard();
        }

        public void GeneratePiece(int x, int y, bool isWhite)
        {
            //is the value of the white prefab true?        else default to black prefab
            GameObject prefab = isWhite ? whitePiecePrefab : blackPiecePrefab;
            //create clone of perfab
            GameObject clone = Instantiate(prefab, checkersParent);
            //Reposition clone
            clone.transform.localPosition = new Vector3(x, 0, y) + boardOffset + pieceOffset;

        }
        public void GenerateBoard()
        {
            //Generate White Team
            for (int y = 0; y < 3; y++)
            {
                bool oddRow = y % 2 == 0;
                // Loop through Columns
                for (int x = 0; x < 8; x += 2)
                {
                    //Generate Piece
                    GeneratePiece(x, y, true);
                }
            }
            // Generate Black Team
            for (int y = 5; y < 8; y++)
            {
                bool oddRow = y % 2 == 0;
                //Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    //Generate Piece
                    GeneratePiece(x, y, false);
                }
            }
        }

    }
}
