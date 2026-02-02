using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Start Panel")]
    public Panel startPanel;
    public Button playButton;

    [Header("Game Over Panel")]
    public Panel gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    [Header("Piece Counter")]
    public TextMeshProUGUI playerPiecesText;
    public TextMeshProUGUI aiPiecesText;

    private GameManager gameManager;
    private int playerPiecesCount = 12;
    private int aiPiecesCount = 12;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        // Setup button listeners
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        // Initialize UI
        UpdatePieceCounters(playerPiecesCount, aiPiecesCount);
        
        // Show start panel at game beginning
        if (startPanel != null)
        {
            startPanel.gameObject.SetActive(true);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called when Play button is clicked - hides start panel and starts game
    /// </summary>
    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked!");
        
        if (startPanel != null)
        {
            startPanel.gameObject.SetActive(false);
        }

        if (gameManager != null)
        {
            gameManager.ChangeState(GameState.PlayerTurn);
        }
    }

    /// <summary>
    /// Called when game ends - shows game over panel with win/lose message
    /// </summary>
    public void ShowGameOver(bool playerWins)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(true);
        }

        if (gameOverText != null)
        {
            if (playerWins)
            {
                gameOverText.text = "YOU WIN!";
                gameOverText.color = Color.green;
            }
            else
            {
                gameOverText.text = "YOU LOSE!";
                gameOverText.color = Color.red;
            }
        }
    }

    /// <summary>
    /// Updates the piece counter display for both player and AI
    /// </summary>
    public void UpdatePieceCounters(int playerCount, int aiCount)
    {
        playerPiecesCount = playerCount;
        aiPiecesCount = aiCount;

        if (playerPiecesText != null)
        {
            playerPiecesText.text = $"Player Pieces: {playerPiecesCount}";
        }

        if (aiPiecesText != null)
        {
            aiPiecesText.text = $"AI Pieces: {aiPiecesCount}";
        }

        Debug.Log($"Pieces - Player: {playerPiecesCount} | AI: {aiPiecesCount}");
    }

    /// <summary>
    /// Called when a piece is captured to update counters
    /// </summary>
    public void OnPieceCaptured(bool isPlayerPiece)
    {
        if (isPlayerPiece)
        {
            playerPiecesCount--;
        }
        else
        {
            aiPiecesCount--;
        }

        UpdatePieceCounters(playerPiecesCount, aiPiecesCount);

        // Check for game over conditions
        if (playerPiecesCount == 0 || aiPiecesCount == 0)
        {
            bool playerWins = aiPiecesCount == 0;
            ShowGameOver(playerWins);
        }
    }

    /// <summary>
    /// Resets the game and restarts
    /// </summary>
    public void OnRestartButtonClicked()
    {
        Debug.Log("Restart button clicked!");
        
        // Reset UI
        playerPiecesCount = 12;
        aiPiecesCount = 12;
        UpdatePieceCounters(playerPiecesCount, aiPiecesCount);

        if (startPanel != null)
        {
            startPanel.gameObject.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(false);
        }

        // Reload scene or reset game state
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
