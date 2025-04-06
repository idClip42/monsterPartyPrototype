using System.Collections.Generic;
using UnityEngine;

#nullable enable

[DisallowMultipleComponent]
public abstract class CharacterComponentCarry : CharacterComponent
{
    ICarryable? _heldObject = null;

    protected abstract Transform CarryParent { get; }

    public ICarryable? HeldObject => _heldObject;

    public override string DebugHeader => "Carry";

    public override void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        infoTarget["Held"] = _heldObject != null ? 
            _heldObject.gameObject.name : 
            "None";
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if(this.Character == null) throw new System.Exception("Null _characterBase");

        if(this.Character.State == Character.StateType.Player){
            if(Input.GetButtonDown("Drop")){
                DropHeldObject();
            }
        }

    }

    public void OnInteractWithCarryable(ICarryable target)
    {
        if (!target.IsCarryable) return;
        if (_heldObject != null) return;

        _heldObject = target;

        // 1. Temporarily unparent to avoid local rotation/position interference
        Transform briefcaseTransform = _heldObject.gameObject.transform;
        briefcaseTransform.SetParent(null); 

        // 2. Calculate relative rotation of the handle to the briefcase
        Quaternion relativeHandleRotation = Quaternion.Inverse(briefcaseTransform.rotation) * _heldObject.CarryHandle.rotation;

        // 3. Determine how to rotate the briefcase so its handle matches the hand's rotation
        Quaternion desiredBriefcaseRotation = CarryParent.rotation * Quaternion.Inverse(relativeHandleRotation);
        briefcaseTransform.rotation = desiredBriefcaseRotation;

        // 4. Position the briefcase so the handle aligns with the CarryParent
        Vector3 handleOffset = _heldObject.CarryHandle.position - briefcaseTransform.position;
        briefcaseTransform.position = CarryParent.position - handleOffset;

        // 5. Re-parent it now that transform is in place
        briefcaseTransform.SetParent(CarryParent, worldPositionStays: true);

        // 6. Trigger any logic tied to pickup
        target.OnPickUp(this);
    }

    public void ForceDrop(){
        DropHeldObject();
    }
    
    private void DropHeldObject()
    {
        if (_heldObject == null) return;

        _heldObject.gameObject.transform.SetParent(null); // unparent from the hand
        _heldObject.OnDrop(this); // let the object handle physics toggling, etc.
        _heldObject = null;
    }
}
