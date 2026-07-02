using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { MainMenu, Playing, Paused, GameOver, Victory }

    [Header("Game Settings")]
    [SerializeField] int wavesToWin = 5;
    [SerializeField] float restartDelay = 3f;

    [Header("References")]
    [SerializeField] InputReader input;
    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] GameObject gameOverUI;
    [SerializeField] GameObject victoryUI;

    [Header("Events")]
    public UnityEvent<GameState> OnStateChanged;
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent OnGameOver;
    public UnityEvent OnVictory;
    public UnityEvent OnPause;
    public UnityEvent OnResume;

    GameState currentState = GameState.Playing;
    int score;

    public static GameManager Instance { get; private set; }
    public GameState CurrentState => currentState;
    public int Score => score;
    public int WavesToWin => wavesToWin;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        SetState(GameState.Playing);
    }

    void Update()
    {
        if (input != null && input.PausePressed)
        {
            if (currentState == GameState.Playing)
                Pause();
            else if (currentState == GameState.Paused)
                Resume();
        }
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(newState);

        if (pauseMenuUI != null) pauseMenuUI.SetActive(newState == GameState.Paused);
        if (gameOverUI != null) gameOverUI.SetActive(newState == GameState.GameOver);
        if (victoryUI != null) victoryUI.SetActive(newState == GameState.Victory);

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Confined;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                OnPause?.Invoke();
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                OnGameOver?.Invoke();
                break;
            case GameState.Victory:
                Time.timeScale = 0f;
                OnVictory?.Invoke();
                break;
        }
    }

    public void Pause()
    {
        if (currentState != GameState.Playing) return;
        SetState(GameState.Paused);
    }

    public void Resume()
    {
        if (currentState != GameState.Paused) return;
        SetState(GameState.Playing);
    }

    public void PlayerDied()
    {
        SetState(GameState.GameOver);
    }

    public void WaveCompleted(int waveNumber)
    {
        if (waveNumber >= wavesToWin)
            SetState(GameState.Victory);
    }

    public void AddScore(int points)
    {
        score += points;
        OnScoreChanged?.Invoke(score);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }
}
