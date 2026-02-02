using UnityEngine;

public class CheckerPiece : MonoBehaviour
{
    public bool isPlayerPiece;
    public bool isKing = false;
    public Sprite kingSprite; // Assign in Inspector
    private Cell cellData;

    void Start()
    {
        // Get the cell data from the same GameObject or parent
        cellData = GetComponent<Cell>();
    }

    // 1. Kinging Logic
    public void CheckForKinging(int yCoordinate, int boardSize)
    {
        if (isKing) return;

        // Player reaches top (9) or AI reaches bottom (0)
        if ((isPlayerPiece && yCoordinate == boardSize - 1) || (!isPlayerPiece && yCoordinate == 0))
        {
            MakeKing();
        }
    }

    private void MakeKing()
    {
        isKing = true;
        GetComponent<SpriteRenderer>().sprite = kingSprite;
        // Logic for Level 2: Kings move any distance diagonally
    }

    // 2. Killing (Capture) Logic
    // This is called by the GameManager when a jump is validated
    public void Capture()
    {
        // Add points to score here
        Destroy(gameObject);
    }

    public Vector2Int GetGridPosition()
    {
        if (cellData != null)
            return cellData.gridPosition;
        return Vector2Int.zero;
    }
}
