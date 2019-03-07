using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{

    public class Grid : MonoBehaviour
    {

        public GameObject redPiecePrefab, whitePiecePrefab;

        public Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
        public Vector3 pieceOffset = new Vector3(.5f, 0, .5f);

        // ------------------------------------------------- //

        public Piece[,] pieces = new Piece[8, 8];

        // ------------------------------------------------- //

        void Start()
        {

            GenerateBoard();


        }

        // ------------------------------------------------- //

        void Update()
        {

            // Get mouse over information
            MouseOver();

            // Left click
            if (Input.GetMouseButtonDown(0))
            {
                // Try selecting piece
                selectedPiece = SelectPiece(mouseOverGridCoord);
            }

            // If there is a selected piece
            if (selectedPiece)
            {

                // Move the piece with the mouse
                DragPiece(selectedPiece);

                // If the button is released
                if (Input.GetMouseButtonUp(0))
                {
                    // Move piece to end position
                    TryMove(selectedPiece, mouseOverGridCoord);

                    // Let go of the piece
                    selectedPiece = null;
                }

            }


        }

        // ------------------------------------------------- //

        /// <summary>Moves a Piece to another coordinate on a 2D Grid.</summary>
        void MovePiece(Piece piece, Vector2Int newCell)
        {
            Vector2Int oldCell = piece.cell;

            // Update array
            pieces[oldCell.x, oldCell.y] = null;
            pieces[newCell.x, newCell.y] = piece;

            // Update data on piece
            piece.oldCell = oldCell;
            piece.cell = newCell;

            // Translate the piece to another location
            piece.transform.localPosition = GetWorldPosition(newCell);

        }
        
        // ------------------------------------------------- //

        /// <summary>Converts array coordinates to world position.</summary>
        /// <returns></returns>
        Vector3 GetWorldPosition(Vector2Int cell)
        {
            return new Vector3(cell.x, 0, cell.y) + boardOffset + pieceOffset;
        }

        // ------------------------------------------------- //

        #region Board and Piece Generation

        /// <summary>Generates a checker piece in specified coordinates.</summary>
        void GeneratePiece(GameObject prefab, Vector2Int desiredCell)
        {

            // Create instance
            GameObject clone = Instantiate(prefab, transform);

            // Get script component
            Piece piece = clone.GetComponent<Piece>();

            // Set the cell data for the first time
            piece.oldCell = desiredCell;
            piece.cell = desiredCell;

            // Reposition clone
            MovePiece(piece, desiredCell);

        }

        // ------------------------------------------------- //

        /// <summary>Generate all checker pieces.</summary>
        void GenerateBoard()
        {

            Vector2Int desiredCell = Vector2Int.zero;

            // White Team
            for (int y = 0; y < 3; y++)
            {
                bool oddRow = y % 2 == 0;

                // Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    desiredCell.x = oddRow ? x : x + 1;
                    desiredCell.y = y;

                    // Generate
                    GeneratePiece(whitePiecePrefab, desiredCell);

                }

            }

            // Red Team
            for (int y = 5; y < 8; y++)
            {
                bool oddRow = y % 2 == 0;

                // Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    desiredCell.x = oddRow ? x : x + 1;
                    desiredCell.y = y;

                    GeneratePiece(redPiecePrefab, desiredCell);

                }

            }

        }

        #endregion

        // ------------------------------------------------- //

        #region Player Interaction

        // For dragging and dropping the pieces

        /// <summary>Grid coordinates the mouse is over</summary>
        private Vector2Int mouseOverGridCoord;
        /// <summary>Piece that has been clicked and dragged</summary>
        private Piece selectedPiece;

        // ------------------------------------------------- //

        /// <summary>Returns the <see cref="Piece"/> </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        Piece GetPiece(Vector2Int cell)
        {
            return pieces[cell.x, cell.y];
        }

        /// <summary>True if the given coordinates are out of the board range.</summary>
        bool IsOutOfBounds(Vector2Int cell)
        {
            return cell.x < 0 || cell.x >= 8 ||
                   cell.y < 0 || cell.y >= 8;
        }

        /// <summary>Selects a piece on the 2D grid and returns it.</summary>
        Piece SelectPiece(Vector2Int cell)
        {
            // Check if X or Y is out of bounds
            if (IsOutOfBounds(cell))
                return null;    // No piece can be selected

            // Get the piece at the locations
            Piece piece = GetPiece(cell);

            if (piece == null)
            {
                Debug.LogWarning("No piece for SelectPiece() to return at in-bounds coordinates?!");
                Debug.LogWarning(cell.x + " / " + cell.y);
            }

            return piece;

        }


        void MouseOver()
        {
            // To physically select the object, we need to perform a raycast.
            // The raycast won't hit the piece directly, we are going to detect collisions
            // with the board and find the piece with the closest point.

            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(camRay, out hit))
            {
                // Convert mouse coordinates to 2D array coordinates
                mouseOverGridCoord.x = (int)(hit.point.x - boardOffset.x);
                mouseOverGridCoord.y = (int)(hit.point.z - boardOffset.z);
            }
            else
            {
                // Default to error (-1)
                mouseOverGridCoord = new Vector2Int(-1, -1);
            }
            
        }

        // Drags the selected piece using raycast location
        void DragPiece(Piece selected)
        {

            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(camRay, out hit))
            {

                // Update the position of the selected piece to hit point + offset
                selected.transform.position = hit.point + Vector3.up;

            }


        }
        
        // ------------------------------------------------- //

        /// <summary>Checks if the selected piece can move to the deisred cell based on Game Rules.</summary>
        bool TryMove(Piece selected, Vector2Int desiredCell)
        {

            Vector2Int startCell = selected.cell;

            if (!IsValidMove(selected, desiredCell))
            {
                // Move it back to original cell and return false, invalid move.
                MovePiece(selected, startCell);
                return false;
            }

            // Replace end coordinates with our selected piece
            MovePiece(selected, desiredCell);

            // Valid move detected!
            return true;

        }

        bool IsValidMove(Piece selected, Vector2Int desiredCell)
        {

            // Get direction of movement for some of the next few rules
            Vector2Int direction = selected.cell - desiredCell;

            #region Rule #01 - Is the piece out of bounds?

            // Is desired cell OOB?
            if (IsOutOfBounds(desiredCell))
            {
                Debug.Log("<color=red>Invalid - You cannot move out side of the map</color>");
                return false;
            }

            #endregion

            #region Rule #02 - Is the selected cell the same as desired?

            #endregion

            #region Rule #03 - Is the desired cell not empty?

            #endregion

            #region Rule #04 - Is there any forced moves?

            #endregion

            #region Rule #05 - Is the selected cell being dragged two cells over?

            #endregion

            #region Rule #06 - Is the piece not going in a diagonal cell?

            #endregion

            #region Rule #07 - Is the piece moving in the right direction?

            #endregion

            // Success! The move is valid.
            Debug.Log("<color=green>Success - Valid move detected!</color>");
            return true;
        }

        #endregion

        // ------------------------------------------------- //


    }

}