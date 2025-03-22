using UnityEngine;

public abstract class CharacterMovement : MonoBehaviour
{
    public abstract Vector3 CurrentVelocity { get; }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if(CurrentVelocity != Vector3.zero){
            Debug.DrawLine(
                transform.position,
                transform.position + CurrentVelocity,
                Color.white
            );
        }
    }
    #endif
}
