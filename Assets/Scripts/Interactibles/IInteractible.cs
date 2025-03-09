using UnityEngine;

#nullable enable

public interface IInteractible
{
    public GameObject gameObject { get; }
    public Vector3 InteractionWorldPosition { get; }
    public string GetInteractionName(Character interactor);
    public void DoInteraction(Character interactor);
}
