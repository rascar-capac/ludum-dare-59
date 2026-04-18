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

    public static PaintingInfo CurrentPainting => Instance._paintings[Instance._currentPaintingIndex];

    public static event Action OnPaintingChanged;

    [ContextMenu("Show Next Painting")]
    public async Task ShowNextPaintingAsync()
    {
        if (_currentPaintingIndex >= 0 && _currentPaintingIndex < _paintings.Count && SceneManager.GetSceneByName(_paintings[_currentPaintingIndex].SceneName).isLoaded)
        {
            await SceneManager.UnloadSceneAsync(_paintings[_currentPaintingIndex].SceneName);
        }

        _currentPaintingIndex++;

        await SceneManager.LoadSceneAsync(_paintings[_currentPaintingIndex].SceneName, LoadSceneMode.Additive);

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
