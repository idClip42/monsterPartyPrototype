using UnityEngine;

public interface IInteractible
{
    public GameObject gameObject { get; }
    public string InteractionName { get; }
    public Vector3 InteractionWorldPosition { get; }
    public void DoInteraction(CharacterBase interactor);
}
