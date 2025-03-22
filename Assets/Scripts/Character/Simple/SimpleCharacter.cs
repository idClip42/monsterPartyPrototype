using UnityEngine;

#nullable enable

[RequireComponent(typeof(SimpleCharacterMovementPlayer))]
[RequireComponent(typeof(SimpleCharacterMovementAI))]
[RequireComponent(typeof(SimpleCharacterCrouch))]
[RequireComponent(typeof(SimpleCharacterInteract))]
public class SimpleCharacter : Character
{
    protected override void HandleDeath(Entity deadEntity)
    {
        base.HandleDeath(deadEntity);
        transform.Rotate(90, 0, 0);
    }
}
