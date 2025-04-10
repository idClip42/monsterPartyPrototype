using UnityEngine;

#nullable enable

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class CarryableObject : MonoBehaviour, IInteractible, ICarryable
{
    private Rigidbody? _rb = null;
    private bool _isHeld = false;
    private bool _defaultKinematicState = false;
    private CharacterComponentCarry? _holder = null;

    [SerializeField]
    private Transform? _carryHandle;

    public Transform CarryHandle { get {
        if(_carryHandle == null)
            throw new System.Exception($"Missing carry handle on {gameObject.name}");
        return _carryHandle;
    }}

    public bool IsCarryable => _isHeld == false;
    public CharacterComponentCarry? Carrier => _holder;

    public float Mass { get {
        if(_rb == null)
            throw new System.Exception($"Missing rigidbody on {gameObject.name}.");
        return _rb.mass;
    }}

    public string GetInteractionName(Character interactor) => "Pick up";

    public Vector3 InteractionWorldPosition => transform.position;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if(_rb == null)
            throw new System.Exception($"Missing rigidbody on {gameObject.name}.");

        if(_carryHandle == null)
            throw new System.Exception($"Missing carry handle on {gameObject.name}.");

        _defaultKinematicState = _rb.isKinematic;
    }

    public bool IsInteractible(Character interactor){
        if(interactor.Carry == null)
            throw new System.Exception($"{gameObject.name} missing Carry.");
        if(interactor.Carry.HeldObject != null)
            return false;
        if(this._isHeld)
            return false;
        return true;
    }

    public void DoInteraction(Character interactor) {
        if(interactor.Carry == null)
            throw new System.Exception($"Missing Carry component on {interactor.gameObject.name}.");
        interactor.Carry.OnInteractWithCarryable(this);
    }

    public void OnPickUp(CharacterComponentCarry pickerUpper){
        if(IsCarryable == false)
            throw new System.Exception("Tried to pick up something that isn't carryable.");
        if(_isHeld == true)
            throw new System.Exception("Tried to pick up something that is already held.");
        if(_rb == null)
            throw new System.Exception($"Missing rigidbody on {gameObject.name}.");
            
        _isHeld = true;
        _holder = pickerUpper;
        
        _rb.isKinematic = true;
        // Apparently you can't do this on kinematic bodies
        // _rb.linearVelocity = Vector3.zero;
        // _rb.angularVelocity = Vector3.zero;
    }

    public void OnDrop(CharacterComponentCarry pickerUpper){
        if(_isHeld == false)
            throw new System.Exception("Tried to drop something that isn't held.");
        if(_rb == null)
            throw new System.Exception($"Missing rigidbody on {gameObject.name}.");

        _isHeld = false;
        _holder = null;

        _rb.isKinematic = _defaultKinematicState;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    public void LockInPlace(Transform targetParent){
        if(_rb == null)
            throw new System.Exception($"Missing rigidbody on {gameObject.name}.");
        if(_holder != null)
            throw new System.Exception("Cannot lock in place while held. Drop first.");

        _isHeld = true;
        _rb.isKinematic = true;

        transform.SetParent(targetParent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public sealed override bool Equals(object other) => base.Equals(other);
    public sealed override int GetHashCode() => base.GetHashCode();

    public sealed override string ToString() => base.ToString();
}
