using UnityEngine;
using UnityEngine.Tilemaps;



public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int boardSize = 10;
    public Tilemap tileMap;
    public TileBase darkTile;
    public TileBase lightTile;
    public Cell[,] grids; // Made public for controllers

    [Header("Player Sprites")]
    public Sprite playerSprite;
    public Sprite aiSprite;
    public Sprite playerKingSprite;
    public Sprite aiKingSprite;

    public GameObject playerPrefab;
    public GameObject aiPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeBoard();
        SpawnPieces();
    }

    void LoadPlayerSprites()
    {
        // Apply sprites to the prefabs or pieces
        if (playerPrefab != null && playerSprite != null)
        {
            SpriteRenderer playerRenderer = playerPrefab.GetComponent<SpriteRenderer>();
            if (playerRenderer != null)
            {
                playerRenderer.sprite = playerSprite;
            }
        }

        if (aiPrefab != null && aiSprite != null)
        {
            SpriteRenderer aiRenderer = aiPrefab.GetComponent<SpriteRenderer>();
            if (aiRenderer != null)
            {
                aiRenderer.sprite = aiSprite;
            }
        }
    }

    void InitializeBoard()
    {
        grids = new Cell[boardSize, boardSize];
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                bool isPlayable = (i + j) % 2 != 0;
                grids[i, j] = new Cell(i, j, isPlayable);
                Vector3Int tilePosition = new Vector3Int(i, j, 0);
                tileMap.SetTile(tilePosition, isPlayable ? lightTile : darkTile);
                
            }
    }
    }

    public bool isValidMove(Vector2Int from, Vector2Int to)
    {
        if (to.x < 0 || to.x >= boardSize || to.y < 0 || to.y >= boardSize) return false;
        
        if(!grids[to.x, to.y].isPlayable) return false;

        if (grids[to.x, to.y].occupant != null) return false;

        return true;
    }


    public void SpawnPieces()
    {
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
                {
                    // Only spawn on playable squares
                    if ((x + y) % 2 != 0) 
                        {
                        // Player Side (Bottom 4 rows)
                            if (y < 4) 
                            {
                                CreatePiece(x, y, playerPrefab, true);
                            }
                            // AI Side (Top 4 rows)
                            else if (y > 5) 
                            {
                            CreatePiece(x, y, aiPrefab, false);
                             }
                        }   
            }
        }
    }

    private void CreatePiece(int x, int y, GameObject prefab, bool isPlayer)
    {
        // Use tilemap's cell center for perfect alignment
        Vector3Int cellPosition = new Vector3Int(x, y, 0);
        Vector3 worldPosition = tileMap.GetCellCenterWorld(cellPosition);
        worldPosition.z = -1f; // Position above tilemap
        
        GameObject pieceObj = Instantiate(prefab, worldPosition, Quaternion.identity, transform);
        pieceObj.name = isPlayer ? $"Player_{x}_{y}" : $"AI_{x}_{y}";
        
        // Tag the piece for raycasting
        pieceObj.tag = "Player";
        
        // Add SpriteRenderer if it doesn't exist and assign sprite
        SpriteRenderer renderer = pieceObj.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = pieceObj.AddComponent<SpriteRenderer>();
        }
        
        // Assign the appropriate sprite
        renderer.sprite = isPlayer ? playerSprite : aiSprite;
        
        // Set sorting order to render above tilemap
        renderer.sortingOrder = 1;
        
        // Add BoxCollider2D for raycasting - MUST be after sprite assignment
        BoxCollider2D collider = pieceObj.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = pieceObj.AddComponent<BoxCollider2D>();
        }
        // Important: Set as NON-trigger for raycast detection
        collider.isTrigger = false;
        // Ensure the collider size matches the sprite bounds
        if (renderer.sprite != null)
        {
            Vector2 spriteSize = renderer.sprite.bounds.size;
            collider.size = spriteSize;
            Debug.Log($"Piece {pieceObj.name} collider size: {collider.size}");
        }
        
        // Scale the piece to fit nicely in the cell (adjust based on PPU 220)
        pieceObj.transform.localScale = Vector3.one;
        
        // Assign to the Cell data we created earlier
        grids[x, y].occupant = pieceObj;
        
        // Set piece properties
        CheckerPiece pieceScript = pieceObj.GetComponent<CheckerPiece>();
        if (pieceScript == null)
        {
            pieceScript = pieceObj.AddComponent<CheckerPiece>();
        }
        pieceScript.isPlayerPiece = isPlayer;
        pieceScript.kingSprite = isPlayer ? playerKingSprite : aiKingSprite;
    }
}
