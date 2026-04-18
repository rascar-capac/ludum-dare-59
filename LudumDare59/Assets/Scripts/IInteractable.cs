using UnityEngine;

public interface IInteractable
{
    void StartInteraction(RaycastHit hitInfo);
    void HoldInteraction(RaycastHit hitInfo);
    void StopInteraction();
}
