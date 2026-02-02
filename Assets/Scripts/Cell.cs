using UnityEngine;

[System.Serializable]
public class Cell : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool isPlayable;
    public GameObject occupant;


    public Cell(int x, int y, bool playerble)
    {
        gridPosition = new Vector2Int(x, y);
        isPlayable = playerble;
        occupant = null;
    }

    public void SetOccupant(GameObject newOccupant)
    {
        occupant = newOccupant;
    }

}
