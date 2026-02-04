using UnityEngine;
using System.Collections;

public enum GameState { PlayerTurn, AITurn, GameOver }

public class GameManager : MonoBehaviour
{
    public GameState currentState;
    public BoardManager boardManager;
    public int aiDifficulty = 2; // 1 = Easy (Random), 2 = Hard (Capture-focused)
    
    private AIController aiController;
    private PlayerController playerController;
    private UIManager uiManager;
    private int playerScore = 0;
    private int aiScore = 0;

    void Start()
    {
        aiController = GetComponent<AIController>();
        playerController = GetComponent<PlayerController>();
        boardManager = FindObjectOfType<BoardManager>();
        uiManager = FindObjectOfType<UIManager>();
        
        // Don't start the game here - wait for play button click
        currentState = GameState.PlayerTurn;
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"Game State Changed to: {newState}");
        
        if (newState == GameState.AITurn)
        {
            StartCoroutine(PerformAITurn());
        }
    }

    IEnumerator PerformAITurn()
    {
        yield return new WaitForSeconds(1.0f); // "Thinking" time for UX
        
        if (aiController != null)
        {
            aiController.MakeMove();
        }
        
        yield return new WaitForSeconds(0.5f);
        ChangeState(GameState.PlayerTurn);
    }

    public void OnPiecesCaptured(int count, bool isPlayer)
    {
        if (isPlayer)
        {
            aiScore += count;
        }
        else
        {
            playerScore += count;
        }
        
        Debug.Log($"Player Score: {playerScore} | AI Score: {aiScore}");
        
        // Update UI piece counters
        if (uiManager != null)
        {
            int playerPieces = 12 - aiScore; // Total player pieces minus captured
            int aiPieces = 12 - playerScore; // Total AI pieces minus captured
            uiManager.UpdatePieceCounters(playerPieces, aiPieces);
        }
    }

    public void EndGame(bool playerWins)
    {
        currentState = GameState.GameOver;
        string winner = playerWins ? "Player Wins!" : "AI Wins!";
        Debug.Log(winner);
        
        // Show game over UI
        if (uiManager != null)
        {
            uiManager.ShowGameOver(playerWins);
        }
    }


    public void Musics()
    {
        
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }

    public int GetAIDifficulty()
    {
        return aiDifficulty;
    }
}
