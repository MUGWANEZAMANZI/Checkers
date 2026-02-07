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
        sfxSource.volume = 1.0f; // make SFX loud by default

        // Debug: ensure AudioListener exists (no audio will be heard without one)
        if (FindObjectOfType<AudioListener>() == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.gameObject.AddComponent<AudioListener>();
                Debug.Log("UIManager: No AudioListener found - added one to Main Camera.");
            }
            else
            {
                Debug.LogWarning("UIManager: No AudioListener found and no Main Camera to attach to.");
            }
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        if (backgroundMusic != null && backgroundMusic.Length > 0)
        {
            musicIndex = 0;
            musicSource.clip = backgroundMusic[musicIndex];
            musicSource.volume = 0.08f; // keep background music very low
            musicSource.Play();
            Debug.Log($"UIManager: Started background music (index {musicIndex}) at volume {musicSource.volume}");
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
            playerPiecesText.text = $"You: {playerPiecesCount}";
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
            // Boost volume for kill SFX to make captures feel impactful
            float scale = (clipToPlay == playerKillSfx || clipToPlay == aiKillSfx) ? 1.8f : 1.0f;
            Debug.Log($"UIManager: Playing SFX {clipToPlay.name} scale={scale} sourceVol={sfxSource.volume}");
            sfxSource.PlayOneShot(clipToPlay, scale);
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

        // Restart background music if present
        if (musicSource != null)
        {
            musicSource.Stop();
            if (backgroundMusic != null && backgroundMusic.Length > 0)
            {
                musicIndex = 0;
                musicSource.clip = backgroundMusic[musicIndex];
                musicSource.volume = 0.08f; // re-apply low music volume on restart
                musicSource.Play();
            }
        }

        // Reload scene or reset game state
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
