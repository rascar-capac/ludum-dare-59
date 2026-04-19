using System;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private Transform _hiddenHandAnchor;
    [SerializeField] private float _delayBeforeSwitching = 1f;

    [SerializeField] private TweenSettings _handSwitchTweenSettings;

    private Vector3 _initialHandPosition;
    private Quaternion _initialHandRotation;

    public event Action OnHidden;

    private void Awake()
    {
        PaintingManager.OnPaintingChanged += PaintingManager_OnPaintingChanged;
        _initialHandPosition = transform.position;
        _initialHandRotation = transform.rotation;

        transform.SetPositionAndRotation(_hiddenHandAnchor.position, _hiddenHandAnchor.rotation);
    }

    private void OnDestroy()
    {
        PaintingManager.OnPaintingChanged -= PaintingManager_OnPaintingChanged;
    }

    private async void PaintingManager_OnPaintingChanged()
    {
        await Task.Delay((int)(_delayBeforeSwitching * 1000));

        await HideAsync();

        if (PaintingManager.PaintingIsLoaded)
        {
            await ShowAsync();
        }
    }

    private async Task ShowAsync()
    {
        await Sequence.Create()
            .Chain(Tween.Position(transform, new(_initialHandPosition, _handSwitchTweenSettings)))
            .Group(Tween.Rotation(transform, new TweenSettings<Quaternion>(_initialHandRotation, _handSwitchTweenSettings)));
    }

    private async Task HideAsync()
    {
        await Sequence.Create()
            .Chain(Tween.Position(transform, new(_hiddenHandAnchor.position, _handSwitchTweenSettings)))
            .Group(Tween.Rotation(transform, new TweenSettings<Quaternion>(_hiddenHandAnchor.rotation, _handSwitchTweenSettings)));

        OnHidden?.Invoke();
    }
}
