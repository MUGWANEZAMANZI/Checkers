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
        
        // Notify UI manager so it can play SFX and update its internal counters
        if (uiManager != null)
        {
            // uiManager.OnPieceCaptured expects `isPlayerPiece` (true if the captured piece belonged to the player)
            bool capturedIsPlayerPiece = !isPlayer; // if capture was by player, captured piece is AI
            for (int i = 0; i < count; i++)
            {
                uiManager.OnPieceCaptured(capturedIsPlayerPiece);
            }
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
