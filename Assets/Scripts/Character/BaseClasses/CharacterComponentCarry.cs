using System.Collections.Generic;
using UnityEngine;

#nullable enable

[DisallowMultipleComponent]
public abstract class CharacterComponentCarry : CharacterComponent
{
    ICarryable? _heldObject = null;

    protected abstract Transform CarryParent { get; }

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

    public void OnInteractWithCarryable(ICarryable target){
        if(target.IsCarryable == false) return;

        _heldObject = target;

        _heldObject.gameObject.transform.SetParent(CarryParent);
        // Handle rotation
        Quaternion relativeHandleRotation = Quaternion.Inverse(_heldObject.gameObject.transform.rotation) * _heldObject.CarryHandle.rotation;
        Quaternion desiredBriefcaseRotation = CarryParent.rotation * Quaternion.Inverse(relativeHandleRotation);
        _heldObject.gameObject.transform.rotation = desiredBriefcaseRotation;
        // Hanlde position
        _heldObject.gameObject.transform.position = CarryParent.position;
        Vector3 handleOffset = _heldObject.CarryHandle.position - _heldObject.gameObject.transform.position;
        _heldObject.gameObject.transform.position -= handleOffset; 
        
        target.OnPickUp(this);
    }
}
