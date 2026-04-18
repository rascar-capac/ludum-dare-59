using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private InputActionReference _interactionInput;
    [SerializeField] private LayerMask _interactionMask;

    private IInteractable _currentInteractable;
    private RaycastHit[] _hitListCache = new RaycastHit[10];

    private void Update()
    {
        CheckInteractions();
    }

    public void CheckInteractions()
    {
        IInteractable previousInteractable = _currentInteractable;
        _currentInteractable = null;
        RaycastHit hitInfo = default;

        if (_interactionInput.action.IsPressed())
        {
            Ray mouseRay = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (previousInteractable != null)
            {
                _currentInteractable = previousInteractable;
            }
            else
            {
                Physics.RaycastNonAlloc(mouseRay, _hitListCache, maxDistance: 10f, layerMask: _interactionMask);

                foreach (RaycastHit hit in _hitListCache)
                {
                    if (hit.collider != null && hit.collider.TryGetComponent(out _currentInteractable))
                    {
                        hitInfo = hit;

                        break;
                    }
                }
            }

            if (_currentInteractable != null)
            {
                if (_currentInteractable != previousInteractable)
                {
                    _currentInteractable.StartInteraction(hitInfo);
                }

                if (_currentInteractable != null)
                {
                    _currentInteractable.HoldInteraction(hitInfo);
                }
            }
        }

        if (previousInteractable != null && _currentInteractable != previousInteractable)
        {
            previousInteractable.StopInteraction();
        }
    }
}
