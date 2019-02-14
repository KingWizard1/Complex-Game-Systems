using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class CheckersBoard : MonoBehaviour
    {
        [Tooltip("Prefabs for Checker Pieces")]
        public GameObject whitePiecePrefab, blackPiecePrefab;

        [Tooltip("Where to attach the spawned pieces in the Hierarchy")]
        public Transform checkersParent;

        public Vector3 boardOffset = new Vector3(4f, 0, -4f);
        public Vector3 pieceOffset = new Vector3(.5f, 0, .5f);

        public float rayDistance = 1000f;
        public LayerMask hitLayers;

        public Piece[,] pieces = new Piece[8,8];

        // ------------------------------------------------- //

        /*
         * isWhiteTurn  = Is it current player's turn or opponent?
         * hasKilled    = Did the player get killed?
         */
        private bool isWhiteTurn = true, hasKilled;

        private Vector2 mouseOver, startDrag, endDrag;

        private Piece selectedPiece;

        // ------------------------------------------------- //

        void Start()
        {
            GenerateBoard();
        }

        // ------------------------------------------------- //

        private void Update()
        {
            // Update mouse over information
            MouseOver();

            if (isWhiteTurn)
            {

                // Get x, y coordinates of selected mouse over
                int x = (int)mouseOver.x;
                int y = (int)mouseOver.y;

                // Detect selected piece
                if (Input.GetMouseButtonDown(0))
                {
                    selectedPiece = SelectPiece(x, y);
                    startDrag = new Vector2(x, y);
                }

                // Move the selected piece to new coordinate
                if (selectedPiece)
                {
                    // Move the piece with the mouse
                    DragPiece(selectedPiece);
                }

                // If button is released
                if (Input.GetMouseButtonUp(0))
                {
                    endDrag = new Vector2(x, y);
                    TryMove(startDrag, endDrag);
                    selectedPiece = null;
                }

            }
        }

        // ------------------------------------------------- //

        /// <summary>
        /// Generates a Checker Piece in specified coordinates
        /// </summary>
        /// <param name="x">X Location</param>
        /// <param name="y">Y Location</param>
        public void GeneratePiece(int x, int y, bool isWhite)
        {
            // What prefab are we using (white or black) ?
            GameObject prefab = isWhite ? whitePiecePrefab : blackPiecePrefab;

            // Generate Instance of prefab
            GameObject clone = Instantiate(prefab, checkersParent);

            // Get the piece component and tell it its initial loc.
            Piece p = clone.GetComponent<Piece>();
            p.x = x;
            p.y = y;

            // Reposition clone
            MovePiece(p, x, y);
        }

        /// <summary>
        /// Clears and re-generates entire board 
        /// </summary>
        public void GenerateBoard()
        {
            // Generate White Team
            for (int y = 0; y < 3; y++)
            {
                bool oddRow = y % 2 == 0;
                // Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    // Generate Piece
                    GeneratePiece(oddRow ? x : x + 1, y, true);
                }
            }
            // Generate Black Team
            for (int y = 5; y < 8; y++)
            {
                bool oddRow = y % 2 == 0;
                // Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    // Generate Piece
                    GeneratePiece(oddRow ? x : x + 1, y, false);
                }
            }
        }

        // ------------------------------------------------- //

        /// <summary>
        /// Selects a piece from the 2D grid and returns it.
        /// </summary>
        private Piece SelectPiece(int x, int y)
        {

            // Check if x or y is out of bounds
            if (OutOfBounds(x, y))
                return null;

            // Get piece from array location
            Piece piece = pieces[x, y];

            // If the piece is not null, store its original position, then return it.
            // Otherwise, return null.
            if (piece)
                return piece;
            else
                return null;


        }

        /// <summary>
        /// Moves a Piece to another coordinate on the grid.
        /// </summary>
        /// <param name="p">The Piece to move.</param>
        /// <param name="x">New X Location</param>
        /// <param name="y">New Y Location</param>
        private void MovePiece(Piece p, int x, int y)
        {

            // Update the array
            pieces[p.x, p.y] = null;
            pieces[  x,   y] = p;

            // Update the piece so it also knows where it is
            p.x = x;
            p.y = y;

            // Move piece to new location
            p.transform.localPosition = new Vector3(x, 0, y) + boardOffset + pieceOffset;

        }

        // ------------------------------------------------- //

        /// <summary>
        /// Updating when the pieces have been selected by the player.
        /// </summary>
        private void MouseOver()
        {

            //if (!Camera.main)
            //{
            //    Debug.LogError("Unable to find main camera");
            //    return;
            //}

            // Perform Raycast from mouse position
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(camRay, out hit, rayDistance, hitLayers))
            {
                // Convert mouse coordinates to 2D array coordinates
                mouseOver.x = (int)(hit.point.x - boardOffset.x);
                mouseOver.y = (int)(hit.point.z - boardOffset.z);
            }
            else
            {
                // Default to error (-1)
                mouseOver.x = -1;
                mouseOver.y = -1;
            }
            
        }

        /// <summary>
        /// Drags the selected piece using Raycast location.
        /// </summary>
        /// <param name="selected"></param>
        private void DragPiece(Piece selected)
        {

            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Detects collision mouse ray hit point
            if (Physics.Raycast(camRay, out hit, rayDistance, hitLayers))
            {
                // Updates position of selected piece to hit point + offset.
                selected.transform.position = hit.point + Vector3.up;
            }

        }


        /// <summary>
        /// Tries moving a piece from x1+y1 to x2+y2 coordinates.
        /// </summary>
        private void TryMove(Vector2 startPos, Vector2 desiredPos)
        {
            int x1 = (int)startPos.x;
            int y1 = (int)startPos.y;
            int x2 = (int)desiredPos.x;
            int y2 = (int)desiredPos.y;

            // Store start drag
            startDrag = new Vector2(x1, y1);

            // Store end drag
            endDrag = new Vector2(x2, y2);

            // If there is a selected piece
            if (selectedPiece)
            {
                // Check if desired drop location is within bounds
                if (OutOfBounds(x2, y2))
                {
                    // Move it back to its original position
                    MovePiece(selectedPiece, x1, y1);
                    return;
                }


                // Move the piece if the desired drop position is valid
                if (IsValidMove(startPos, desiredPos))
                {
                    MovePiece(selectedPiece, x2, y2);
                }
                else
                    MovePiece(selectedPiece, x1, y1);
                

            }



        }

        // ------------------------------------------------- //

        private bool OutOfBounds(int x, int y)
        {
            return x < 0 || x >= 8 || y < 0 || y >= 8;
        }

        private bool IsValidMove(Vector2 startPos, Vector2 desiredPos)
        {
            int x1 = (int)startPos.x;
            int y1 = (int)startPos.y;
            int x2 = (int)desiredPos.x;
            int y2 = (int)desiredPos.y;

            // Allow drop on the square the piece is on
            if (startPos == desiredPos)
                return true;

            // If you are moving on top of another piece
            if (pieces[x2, y2])
                return false;

            // Valid move!
            return true;
        }

    }
}