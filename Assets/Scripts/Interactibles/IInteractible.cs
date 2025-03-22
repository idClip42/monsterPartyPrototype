using UnityEngine;

#nullable enable

public interface IInteractible
{
    GameObject gameObject { get; }
    bool IsInteractible { get; }
    Vector3 InteractionWorldPosition { get; }
    string GetInteractionName(Character interactor);
    void DoInteraction(Character interactor);
}
