using UnityEditor;
using UnityEngine;

public abstract class CharacterMovement : MonoBehaviour
{
    public abstract Vector3 CurrentVelocity { get; }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if(CurrentVelocity != Vector3.zero){
            Handles.DrawLine(
                transform.position,
                transform.position + CurrentVelocity
            );
        }
    }
    #endif
}
