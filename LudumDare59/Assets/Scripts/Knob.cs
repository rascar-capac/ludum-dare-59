// using TMPro;
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

        if (_currentValue != value)
        {
            SetValue(value);
        }
    }

    public void SetValue(float value, bool notify = true)
    {
        _cursor.localRotation = Quaternion.Euler(_cursor.localRotation.x, _cursor.localRotation.y, value * -360f);
        // _fill.fillAmount = value;
        // _value.text = value.ToString("N1");
        _currentValue = value;

        if (notify)
        {
            Tuner.ApplyTuning(_tuningType, value);
            OnValueChanged.Invoke();
        }
    }

    private void OnValidate()
    {
        _currentValue = Mathf.Clamp01(_currentValue);

        if (Application.isPlaying && PaintingManager.PaintingIsLoaded)
        {
            SetValue(_currentValue);
        }
    }

    private void PaintingManager_OnPaintingChanged()
    {
        //TODO: tween and disable during it
        SetValue(0f, notify: false);
    }
}
