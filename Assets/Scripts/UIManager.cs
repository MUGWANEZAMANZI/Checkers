using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Start Panel")]
    public GameObject startPanel;
    public Button playButton;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    [Header("Piece Counter")]
    public TextMeshProUGUI playerPiecesText;
    public TextMeshProUGUI aiPiecesText;

    private GameManager gameManager;
    private int playerPiecesCount = 12;
    private int aiPiecesCount = 12;

    [Header("Audio")]
    public AudioClip playerKillSfx;
    public AudioClip aiKillSfx;
    public AudioClip dangerSfx;
    public AudioClip[] backgroundMusic; // looping background music options

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private int musicIndex = 0;

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
        // Initialize audio sources
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        if (backgroundMusic != null && backgroundMusic.Length > 0)
        {
            musicIndex = 0;
            musicSource.clip = backgroundMusic[musicIndex];
            musicSource.Play();
            musicSource.volume = 0.5f;
        }
        
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
            playerPiecesText.text = $"Player: {playerPiecesCount}";
        }

        if (aiPiecesText != null)
        {
            aiPiecesText.text = $"AI: {aiPiecesCount}";
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

        // Play appropriate SFX (use danger SFX when either side has less than 5 pieces)
        AudioClip clipToPlay = null;
        int remainingPlayer = playerPiecesCount;
        int remainingAI = aiPiecesCount;

        if (remainingPlayer < 5 || remainingAI < 5)
        {
            clipToPlay = dangerSfx;
        }
        else
        {
            // If the captured piece belongs to the player, AI killed a piece -> play aiKillSfx
            // If the captured piece belongs to the AI, player killed a piece -> play playerKillSfx
            clipToPlay = isPlayerPiece ? aiKillSfx : playerKillSfx;
        }

        if (clipToPlay != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clipToPlay);
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
        playerPiecesCount = 20;
        aiPiecesCount = 20;
        UpdatePieceCounters(playerPiecesCount, aiPiecesCount);

        if (startPanel != null)
        {
            startPanel.gameObject.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(false);
        }

        // Restart background music if present
        if (musicSource != null)
        {
            musicSource.Stop();
            if (backgroundMusic != null && backgroundMusic.Length > 0)
            {
                musicIndex = 0;
                musicSource.clip = backgroundMusic[musicIndex];
                musicSource.Play();
            }
        }

        // Reload scene or reset game state
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
