using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Button playButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button quitButton;

    [Header("Panels")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject creditsPanel;

    [Header("Options")]
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] Button optionsBackButton;

    [Header("Credits")]
    [SerializeField] Button creditsBackButton;

    [Header("Scene")]
    [SerializeField] string gameSceneName = "Game";
    [SerializeField] int gameSceneIndex = 1;
    [SerializeField] bool useSceneName;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        if (playButton != null) playButton.onClick.AddListener(OnPlay);
        if (optionsButton != null) optionsButton.onClick.AddListener(ShowOptions);
        if (creditsButton != null) creditsButton.onClick.AddListener(ShowCredits);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
        if (optionsBackButton != null) optionsBackButton.onClick.AddListener(ShowMain);
        if (creditsBackButton != null) creditsBackButton.onClick.AddListener(ShowMain);

        SetupVolumeSliders();
        ShowMain();

        AudioManager.Instance?.PlayMenuMusic();
    }

    void OnPlay()
    {
        if (SceneTransitionManager.Instance != null)
        {
            if (useSceneName)
                SceneTransitionManager.Instance.LoadScene(gameSceneName);
            else
                SceneTransitionManager.Instance.LoadScene(gameSceneIndex);
        }
        else
        {
            if (useSceneName)
                SceneManager.LoadScene(gameSceneName);
            else
                SceneManager.LoadScene(gameSceneIndex);
        }
    }

    void ShowMain()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    void ShowOptions()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    void ShowCredits()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    void SetupVolumeSliders()
    {
        if (AudioManager.Instance == null) return;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
            masterVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance.MasterVolume = v);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.MusicVolume;
            musicVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance.MusicVolume = v);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance.SFXVolume;
            sfxVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance.SFXVolume = v);
        }
    }

    void OnQuit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
