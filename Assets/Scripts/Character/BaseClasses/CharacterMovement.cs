using UnityEngine;

public abstract class CharacterMovement : MonoBehaviour
{
    public abstract Vector3 CurrentVelocity { get; }
}
