using System;
using UnityEngine;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private ScreenSequence _startupSequence;
    [SerializeField] private ScreenSequence _endSequence;

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
        await _startupSequence.LaunchAsync();
    }

    private async Task LaunchEndSequenceAsync()
    {
        await _endSequence.LaunchAsync();

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