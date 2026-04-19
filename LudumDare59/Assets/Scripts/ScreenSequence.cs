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

    public async Task LaunchAsync()
    {
        await DisplayScreensAsync();
    }

    private async Task DisplayScreensAsync()
    {
        foreach (GameObject screen in _screenList)
        {
            screen.SetActive(true);

            await Task.Delay((int)(_screenDisplayTime * 1000));

            screen.SetActive(false);

            await Task.Delay((int)(_timeBetweenScreens * 1000));
        }
    }
}
