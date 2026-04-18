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

    private float _initialValue;
    private float _currentValue;
    private float _currentMouseOffsetX;

    public UnityEvent OnValueChanged { get; } = new();

    public void StartInteraction(RaycastHit hitInfo)
    {
        _initialValue = _currentValue;
        _currentMouseOffsetX = 0f;
    }

    public void HoldInteraction(RaycastHit hitInfo)
    {
        UpdateValue();
    }

    public void StopInteraction() { }

    private void Awake()
    {
        SetValue(0f, notify: false);
    }

    private void UpdateValue()
    {
        _currentMouseOffsetX += Mouse.current.delta.ReadValue().x;
        float value = _initialValue + _currentMouseOffsetX / _maxOffsetInPixels;
        value = Mathf.Clamp01(value);
        SetValue(value);
    }

    public void SetValue(float value, bool notify = true)
    {
        if (_currentValue == value)
        {
            return;
        }

        _cursor.rotation = Quaternion.Euler(_cursor.rotation.x, _cursor.rotation.y, value * -360f);
        // _fill.fillAmount = value;
        // _value.text = value.ToString("N1");
        _currentValue = value;

        if (notify)
        {
            Tuner.ApplyTuning(_tuningType, value);
            OnValueChanged.Invoke();
        }
    }
}
