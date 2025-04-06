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

        Vector3 handleOffset = target.CarryHandle.position - target.gameObject.transform.position;

        target.gameObject.transform.SetParent(CarryParent);
        target.gameObject.transform.position = CarryParent.position;
        target.gameObject.transform.position -= handleOffset; 

        // TODO: Rotation;
        
        target.OnPickUp(this);
    }
}
