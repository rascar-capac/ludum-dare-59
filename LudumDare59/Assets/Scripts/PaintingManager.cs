using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PaintingManager : Singleton<PaintingManager>
{
    [SerializeField] private List<string> _paintingScenes;

    private int _currentPaintingIndex;

    [ContextMenu("Show Next Painting")]
    public async Task ShowNextPaintingAsync()
    {
        if (_currentPaintingIndex >= 0 && _currentPaintingIndex < _paintingScenes.Count && SceneManager.GetSceneByName(_paintingScenes[_currentPaintingIndex]).isLoaded)
        {
            await SceneManager.UnloadSceneAsync(_paintingScenes[_currentPaintingIndex]);
        }

        _currentPaintingIndex++;

        await SceneManager.LoadSceneAsync(_paintingScenes[_currentPaintingIndex], LoadSceneMode.Additive);
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
        foreach (string paintingScene in _paintingScenes)
        {
            if (SceneManager.GetSceneByName(paintingScene).isLoaded)
            {
                await SceneManager.UnloadSceneAsync(paintingScene);
            }
        }
    }
}
