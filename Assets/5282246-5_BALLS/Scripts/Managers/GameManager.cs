using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;

public enum GameState { 
    PreGame,
    LoadDataFromYG,
    MainMenu,
    Gameplay,
    Pause,
    GameOver,
}

public class GameManager : Soliton<GameManager>
{
    [Header("GameState")]

    [SerializeField] private GameState _gameState;
    public static GameState gameState
    {
        get { return Instance._gameState; }
        set { Instance._gameState = value; }
    }

    [SerializeField] private bool testGameplay;
    public static bool TEST_GAMEPLAY;

    public bool IS_MOBILE;
    public bool SIMULATE_MOBILE;

    [Header("Game Settings")]
    [SerializeField] private static float _soundVolume = 0.5f;
    public static float soundVolume {
        get { return _soundVolume; }
        set { 
            _soundVolume = Mathf.Clamp01(value);
            OnSoundVolumeChanged(_soundVolume);
        }
    }
    public static event Action<float> OnSoundVolumeChanged;

    [SerializeField] private static float _musicVolume = 0.5f;
    public static float musicVolume
    {
        get { return _musicVolume; }
        set { 
            _musicVolume = Mathf.Clamp01(value);
            OnMusicVolumeChanged(_musicVolume);
        }
    }
    public static event Action<float> OnMusicVolumeChanged;

    [Header("Screen Size")]
    public Vector2Int screenSize;
    public static event Action<Vector2Int> OnScreenSizeChanged;

    [Header("Score Management")]
    public int highScore;


    [SerializeField] private string scoreLeaderBoardName = "scoreList";


    public string lang;

    public override void Awake()
    {
        base.Awake();

        gameState = GameState.PreGame;
        IS_MOBILE = false
            || SIMULATE_MOBILE;

        TEST_GAMEPLAY = testGameplay;

        screenSize = new Vector2Int(Screen.width, Screen.height);

        OnSoundVolumeChanged += Foo;
        OnMusicVolumeChanged += Foo;

        gameState = GameState.LoadDataFromYG;

        YandexGame.GetDataEvent += GetLoadDataYG;

        lang = YandexGame.EnvironmentData.language;
        SetLanguage(lang);

//        YandexGame.GetLeaderboard

        if (TEST_GAMEPLAY)
        {
            START_GAME();

        }
        else { 
            gameState = GameState.MainMenu;
            GameplayUIManager.Instance.SetActiveMainMenu(true);
        }

    }

    public void Update()
    {
        UpdateScreenSize();
    }

    public static void START_GAME()
    {
        gameState = GameState.Gameplay;
        GameplayUIManager.Instance.StartGame();
        GameplayManager.Instance.StartGame();
    }

    public static void RESTART_GAME()
    {
        YandexGame.FullscreenShow();
        GameplayManager.Instance.RestartGame();
        START_GAME();
    }

    public static void RESUME_GAME() {
        gameState = GameState.Gameplay;
    }

    public static void PAUSE_GAME()
    {
        gameState = GameState.Pause;
    }

    public static void GAME_OVER(int score)
    {
        gameState = GameState.GameOver;
        bool isNewHighScore = false;

        isNewHighScore = Instance.CheckHighScore(score);
        GameplayUIManager.Instance.GameOver(score, isNewHighScore);
//        YandexGame.FullscreenShow();
        Debug.Log("GAME_OVER");
    }


    #region SCORE

    public bool CheckHighScore(int score) { 
        if(score > highScore) { 
            highScore = score;
            SaveDataYG();
            YandexGame.NewLeaderboardScores(Instance.scoreLeaderBoardName, score);
            return true;
        }
        return false;
    }

    #endregion SCORE

    #region TECH_MANAGEMENT

    void UpdateScreenSize() {
        Vector2Int newScreenSize = new Vector2Int(Screen.width, Screen.height);
        if (screenSize != newScreenSize) {
            OnScreenSizeChanged(newScreenSize);
            screenSize = newScreenSize;
        }
    }

    #endregion TECH_MANAGEMENT

    #region SAVES
    public void SaveDataYG() {
        YandexGame.savesData.highScore = highScore;

        YandexGame.SaveProgress();
    }

    public void GetLoadDataYG() {
        highScore = 0;

        highScore = YandexGame.savesData.highScore;
    }

    #endregion SAVES


    public void OnDestroy()
    {
        YandexGame.GetDataEvent -= GetLoadDataYG;
    }



    public static void SetLanguage(TextLang textLang) {
        TextControlManager.textLang = textLang;
    }

    public static void SetLanguage(string lang) {
        switch (lang) {
            case "ru":
                SetLanguage(TextLang.Rus);
                break;
            case "en":
                SetLanguage(TextLang.Eng);
                break;
            case "es":
                SetLanguage(TextLang.Esp);
                break;
            case "fr":
                SetLanguage(TextLang.Fra);
                break;
            case "tr":
                SetLanguage(TextLang.Tur);
                break;
            default:
                SetLanguage(TextLang.Rus);
                break;

        }
    }

    public void Foo(float value) { 
    }
}
