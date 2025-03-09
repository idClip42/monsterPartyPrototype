using UnityEngine;

public abstract class CharacterMovementAI : MonoBehaviour, IInteractible
{
    public abstract Vector3 InteractionWorldPosition { get; }

    public abstract void DoInteraction(SimpleCharacter interactor);

    public abstract string GetInteractionName(SimpleCharacter interactor);
}
