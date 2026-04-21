using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FMODUnity;
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
    public static bool HasStillPaintingsToShow => Instance._currentPaintingIndex < Instance._paintings.Count - 1;

    public static event Action OnPaintingChanged;
    public static event Action OnAllPaintingShown;

    public static async Task ShowNextPaintingAsync() => await Instance.ShowNextPaintingAsync_Internal();
    private async Task ShowNextPaintingAsync_Internal()
    {
        if (_currentPaintingIndex >= 0 && _currentPaintingIndex < _paintings.Count && SceneManager.GetSceneByName(_paintings[_currentPaintingIndex].SceneName).isLoaded)
        {
            await SceneManager.UnloadSceneAsync(_paintings[_currentPaintingIndex].SceneName);

            _paintingIsLoaded = false;
        }

        _currentPaintingIndex++;

        while (_currentPaintingIndex < _paintings.Count && !_paintings[_currentPaintingIndex].IsEnabled)
        {
            _currentPaintingIndex++;
        }

        if (_currentPaintingIndex < _paintings.Count)
        {
            await SceneManager.LoadSceneAsync(_paintings[_currentPaintingIndex].SceneName, LoadSceneMode.Additive);

            _paintingIsLoaded = true;
        }
        else
        {
            OnAllPaintingShown?.Invoke();
        }

        //maybe in Gamemanager instead?
        OnPaintingChanged.Invoke();
    }

    protected override async void Awake()
    {
        base.Awake();

        await ResetAsync();
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
        public bool IsEnabled;
        public string SceneName;
        public Texture2D Original;
        public SerializableDictionary<TuningType, ChannelInfo> Channels;
        public EventReference TuningAudio;
    }

    [Serializable]
    public struct ChannelInfo
    {
        public float TunedValue;
        [ParamRef]
        public string FmodParameterName;
    }
}
