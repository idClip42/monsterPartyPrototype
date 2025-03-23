using UnityEditor;
using UnityEngine;

public abstract class CharacterComponentMovement : CharacterComponent
{
    public abstract Vector3 CurrentVelocity { get; }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        using(new Handles.DrawingScope(Color.white)){
            if(CurrentVelocity != Vector3.zero){
                Handles.DrawLine(
                    transform.position,
                    transform.position + CurrentVelocity
                );
            }
        }
    }
#endif
}
