using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rascar.Toolbox.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PaintingManager : Singleton<PaintingManager>
{
    [SerializeField] private List<PaintingInfo> _paintings;

    private int _currentPaintingIndex;
    private bool _paintingIsLoaded;

    public static PaintingInfo CurrentPainting => PaintingIsLoaded ? Instance._paintings[Instance._currentPaintingIndex] : default;
    public static bool PaintingIsLoaded => Instance._paintingIsLoaded;

    public static event Action OnPaintingChanged;

    public static async Task ShowNextPaintingAsync() => await Instance.ShowNextPaintingAsync_Internal();
    private async Task ShowNextPaintingAsync_Internal()
    {
        if (_currentPaintingIndex >= 0 && _currentPaintingIndex < _paintings.Count && SceneManager.GetSceneByName(_paintings[_currentPaintingIndex].SceneName).isLoaded)
        {
            await SceneManager.UnloadSceneAsync(_paintings[_currentPaintingIndex].SceneName);

            _paintingIsLoaded = false;
        }

        _currentPaintingIndex++;

        if (_currentPaintingIndex < _paintings.Count)
        {
            await SceneManager.LoadSceneAsync(_paintings[_currentPaintingIndex].SceneName, LoadSceneMode.Additive);

            _paintingIsLoaded = true;
        }

        //maybe in Gamemanager instead?
        OnPaintingChanged.Invoke();
    }

    protected override async void Awake()
    {
        base.Awake();

        await ResetAsync();
    }

    private async void Start()
    {
        //TODO: in a game manager
        await ShowNextPaintingAsync();
    }

    public async Task ResetAsync()
    {
        _currentPaintingIndex = -1;

        await UnloadAllScenesAsync();
    }

    private async Task UnloadAllScenesAsync()
    {
        foreach (PaintingInfo painting in _paintings)
        {
            if (SceneManager.GetSceneByName(painting.SceneName).isLoaded)
            {
                await SceneManager.UnloadSceneAsync(painting.SceneName);

                _paintingIsLoaded = false;
            }
        }
    }

    [Serializable]
    public struct PaintingInfo
    {
        public string SceneName;
        public Texture2D Original;
        public SerializableDictionary<TuningType, float> Combination;
    }
}
