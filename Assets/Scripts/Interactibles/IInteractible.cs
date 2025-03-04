using UnityEngine;

public interface IInteractible
{
    public GameObject gameObject { get; }
    public Vector3 InteractionWorldPosition { get; }
    public string GetInteractionName(CharacterBase interactor);
    public void DoInteraction(CharacterBase interactor);
}
