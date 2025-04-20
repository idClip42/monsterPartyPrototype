using UnityEditor;
using UnityEngine;

#nullable enable

public abstract class CharacterComponentMovement : CharacterComponent
{
    [SerializeField]
    [Range(0, 0.2f)]
    private float speedHandicapPerMassUnit = 0.05f;

    public abstract Vector3 CurrentVelocity { get; }

    protected float GetMaxMoveSpeed(){
        if(this.Character == null)
            throw new System.Exception($"Null Character on {this.gameObject.name}");
        if(this.Character.Carry == null)
            throw new System.Exception($"Null Carry on character {this.gameObject.name}");

        float baseSpeed = this.Character.Movement.RunSpeed;

        if(this.Character.Carry.HeldObject == null)
            return baseSpeed;
            
        float heldMass = this.Character.Carry.HeldObject.Mass;
        float speedMultiplier = Mathf.Clamp01(1f - (heldMass * speedHandicapPerMassUnit));
        return baseSpeed * speedMultiplier;
    }

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
