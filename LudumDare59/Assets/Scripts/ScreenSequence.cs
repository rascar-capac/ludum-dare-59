using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ScreenSequence : MonoBehaviour
{
    [SerializeField] private List<GameObject> _screenList;
    [SerializeField] private float _screenDisplayTime;
    [SerializeField] private float _timeBetweenScreens;

    private void Awake()
    {
        foreach (GameObject screen in _screenList)
        {
            screen.SetActive(false);
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
