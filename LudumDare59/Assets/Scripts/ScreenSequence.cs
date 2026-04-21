using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ScreenSequence : MonoBehaviour
{
    [SerializeField] private List<GameObject> _screenList;
    [SerializeField] private float _screenDisplayTime;
    [SerializeField] private float _timeBetweenScreens;
    [SerializeField] private bool _isStartupSequence;

    private void Awake()
    {
        foreach (GameObject screen in _screenList)
        {
            screen.SetActive(false);
        }

        GameManager.OnGameReady += GameManager_OnGameReady;
    }

    private void OnDestroy()
    {
        GameManager.OnGameReady -= GameManager_OnGameReady;
    }

    private void GameManager_OnGameReady()
    {
        if (_isStartupSequence)
        {
            GameManager.RegisterStartupSequence(this);
        }
        else
        {
            GameManager.RegisterEndSequence(this);
        }
    }

    public async Task LaunchAsync(bool closeLastOne = true)
    {
        await DisplayScreensAsync(closeLastOne);
    }

    private async Task DisplayScreensAsync(bool closeLastOne)
    {
        foreach (GameObject screen in _screenList)
        {
            screen.SetActive(true);

            await Task.Delay((int)(_screenDisplayTime * 1000));

            if (screen != _screenList[^1] || closeLastOne)
            {
                screen.SetActive(false);
            }

            await Task.Delay((int)(_timeBetweenScreens * 1000));
        }
    }
}
