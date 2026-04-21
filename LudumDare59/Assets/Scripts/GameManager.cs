using System;
using UnityEngine;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : Singleton<GameManager>
{
    private ScreenSequence _startupSequence;
    private ScreenSequence _endSequence;

    public bool GameIsStarted { get; private set; }
    public bool GameIsPaused { get; private set; }
    public bool GameIsRunning => GameIsStarted && !GameIsPaused;
    public bool GameHasBeenStartedOnce { get; private set; }

    public static event Action OnGameReady;
    public static event Action<bool> OnGameStarted;
    public static event Action OnGameStopped;
    public static event Action<bool> OnGamePauseChanged;

    protected override void Awake()
    {
        base.Awake();

        PaintingManager.OnAllPaintingShown += PaintingManager_OnAllPaintingsShown;
    }

    private async void Start()
    {
        OnGameReady?.Invoke();

        //TODO: power button should call this
        await StartGameAsync();
    }

    private void OnDestroy()
    {
        PaintingManager.OnAllPaintingShown -= PaintingManager_OnAllPaintingsShown;
    }

    public static void RegisterStartupSequence(ScreenSequence sequence)
    {
        Instance._startupSequence = sequence;
    }

    public static void RegisterEndSequence(ScreenSequence sequence)
    {
        Instance._endSequence = sequence;
    }

    public async Task StartGameAsync()
    {
        GameIsStarted = true;
        SetPause(false);

        OnGameStarted?.Invoke(!GameHasBeenStartedOnce);

        GameHasBeenStartedOnce = true;

        await Task.Delay(1000);

        await LaunchStartupSequenceAsync();

        await PaintingManager.ShowNextPaintingAsync();
    }

    private async Task LaunchStartupSequenceAsync()
    {
        Tuner.Instance.SetEnabledPaintingMaterial(false);
        await _startupSequence.LaunchAsync();
        Tuner.Instance.SetEnabledPaintingMaterial(true);
    }

    private async Task LaunchEndSequenceAsync()
    {
        Tuner.Instance.SetEnabledPaintingMaterial(false);
        await _endSequence.LaunchAsync(closeLastOne: false);

        //TODO: shutdown screen
    }

    public void StopGame()
    {
        GameIsStarted = false;
        SetPause(false);

        OnGameStopped?.Invoke();
    }

    public void SetPause(bool isPaused)
    {
        if (isPaused && !GameIsStarted)
        {
            return;
        }

        GameIsPaused = isPaused;
        Time.timeScale = GameIsPaused ? 0f : 1f;
        OnGamePauseChanged?.Invoke(GameIsPaused);
    }

    public void TogglePause()
    {
        SetPause(!GameIsPaused);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    private async void PaintingManager_OnAllPaintingsShown()
    {
        await LaunchEndSequenceAsync();
    }
}