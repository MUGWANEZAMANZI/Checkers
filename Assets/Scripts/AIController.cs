using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AIController : MonoBehaviour
{
    private GameManager gameManager;
    private BoardManager boardManager;
    private List<MoveData> allValidMoves = new List<MoveData>();

    public struct MoveData
    {
        public Vector2Int from;
        public Vector2Int to;
        public bool isCapture;
        public int captureCount;
        public int evaluationScore; // Heuristic score for this move
        public System.Collections.Generic.List<Vector2Int> capturedPositions;
    }

    void Start()
    {
        gameManager = GetComponent<GameManager>();
        boardManager = FindObjectOfType<BoardManager>();
    }

    public void MakeMove()
    {
        allValidMoves.Clear();
        GetAllValidMoves();

        if (allValidMoves.Count == 0)
        {
            Debug.Log("AI has no valid moves!");
            gameManager.EndGame(true); // Player wins
            return;
        }

        int difficulty = gameManager.GetAIDifficulty();
        MoveData selectedMove;

        if (difficulty == 2)
        {
            // Level 2 (Hard): Prioritize capture moves
            selectedMove = GetHardMove();
        }
        else
        {
            // Level 1 (Easy): Random move
            selectedMove = GetEasyMove();
        }

        ExecuteMove(selectedMove);
    }

    private void GetAllValidMoves()
    {
        for (int x = 0; x < boardManager.boardSize; x++)
        {
            for (int y = 0; y < boardManager.boardSize; y++)
            {
                GameObject piece = boardManager.grids[x, y].occupant;
                
                if (piece != null)
                {
                    CheckerPiece checkerPiece = piece.GetComponent<CheckerPiece>();
                    if (checkerPiece != null && !checkerPiece.isPlayerPiece)
                    {
                        FindMovesForPiece(new Vector2Int(x, y), checkerPiece);
                    }
                }
            }
        }
    }

    private void FindMovesForPiece(Vector2Int pos, CheckerPiece piece)
    {
        // For kings: handle advanced diagonal movement
        if (piece.isKing)
        {
            FindKingMoves(pos, piece);
        }
        else
        {
            // Regular moves (1 square diagonally)
            Vector2Int[] directions = new[] { new Vector2Int(1, -1), new Vector2Int(-1, -1) }; // AI moves down

            foreach (Vector2Int dir in directions)
            {
                Vector2Int targetPos = pos + dir;
                
                if (IsWithinBounds(targetPos) && boardManager.isValidMove(pos, targetPos))
                {
                    allValidMoves.Add(new MoveData 
                    { 
                        from = pos, 
                        to = targetPos, 
                        isCapture = false,
                        captureCount = 0
                    });
                }

                // Jump moves (2 squares diagonally)
                Vector2Int jumpPos = pos + (dir * 2);
                if (IsWithinBounds(jumpPos) && boardManager.isValidMove(pos, jumpPos))
                {
                    Vector2Int capturedPos = pos + dir;
                    GameObject capturedPiece = boardManager.grids[capturedPos.x, capturedPos.y].occupant;

                    if (capturedPiece != null)
                    {
                        CheckerPiece capturedScript = capturedPiece.GetComponent<CheckerPiece>();
                        if (capturedScript != null && capturedScript.isPlayerPiece)
                        {
                            var md = new MoveData
                            {
                                from = pos,
                                to = jumpPos,
                                isCapture = true,
                                captureCount = 1,
                                capturedPositions = new System.Collections.Generic.List<Vector2Int> { capturedPos }
                            };
                            allValidMoves.Add(md);
                            // Also explore multi-jump sequences starting from this landing
                            FindRegularCaptures(pos, jumpPos, md.capturedPositions);
                        }
                    }
                }
            }
        }
    }

    // Explore multi-jump sequences for regular (non-king) pieces using DFS
    private void FindRegularCaptures(Vector2Int startPos, Vector2Int currentPos, System.Collections.Generic.List<Vector2Int> capturedSoFar)
    {
        Vector2Int[] jumpDirs = new[] { new Vector2Int(2, 2), new Vector2Int(2, -2), new Vector2Int(-2, 2), new Vector2Int(-2, -2) };

        bool extended = false;

        foreach (var jumpDir in jumpDirs)
        {
            Vector2Int landing = currentPos + jumpDir;
            if (!IsWithinBounds(landing)) continue;
            // landing must be empty and valid
            if (!boardManager.isValidMove(currentPos, landing)) continue;

            Vector2Int mid = currentPos + new Vector2Int(jumpDir.x / 2, jumpDir.y / 2);
            GameObject midPiece = boardManager.grids[mid.x, mid.y].occupant;
            if (midPiece == null) continue;
            CheckerPiece midScript = midPiece.GetComponent<CheckerPiece>();
            if (midScript == null || !midScript.isPlayerPiece) continue; // must capture player pieces

            // avoid capturing same piece twice in a sequence
            if (capturedSoFar.Contains(mid)) continue;

            extended = true;
            var newCaptured = new System.Collections.Generic.List<Vector2Int>(capturedSoFar) { mid };

            var md = new MoveData
            {
                from = startPos,
                to = landing,
                isCapture = true,
                captureCount = newCaptured.Count,
                capturedPositions = new System.Collections.Generic.List<Vector2Int>(newCaptured)
            };

            allValidMoves.Add(md);

            // recurse to find further captures from landing
            FindRegularCaptures(startPos, landing, newCaptured);
        }
    }
    
    private void FindKingMoves(Vector2Int pos, CheckerPiece piece)
    {
        // Kings can move in all 4 diagonal directions
        Vector2Int[] directions = new[] { 
            new Vector2Int(1, 1), new Vector2Int(1, -1), 
            new Vector2Int(-1, 1), new Vector2Int(-1, -1) 
        };
        
        foreach (Vector2Int dir in directions)
        {
            // Regular king moves (1 square)
            Vector2Int movePos = pos + dir;
            if (IsWithinBounds(movePos) && boardManager.isValidMove(pos, movePos))
            {
                allValidMoves.Add(new MoveData
                {
                    from = pos,
                    to = movePos,
                    isCapture = false,
                    captureCount = 0
                });
            }
            
            // King capture moves - check along the diagonal line
            FindKingCaptures(pos, dir);
        }
    }
    
    private void FindKingCaptures(Vector2Int startPos, Vector2Int direction)
    {
        // Travel along diagonal and find valid capture sequences
        Vector2Int currentPos = startPos + direction;
        List<Vector2Int> capturedPositions = new List<Vector2Int>();
        int consecutiveEnemies = 0;
        
        while (IsWithinBounds(currentPos))
        {
            GameObject piece = boardManager.grids[currentPos.x, currentPos.y].occupant;
            
            if (piece == null)
            {
                // Empty square - valid landing position after captures
                if (capturedPositions.Count > 0)
                {
                    allValidMoves.Add(new MoveData
                    {
                        from = startPos,
                        to = currentPos,
                        isCapture = true,
                        captureCount = capturedPositions.Count,
                        capturedPositions = new System.Collections.Generic.List<Vector2Int>(capturedPositions)
                    });
                }
                consecutiveEnemies = 0;
                currentPos += direction;
            }
            else
            {
                CheckerPiece checkerPiece = piece.GetComponent<CheckerPiece>();
                
                // Player piece - can capture
                if (checkerPiece != null && checkerPiece.isPlayerPiece)
                {
                    consecutiveEnemies++;
                    
                    // Cannot jump over 2+ consecutive enemies
                    if (consecutiveEnemies > 1)
                    {
                        break; // Stop this diagonal
                    }
                    
                    capturedPositions.Add(currentPos);
                    currentPos += direction;
                }
                else
                {
                    // Own piece or different piece - stop
                    break;
                }
            }
        }
    }

    private MoveData GetHardMove()
    {
        // Level 2 (Hard): Intelligent AI with Heuristic Evaluation
        // 1. Capture moves (MANDATORY if available)
        // 2. Evaluate all moves with scoring system
        // 3. Look-ahead to avoid being captured
        
        var captureMoves = allValidMoves.Where(m => m.isCapture).ToList();

        if (captureMoves.Count > 0)
        {
            // MANDATORY CAPTURE: Evaluate and pick the best capture
            foreach (var move in captureMoves)
            {
                allValidMoves[allValidMoves.IndexOf(move)] = EvaluateMove(move);
            }
            return captureMoves.OrderByDescending(m => m.evaluationScore).First();
        }

        // No captures - evaluate all moves with heuristic scoring
        for (int i = 0; i < allValidMoves.Count; i++)
        {
            allValidMoves[i] = EvaluateMove(allValidMoves[i]);
        }
        
        // Return the move with the highest score
        var bestMove = allValidMoves.OrderByDescending(m => m.evaluationScore).First();
        Debug.Log($"AI chose move from {bestMove.from} to {bestMove.to} with score: {bestMove.evaluationScore}");
        return bestMove;
    }
    
    private MoveData EvaluateMove(MoveData move)
    {
        int score = 0;
        
        // PRIORITY 1: Capture moves (+100 points)
        if (move.isCapture)
        {
            score += 100 * move.captureCount;
            Debug.Log($"Move {move.from}->{move.to}: Capture bonus +{100 * move.captureCount}");
        }
        
        // PRIORITY 2: Kinging moves (+50 points)
        if (WillBecomeKing(move))
        {
            score += 50;
            Debug.Log($"Move {move.from}->{move.to}: Kinging bonus +50");
        }
        
        // PRIORITY 3: Safe move - won't be captured next turn (+10 points)
        if (!CanPlayerCaptureOnNextTurn(move.to))
        {
            score += 10;
        }
        else
        {
            // PENALTY: Move puts piece in danger (-30 points)
            score -= 30;
            Debug.Log($"Move {move.from}->{move.to}: Danger penalty -30");
        }
        
        // PRIORITY 4: Center control (+5 points)
        if (IsInCenter(move.to))
        {
            score += 5;
        }
        
        // PRIORITY 5: Compact formation - stay close to other AI pieces (+3 points)
        int nearbyAllies = CountNearbyAllies(move.to);
        score += nearbyAllies * 3;
        
        // PRIORITY 6: Aggressive positioning - move closer to player pieces (+2 points)
        if (IsMovingTowardPlayer(move))
        {
            score += 2;
        }
        
        // PRIORITY 7: Protect the back row (+4 points)
        if (IsProtectingBackRow(move))
        {
            score += 4;
        }
        
        move.evaluationScore = score;
        return move;
    }
    
    private bool CanPlayerCaptureOnNextTurn(Vector2Int aiPiecePos)
    {
        // Check if player can capture this AI piece on their next turn
        Vector2Int[] jumpDirections = { 
            new Vector2Int(2, 2), new Vector2Int(2, -2), 
            new Vector2Int(-2, 2), new Vector2Int(-2, -2) 
        };
        
        foreach (Vector2Int jumpDir in jumpDirections)
        {
            Vector2Int playerPos = aiPiecePos - jumpDir;
            Vector2Int landingPos = aiPiecePos + jumpDir;
            
            if (!IsWithinBounds(playerPos) || !IsWithinBounds(landingPos))
                continue;
            
            GameObject potentialPlayer = boardManager.grids[playerPos.x, playerPos.y].occupant;
            
            if (potentialPlayer != null)
            {
                CheckerPiece playerPiece = potentialPlayer.GetComponent<CheckerPiece>();
                if (playerPiece != null && playerPiece.isPlayerPiece)
                {
                    // Check if landing position is valid
                    if (boardManager.isValidMove(playerPos, landingPos))
                    {
                        return true; // AI piece is in danger!
                    }
                }
            }
        }
        
        return false; // Safe position
    }
    
    private bool IsInCenter(Vector2Int pos)
    {
        // Center of 10x10 board is roughly positions 3-6 on both axes
        int centerStart = boardManager.boardSize / 2 - 2;
        int centerEnd = boardManager.boardSize / 2 + 1;
        
        return pos.x >= centerStart && pos.x <= centerEnd && 
               pos.y >= centerStart && pos.y <= centerEnd;
    }
    
    private int CountNearbyAllies(Vector2Int pos)
    {
        int count = 0;
        Vector2Int[] adjacentOffsets = {
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1),
            new Vector2Int(2, 0), new Vector2Int(-2, 0),
            new Vector2Int(0, 2), new Vector2Int(0, -2)
        };
        
        foreach (Vector2Int offset in adjacentOffsets)
        {
            Vector2Int checkPos = pos + offset;
            if (!IsWithinBounds(checkPos)) continue;
            
            GameObject piece = boardManager.grids[checkPos.x, checkPos.y].occupant;
            if (piece != null)
            {
                CheckerPiece checkerPiece = piece.GetComponent<CheckerPiece>();
                if (checkerPiece != null && !checkerPiece.isPlayerPiece)
                {
                    count++;
                }
            }
        }
        
        return count;
    }
    
    private bool IsMovingTowardPlayer(MoveData move)
    {
        // AI moves down (decreasing y), so moving toward player means y is decreasing
        return move.to.y < move.from.y;
    }
    
    private bool IsProtectingBackRow(MoveData move)
    {
        // Back row for AI is y = boardSize - 1 (top row)
        // Keeping pieces on rows 8-9 helps protect from breakthrough
        return move.to.y >= boardManager.boardSize - 2;
    }

    
    private bool WillBecomeKing(MoveData move)
    {
        // AI pieces become king when they reach y = 0 (bottom row)
        return move.to.y == 0;
    }

    private MoveData GetEasyMove()
    {
        // Level 1 (Easy): Random move
        return allValidMoves[Random.Range(0, allValidMoves.Count)];
    }

    private void ExecuteMove(MoveData moveData)
    {
        Vector2Int from = moveData.from;
        Vector2Int to = moveData.to;

        // Move piece
        Cell toCell = boardManager.grids[to.x, to.y];
        Cell fromCell = boardManager.grids[from.x, from.y];

        GameObject movingPiece = fromCell.occupant;
        toCell.occupant = movingPiece;
        fromCell.occupant = null;

        // Update position
        movingPiece.transform.position = boardManager.tileMap.GetCellCenterWorld(
            new Vector3Int(to.x, to.y, 0));

        // Handle capture: if MoveData contains capturedPositions, remove them all (works for multi-jump and king captures)
        if (moveData.isCapture)
        {
            if (moveData.capturedPositions != null && moveData.capturedPositions.Count > 0)
            {
                foreach (var capPos in moveData.capturedPositions)
                {
                    if (!IsWithinBounds(capPos)) continue;
                    GameObject capturedPiece = boardManager.grids[capPos.x, capPos.y].occupant;
                    if (capturedPiece != null)
                    {
                        CheckerPiece capturedScript = capturedPiece.GetComponent<CheckerPiece>();
                        if (capturedScript != null && capturedScript.isPlayerPiece)
                        {
                            capturedScript.Capture();
                            gameManager.OnPiecesCaptured(1, false);
                        }
                        boardManager.grids[capPos.x, capPos.y].occupant = null;
                    }
                }
            }
            else
            {
                // Fallback: single capture between from and to
                Vector2Int capturedPos = (from + to) / 2;
                if (IsWithinBounds(capturedPos))
                {
                    GameObject capturedPiece = boardManager.grids[capturedPos.x, capturedPos.y].occupant;
                    if (capturedPiece != null)
                    {
                        CheckerPiece capturedScript = capturedPiece.GetComponent<CheckerPiece>();
                        if (capturedScript != null)
                        {
                            capturedScript.Capture();
                            gameManager.OnPiecesCaptured(1, false);
                        }
                        boardManager.grids[capturedPos.x, capturedPos.y].occupant = null;
                    }
                }
            }
        }

        // Check for kinging
        CheckerPiece movedPiece = movingPiece.GetComponent<CheckerPiece>();
        if (movedPiece != null)
        {
            movedPiece.CheckForKinging(to.y, boardManager.boardSize);
        }

        Debug.Log($"AI moved from {from} to {to}");
    }

    private bool IsWithinBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < boardManager.boardSize && 
               pos.y >= 0 && pos.y < boardManager.boardSize;
    }
}
