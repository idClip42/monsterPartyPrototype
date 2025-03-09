using UnityEngine;

public interface IInteractible
{
    public GameObject gameObject { get; }
    public Vector3 InteractionWorldPosition { get; }
    public string GetInteractionName(SimpleCharacter interactor);
    public void DoInteraction(SimpleCharacter interactor);
}
