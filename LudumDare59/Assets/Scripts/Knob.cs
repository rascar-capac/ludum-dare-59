// using TMPro;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Knob : MonoBehaviour, IInteractable
{
    [SerializeField] private TuningType _tuningType;
    [SerializeField] private Transform _cursor;
    // [SerializeField] private TMP_Text _value;
    [SerializeField] private int _maxOffsetInPixels;
    [SerializeField] private float _currentValue;
    [SerializeField] private TweenSettings _resetTweenSettings;

    private float _initialValue;
    private float _currentMouseOffsetX;
    private bool _isEnabled;

    public bool IsUsed { get; private set; }
    public float Value => _currentValue;

    public UnityEvent OnValueChanged { get; } = new();

    public void StartInteraction(RaycastHit hitInfo)
    {
        _initialValue = _currentValue;
        _currentMouseOffsetX = 0f;
        IsUsed = true;
    }

    public void HoldInteraction(RaycastHit hitInfo)
    {
        UpdateValue();
    }

    public void StopInteraction()
    {
        IsUsed = false;
    }

    private void Awake()
    {
        SetValue(0f, notify: false);
        PaintingManager.OnPaintingChanged += PaintingManager_OnPaintingChanged;
    }

    private void OnDestroy()
    {
        PaintingManager.OnPaintingChanged -= PaintingManager_OnPaintingChanged;
    }

    private void UpdateValue()
    {
        _currentMouseOffsetX += Mouse.current.delta.ReadValue().x;
        float value = _initialValue + _currentMouseOffsetX / _maxOffsetInPixels;
        value = Mathf.Clamp(value, -1, 1);

        if (_currentValue != value)
        {
            SetValue(value);
        }
    }

    public void SetValue(float value, bool notify = true)
    {
        _cursor.localRotation = Quaternion.Euler(_cursor.localRotation.x, _cursor.localRotation.y, value * 180f);
        // _fill.fillAmount = value;
        // _value.text = value.ToString("N1");
        _currentValue = value;
        Tuner.ApplyTuning(_tuningType, value);

        if (notify)
        {
            OnValueChanged.Invoke();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _currentValue = Mathf.Clamp(_currentValue, -1, 1);

        if (Application.isPlaying && PaintingManager.PaintingIsLoaded)
        {
            SetValue(_currentValue);
        }
    }
#endif

    private async void PaintingManager_OnPaintingChanged()
    {
        Tuner.ApplyTuning(_tuningType, 0f);
        _currentValue = 0f;
        _isEnabled = false;

        await Tween.LocalRotation(_cursor, new TweenSettings<Quaternion>(Quaternion.Euler(_cursor.localRotation.x, _cursor.localRotation.y, 0f), _resetTweenSettings));

        _isEnabled = true;
    }
}
