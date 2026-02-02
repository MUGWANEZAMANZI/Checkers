using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CheckerPiece selectedPiece;
    private GameManager gameManager;
    private BoardManager boardManager;
    private Vector2Int selectedPiecePos;
    private bool hasMoreJumps = false;

    void Start()
    {
        gameManager = GetComponent<GameManager>();
        boardManager = FindObjectOfType<BoardManager>();
        
        // Debug: Check if pieces have colliders after a short delay
        Invoke(nameof(DebugCheckPieces), 1f);
    }
    
    private void DebugCheckPieces()
    {
        // Find all pieces and check their colliders
        CheckerPiece[] allPieces = FindObjectsOfType<CheckerPiece>();
        Debug.Log($"Found {allPieces.Length} total pieces in scene");
        
        foreach (var piece in allPieces)
        {
            BoxCollider2D col = piece.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                Debug.Log($"Piece {piece.name}: Position={piece.transform.position}, Collider Size={col.size}, IsTrigger={col.isTrigger}");
            }
            else
            {
                Debug.LogError($"Piece {piece.name} has NO COLLIDER!");
            }
        }
    }

    void Update()
    {
        if (gameManager.GetCurrentState() != GameState.PlayerTurn)
        {
            Debug.Log($"Not player turn. Current state: {gameManager.GetCurrentState()}");
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Get mouse position with proper Z coordinate for 2D
            Vector3 mousePos3D = Input.mousePosition;
            mousePos3D.z = Mathf.Abs(Camera.main.transform.position.z); // Distance from camera to game plane
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(mousePos3D);
            Debug.Log($"Mouse clicked at screen: {Input.mousePosition} -> world: {mousePos}");
            HandleMouseClick(mousePos);
        }
    }

    private void HandleMouseClick(Vector2 mousePos)
    {
        // Draw debug line to visualize raycast
        Debug.DrawLine(mousePos, mousePos + Vector2.up * 0.1f, Color.red, 2f);
        
        // Try raycast with distance
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity);
        
        // Also try getting all colliders at the point
        Collider2D[] colliders = Physics2D.OverlapPointAll(mousePos);
        Debug.Log($"Raycast hit: {(hit.collider != null ? hit.collider.gameObject.name : "Nothing")} | Colliders at point: {colliders.Length}");
        
        if (colliders.Length > 0)
        {
            Debug.Log($"Found {colliders.Length} colliders at click position:");
            foreach (var col in colliders)
            {
                Debug.Log($"  - {col.gameObject.name} (Tag: {col.tag})");
            }
        }

        if (hit.collider != null)
        {
            GameObject clickedObj = hit.collider.gameObject;
            CheckerPiece piece = clickedObj.GetComponent<CheckerPiece>();

            // Check if clicked on a player piece
            if (piece != null && piece.isPlayerPiece)
            {
                Debug.Log($"✓ Clicked on PLAYER piece: {clickedObj.name} - Ready to move!");
                SelectPiece(piece);
            }
            else if (selectedPiece != null)
            {
                Debug.Log($"Clicked on non-player object while piece selected. Trying to move...");
                // Attempt to move to clicked position
                Vector2Int targetPos = WorldToGridPosition(mousePos);
                Debug.Log($"→ Moving to grid position: {targetPos}");
                TryMove(selectedPiecePos, targetPos);
            }
            else
            {
                Debug.Log("Clicked on object but it's not a player piece and no piece is selected");
            }
        }
        else if (selectedPiece != null)
        {
            Debug.Log("Clicked on empty space with piece selected. Trying to move...");
            // Clicked on empty space - try to move there
            Vector2Int targetPos = WorldToGridPosition(mousePos);
            Debug.Log($"→ Moving to grid position: {targetPos}");
            TryMove(selectedPiecePos, targetPos);
        }
        else
        {
            Debug.Log("Clicked on empty space but no piece is selected");
        }
    }

    private void SelectPiece(CheckerPiece piece)
    {
        // Safety check
        if (boardManager == null || boardManager.grids == null)
        {
            Debug.LogError("BoardManager not properly initialized!");
            return;
        }

        // Deselect previous piece
        if (selectedPiece != null)
        {
            Debug.Log($"Deselecting previous piece");
            // Visual feedback: deselect (you can add outline removal here)
        }

        selectedPiece = piece;
        // Get position from the grid by finding the piece
        selectedPiecePos = FindPiecePosition(piece.gameObject);
        Debug.Log($"★ PIECE SELECTED at {selectedPiecePos} - Click anywhere to move!");
        // Visual feedback: highlight selected piece
    }

    private void TryMove(Vector2Int from, Vector2Int to)
    {
        if (boardManager == null)
        {
            Debug.LogError("BoardManager is null!");
            return;
        }

        Debug.Log($"Attempting move from {from} to {to}");

        // Validate basic move
        if (!boardManager.isValidMove(from, to))
        {
            Debug.LogWarning($"✗ Invalid move from {from} to {to}!");
            return;
        }

        Debug.Log($"✓ Valid move confirmed!");

        // Check if it's a capture move (jump)
        bool isCapture = IsJumpMove(from, to);
        
        // MANDATORY CAPTURE RULE: If player has any capture moves available, they MUST capture
        if (!isCapture && HasAnyCaptureAvailable())
        {
            Debug.LogWarning($"✗ You must capture when a capture move is available!");
            return;
        }

        if (isCapture)
        {
            Debug.Log($"Capture move detected!");
            ExecuteCapture(from, to);
            hasMoreJumps = HasMoreJumps(to);
            
            if (!hasMoreJumps)
            {
                Debug.Log($"No more jumps available. Switching to AI turn.");
                gameManager.ChangeState(GameState.AITurn);
                selectedPiece = null;
            }
            else
            {
                // Update piece position for multi-jump
                selectedPiecePos = to;
                Debug.Log($"Multi-jump available! Piece can jump again from {to}");
            }
        }
        else
        {
            Debug.Log($"Regular move - executing now!");
            ExecuteMove(from, to);
            Debug.Log($"Move complete. Switching to AI turn.");
            gameManager.ChangeState(GameState.AITurn);
            selectedPiece = null;
        }
    }

    private bool IsJumpMove(Vector2Int from, Vector2Int to)
    {
        int distX = Mathf.Abs(to.x - from.x);
        int distY = Mathf.Abs(to.y - from.y);
        
        // Jump is 2 squares diagonally
        return distX == 2 && distY == 2;
    }

    private void ExecuteMove(Vector2Int from, Vector2Int to)
    {
        // Move piece in grid
        Cell toCell = boardManager.grids[to.x, to.y];
        Cell fromCell = boardManager.grids[from.x, from.y];

        GameObject movingPiece = fromCell.occupant;
        toCell.occupant = movingPiece;
        fromCell.occupant = null;

        // Update piece position in world
        Vector3 newWorldPos = boardManager.tileMap.GetCellCenterWorld(new Vector3Int(to.x, to.y, 0));
        newWorldPos.z = -1f; // Keep Z position for rendering
        selectedPiece.transform.position = newWorldPos;

        // Check for kinging
        selectedPiece.CheckForKinging(to.y, boardManager.boardSize);

        Debug.Log($"✓ Piece moved from {from} to {to}. New position: {newWorldPos}");
    }

    private void ExecuteCapture(Vector2Int from, Vector2Int to)
    {
        // Find captured piece (between from and to)
        Vector2Int capturedPos = (from + to) / 2;
        GameObject capturedPiece = boardManager.grids[capturedPos.x, capturedPos.y].occupant;

        if (capturedPiece != null)
        {
            CheckerPiece capturedScript = capturedPiece.GetComponent<CheckerPiece>();
            if (capturedScript != null)
            {
                capturedScript.Capture();
                gameManager.OnPiecesCaptured(1, true); // Player captured an AI piece
            }
            boardManager.grids[capturedPos.x, capturedPos.y].occupant = null;
        }

        // Move the piece
        ExecuteMove(from, to);
    }

    private bool HasAnyCaptureAvailable()
    {
        // Check if player has ANY capture moves available
        for (int x = 0; x < boardManager.boardSize; x++)
        {
            for (int y = 0; y < boardManager.boardSize; y++)
            {
                GameObject piece = boardManager.grids[x, y].occupant;
                if (piece != null)
                {
                    CheckerPiece checkerPiece = piece.GetComponent<CheckerPiece>();
                    if (checkerPiece != null && checkerPiece.isPlayerPiece)
                    {
                        if (HasCaptureMovesForPiece(new Vector2Int(x, y)))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    
    private bool HasCaptureMovesForPiece(Vector2Int pos)
    {
        Vector2Int[] directions = { new Vector2Int(2, 2), new Vector2Int(2, -2), 
                                   new Vector2Int(-2, 2), new Vector2Int(-2, -2) };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int targetPos = pos + dir;
            
            if (!boardManager.isValidMove(pos, targetPos))
                continue;
            
            Vector2Int middlePos = pos + (dir / 2);
            GameObject middlePiece = boardManager.grids[middlePos.x, middlePos.y].occupant;
            
            if (middlePiece != null)
            {
                CheckerPiece middleScript = middlePiece.GetComponent<CheckerPiece>();
                if (middleScript != null && !middleScript.isPlayerPiece)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool HasMoreJumps(Vector2Int currentPos)
    {
        // Check if piece can make additional jumps
        Vector2Int[] directions = { new Vector2Int(2, 2), new Vector2Int(2, -2), 
                                   new Vector2Int(-2, 2), new Vector2Int(-2, -2) };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int targetPos = currentPos + dir;
            
            // Check if target position is valid and empty
            if (!boardManager.isValidMove(currentPos, targetPos))
                continue;
            
            // Check if there's an enemy piece in between to capture
            Vector2Int middlePos = currentPos + (dir / 2);
            GameObject middlePiece = boardManager.grids[middlePos.x, middlePos.y].occupant;
            
            if (middlePiece != null)
            {
                CheckerPiece middleScript = middlePiece.GetComponent<CheckerPiece>();
                if (middleScript != null && !middleScript.isPlayerPiece)
                {
                    // Found a valid jump with an enemy piece to capture
                    return true;
                }
            }
        }
        return false;
    }

    private Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        Vector3Int cellPos = boardManager.tileMap.WorldToCell(worldPos);
        return new Vector2Int(cellPos.x, cellPos.y);
    }

    private Vector2Int FindPiecePosition(GameObject piece)
    {
        // Safety checks
        if (boardManager == null)
        {
            Debug.LogError("BoardManager is null in FindPiecePosition!");
            return Vector2Int.zero;
        }
        
        if (boardManager.grids == null)
        {
            Debug.LogError("BoardManager.grids is null!");
            return Vector2Int.zero;
        }

        // Search through the grid to find this piece
        for (int x = 0; x < boardManager.boardSize; x++)
        {
            for (int y = 0; y < boardManager.boardSize; y++)
            {
                if (boardManager.grids[x, y].occupant == piece)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        Debug.LogError($"Could not find position for piece {piece.name} in grid!");
        return Vector2Int.zero;
    }
}
