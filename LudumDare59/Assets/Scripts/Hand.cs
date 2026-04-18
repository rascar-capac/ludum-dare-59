using UnityEngine;
using UnityEngine.InputSystem;

public class Hand : MonoBehaviour
{
    [SerializeField] private InputActionReference _showOriginalInput;
    [SerializeField] private Transform _shownHandAnchor;

    private Vector3 _initialHandPosition;

    private void Awake()
    {
        _showOriginalInput.action.performed += _ => Show();
        _showOriginalInput.action.canceled += _ => Hide();
        _initialHandPosition = transform.position;
    }

    private void Show()
    {
        //TODO: tween
        transform.position = _shownHandAnchor.position;
    }

    private void Hide()
    {
        //TODO: call when painting changes
        transform.position = _initialHandPosition;
    }
}
