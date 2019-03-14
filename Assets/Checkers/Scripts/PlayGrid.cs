using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{

    using ForcedMoves = Dictionary<Piece, List<Vector2Int>>;

    public class PlayGrid : MonoBehaviour
    {

        public GameObject redPiecePrefab, whitePiecePrefab;

        public Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
        public Vector3 pieceOffset = new Vector3(.5f, 0, .5f);

        private ForcedMoves forcedMoves = new ForcedMoves();

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

        #endregion

        // ------------------------------------------------- //

        #region Moving Pieces - Rules

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

            #region Rule #01 - Is the piece out of bounds?

            // Is desired cell OOB?
            if (IsOutOfBounds(desiredCell))
            {
                Debug.Log("<color=red>Invalid - You cannot move out side of the map</color>");
                return false;
            }

            #endregion

            #region Rule #02 - Is the selected cell the same as desired?

            if (selected.cell == desiredCell)
            {
                Debug.Log("<color=red>Invalid - Putting pieces back don't count as a valid move.</color>");
                return false;
            }

            #endregion

            #region Rule #03 - Is the desired cell not empty?

            if (GetPiece(desiredCell))
            {
                Debug.Log("<color=red>Invalid - You can't go on top of other pieces</color>");
                return false;
            }

            #endregion

            #region Rule #04 - Is there any forced moves?

            // When playing Checkers, you can be put in situations where you are forced to make a move. 
            // In order to do this, we need to:
            //  1. Detect for Forced Moves; Using various algorithms to solve this
            //  2. Store the Forced Moves; We need to store them to force the player to use them once detected
            //  3. Check if player has forced moves; Based on the ones that are stored, which ones apply to the current player (Red / White)
            // There is a pretty big function that's almost as big as TryMove called "CheckForcedMove".
            // We're going to implement this and look at it in sections (Checks).

            // Is there any forced moves?
            if (HasForcedMoves(selected))
            {
                // If it is not a forced move
                if (!IsForcedMove(selected, desiredCell))
                {
                    Debug.Log("<color=red>Invalid - You have to use forced moves!</color>");
                    return false;
                }
            }

            #endregion

            // Get direction of movement for some of the next few rules
            Vector2Int direction = selected.cell - desiredCell;

            #region Rule #05 - Is the selected cell being dragged two cells over?

            // Is the piece moved two spaces?
            if (direction.magnitude > 2)
            {
                // Is there no forced moves?
                if (forcedMoves.Count == 0)
                {
                    Debug.Log("<color=red>Invalid - You can only move two spaces if there are forced moves on selected piece</color>");
                    return false;
                }
            }

            #endregion

            #region Rule #06 - Is the piece not going in a diagonal cell?

            // Is the player not moving diagonally?
            if (Mathf.Abs(direction.x) != Mathf.Abs(direction.y))
            {
                Debug.Log("<color=red>Invalid - You have to be moving diagonally.</color>");
                return false;
            }

            #endregion

            #region Rule #07 - Is the piece moving in the right direction?

            // Is the selected piece not a king?
            if (!selectedPiece.isKing)
            {
                Debug.Log(direction);
                if (selectedPiece.isWhite)
                {
                    // Is it moving down?
                    if (direction.y > 0)
                    {
                        Debug.Log("<color=red>Invalid - Can't move a white piece backwards.</color>");
                        return false;
                    }
                }
                else
                {
                    // The piece is red.
                    // Is it moving up?
                    if (direction.y < 0)
                    {
                        Debug.Log("<color=red>Invalid - Can't move a red piece backwards.</color>");
                        return false;
                    }
                }
            }

            #endregion

            // Success! The move is valid.
            Debug.Log("<color=green>Success - Valid move detected!</color>");
            return true;
        }

        #endregion

        // ------------------------------------------------- //

        #region Moving Pieces - Forced Moves
        
        /// <summary>Checks if a piece has forced pieces based on color.</summary>
        bool HasForcedMoves(Piece selected)
        {
            foreach (var move in forcedMoves)
            {
                Piece piece = move.Key;
                // Is the piece being forced to move the same color as selected piece?
                if (piece.isWhite == selected.isWhite)
                {
                    // Has forced moves!
                    return true;
                }
            }
            // Does not have any forced moves
            return false;
        }

        // ------------------------------------------------- //

        bool IsForcedMove(Piece selected, Vector2Int desiredCell)
        {
            // Does the selected piece have a forced move?
            if (forcedMoves.ContainsKey(selected))
            {
                // Is there any forced moves for this piece?
                if (forcedMoves[selected].Contains(desiredCell))
                {
                    // It is a forced move
                    return true;
                }
            }
            // It is not a forced move
            return false;
        }

        // ------------------------------------------------- //

        /// <summary>Scans the board for forced moves.</summary>
        void DetectForcedMoves()
        {

            // Refresh forced moves
            forcedMoves = new ForcedMoves();

            // Loop through entire board
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    // Get piece at index
                    Piece pieceToCheck = pieces[x, y];

                    // If the piece exists
                    if (pieceToCheck)
                    {
                        // Check piece for forced moves
                        CheckForcedMove(pieceToCheck);
                    }
                }
            }

        }

        // ------------------------------------------------- //

        /// <summary>Detect if there is a forced move for a given piece.</summary>
        void CheckForcedMove(Piece piece)
        {
            // Get cell location of piece
            Vector2Int cell = piece.cell;

            // Loop through adjacent cells of a cell
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    // Create offset cell from index
                    Vector2Int offset = new Vector2Int(x, y);

                    // Create a new X from piece coordinates using offset
                    Vector2Int desiredCell = cell + offset;

                    #region Check #01 - Correct Direction?

                    // Is the piece not king?
                    if (!piece.isKing)
                    {
                        // Is the piece white?
                        if (piece.isWhite)
                        {
                            // Is the piece moving backwards? If yes, invalid move. Check next.
                            if (desiredCell.y < cell.y)
                                continue;
                        }
                        else
                        {
                            // The piece is red.
                            // Is the piece moving backwards? If yes, invalid move. Check next.
                            if (desiredCell.y > cell.y)
                                continue;
                        }
                    }

                    #endregion

                    #region Check #02 - Is the adjacent cell out of bounds?

                    // Is desired cell out of bounds?
                    if (IsOutOfBounds(desiredCell))
                        continue;

                    #endregion

                    // Try getting the piece at coordinates
                    Piece detectedPiece = GetPiece(desiredCell);

                    #region Check #03 - Is the desired cell empty?

                    // Is there a detected piece? If not, check next.
                    if (detectedPiece == null)
                        continue;

                    #endregion

                    #region Check #04 - Is the detected piece the same color?

                    // Is the detected piece the same color? If yes, invalid. Check next.
                    if (detectedPiece.isWhite == piece.isWhite)
                        continue;

                    #endregion

                    // Try getting the diagonal cell next to detected piece
                    Vector2Int jumpCell = cell + (offset * 2);

                    #region Check #05 - Is the jump cell out of bounds?

                    // Is the detination cell out of bounds? If yes, invalid. Check the next one.
                    if (IsOutOfBounds(jumpCell))
                        continue;

                    #endregion

                    #region Check #06 - Is there a piece at the jump cell?

                    // Get piece next to the one we want to jump
                    detectedPiece = GetPiece(jumpCell);

                    // Is there a piece there? If yes, invalid. Check the next one.
                    if (detectedPiece)
                        continue;

                    #endregion

                    // If code execution makes it to this point, a forced move has been detected!

                    #region Store Forced Move

                    // Check if forced moves contains the piece we're currently holding
                    if (!forcedMoves.ContainsKey(piece))
                    {
                        // Add it to list of forced moves
                        forcedMoves.Add(piece, new List<Vector2Int>());
                    }

                    // Add the jump cell to the piece's forced moves
                    forcedMoves[piece].Add(jumpCell);

                    #endregion
                }
            }

        }

        #endregion

    }

}