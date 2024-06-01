using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIManager : Singleton<GameplayUIManager>
{
    [Header("MAIN MENU")]
    [SerializeField] private Button button_Play;
    [SerializeField] private Button button_Settings;
    [SerializeField] private Button button_Leaderboard;
    [SerializeField] private Button button_Info;

    [SerializeField] private GameObject canvasGO_MainMenu;
    [SerializeField] private GameObject canvasGO_Gameplay;

    [Header("Settings")]
    [SerializeField] private GameObject canvasGO_Settings;
    [SerializeField] private Button button_CloseSettings;

    [SerializeField] private Slider slider_MusicVolume;
    [SerializeField] private TextMeshProUGUI text_MusicVolumeCount;

    [SerializeField] private Slider slider_SoundVolume;
    [SerializeField] private TextMeshProUGUI text_SoundVolumeCount;

    [SerializeField] private TMP_Dropdown dropdown_Language;

    [Header("Leaderboard")]
    [SerializeField] private GameObject canvasGO_Leaderboard;
    [SerializeField] private Button button_CloseLeaderboard;

    [Header("Info")]
    [SerializeField] private GameObject canvasGO_Info;
    [SerializeField] private Button button_CloseInfo;

    [Header("GAMEPLAY")]

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI text_Score;
    [SerializeField] private TextMeshProUGUI text_HighScore;

    [SerializeField] private Button button_GameplayPause;

    [Header("Gameplay Menu")]
    [SerializeField] private GameObject canvasGO_GameplayPause;

    [SerializeField] private Slider slider_MusicVolumeGameplayPause;
    [SerializeField] private TextMeshProUGUI text_MusicVolumeCountGameplayPause;

    [SerializeField] private Slider slider_SoundVolumeGameplayPause;
    [SerializeField] private TextMeshProUGUI text_SoundVolumeCountGameplayPause;

    [SerializeField] private TMP_Dropdown dropdown_LanguageGameplayPause;

    [SerializeField] private Button button_Resume;
    [SerializeField] private Button button_Restart;

    [Header("Game Over")]

    [SerializeField] private GameObject canvasGO_GameOver;
    [SerializeField] private Button button_GameOverRestart;
    [SerializeField] private TextMeshProUGUI text_GameOverScoreCount;
    [SerializeField] private GameObject textNewRecordGO;

    [Header("UI Audio Control")]
    public AudioClip audioClipUI;
    public AudioControl audioControl;

    public override void Awake()
    {
        base.Awake();

        canvasGO_MainMenu.SetActive(true);
        canvasGO_Gameplay.SetActive(false);
        canvasGO_Settings.SetActive(false);
        canvasGO_Leaderboard.SetActive(false);
        canvasGO_Info.SetActive(false);
        canvasGO_GameOver.SetActive(false);
        canvasGO_GameplayPause.SetActive(false);


        /*
         Main Menu
         */

        button_Play.onClick.AddListener(() =>
        {
            // Debug.STart Game
            audioControl.PlayOneShoot(audioClipUI);
            SetActiveMainMenu(false);
            canvasGO_Gameplay.SetActive(true);
            GameManager.START_GAME();
        });
        button_Settings.onClick.AddListener(() =>
        {
            UpdateInitMainMenuValues();
            audioControl.PlayOneShoot(audioClipUI);
            canvasGO_Settings.SetActive(true);
        });
        button_Leaderboard.onClick.AddListener(() =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            canvasGO_Leaderboard.SetActive(true);
        });
        button_Info.onClick.AddListener(() =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            canvasGO_Info.SetActive(true);
        });

        /*
         Settings
         */

        button_CloseSettings.onClick.AddListener(() =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            canvasGO_Settings.SetActive(false);
        });
        slider_MusicVolume.onValueChanged.AddListener((value) =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            GameManager.musicVolume = value;
            text_MusicVolumeCount.text = ((int)(value * 100)).ToString();
        });
        slider_SoundVolume.onValueChanged.AddListener((value) =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            GameManager.soundVolume = value;
            text_SoundVolumeCount.text = ((int)(value * 100)).ToString();
        });
        dropdown_Language.onValueChanged.AddListener((value) =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            SetLanguage(value);
        });


        /*
         Leader board
         */

        button_CloseLeaderboard.onClick.AddListener(() =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            canvasGO_Leaderboard.SetActive(false);
        });

        /*
         Info
         */

        button_CloseInfo.onClick.AddListener(() =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            canvasGO_Info.SetActive(false);
        });

        /*
         Gameplay Pause
         */

        button_GameplayPause.onClick.AddListener(() =>
        {
            UpdateInitPauseMenuValues();
            audioControl.PlayOneShoot(audioClipUI);
            GameManager.PAUSE_GAME();
            canvasGO_GameplayPause.SetActive(true);
        });


        slider_MusicVolumeGameplayPause.onValueChanged.AddListener((value) =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            GameManager.musicVolume = value;
            text_MusicVolumeCountGameplayPause.text = ((int)(value * 100)).ToString();
        });
        slider_SoundVolumeGameplayPause.onValueChanged.AddListener((value) =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            GameManager.soundVolume = value;
            text_SoundVolumeCountGameplayPause.text = ((int)(value * 100)).ToString();
        });

        dropdown_LanguageGameplayPause.onValueChanged.AddListener((value) =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            SetLanguage(value);
        });

        button_Resume.onClick.AddListener(() =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            GameManager.RESUME_GAME();
            canvasGO_GameplayPause.SetActive(false);
        });
        button_Restart.onClick.AddListener(() =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            GameManager.RESTART_GAME();
            canvasGO_GameplayPause.SetActive(false);
        });

        /*
         GAME OVER
         */


        button_GameOverRestart.onClick.AddListener(() =>
        {
            audioControl.PlayOneShoot(audioClipUI);
            GameManager.RESTART_GAME();
            canvasGO_GameOver.SetActive(false);
        });

        UpdateInitMainMenuValues();
        UpdateInitPauseMenuValues();
    }

    public void StartGame()
    {
        canvasGO_MainMenu.SetActive(false);
        canvasGO_Gameplay.SetActive(true);
        canvasGO_Settings.SetActive(false);
        canvasGO_Leaderboard.SetActive(false);
        canvasGO_Info.SetActive(false);
        canvasGO_GameOver.SetActive(false);

        UpdateScore(0);
        UpdateHighScore(GameManager.Instance.highScore);
    }

    public void GameOver(int endScore, bool isHighScore)
    {
        canvasGO_GameOver.SetActive(true);
        textNewRecordGO.SetActive(false);
        text_GameOverScoreCount.text = endScore.ToString();
        if (isHighScore) textNewRecordGO.SetActive(true);

    }

    public void UpdateScore(int score)
    {
        text_Score.text = score.ToString();
    }

    public void UpdateHighScore(int score)
    {
        text_HighScore.text = score.ToString();
    }

    public void SetActiveMainMenu(bool isActive)
    {

        canvasGO_MainMenu.SetActive(isActive);
    }

    public void SetLanguage(int value)
    {
        switch (value)
        {
            case 0:
                GameManager.SetLanguage(TextLang.Rus);
                break;
            case 1:
                GameManager.SetLanguage(TextLang.Eng);
                break;
            case 2:
                GameManager.SetLanguage(TextLang.Esp);
                break;
            case 3:
                GameManager.SetLanguage(TextLang.Fra);
                break;
            case 4:
                GameManager.SetLanguage(TextLang.Tur);
                break;
        }
    }

    public void UpdateInitMainMenuValues()
    {
        float musicvol = GameManager.musicVolume;
        float soundVol = GameManager.soundVolume;

        dropdown_Language.SetValueWithoutNotify(TextControlManager.GetTextLangInt());
        slider_MusicVolume.SetValueWithoutNotify(GameManager.musicVolume);
        slider_SoundVolume.SetValueWithoutNotify(GameManager.soundVolume);

        text_MusicVolumeCount.text = ((int)(musicvol * 100)).ToString();
        text_SoundVolumeCount.text = ((int)(soundVol * 100)).ToString();


    }

    public void UpdateInitPauseMenuValues()
    {
        float musicvol = GameManager.musicVolume;
        float soundVol = GameManager.soundVolume;

        dropdown_LanguageGameplayPause.SetValueWithoutNotify(TextControlManager.GetTextLangInt());
        slider_MusicVolumeGameplayPause.SetValueWithoutNotify(musicvol);
        slider_SoundVolumeGameplayPause.SetValueWithoutNotify(soundVol);

        text_MusicVolumeCountGameplayPause.text = ((int)(musicvol * 100)).ToString();
        text_SoundVolumeCountGameplayPause.text = ((int)(soundVol * 100)).ToString();
    }
}
