using UnityEngine;

#nullable enable

public class SimpleMonsterKillerMachineReceptacle : MonoBehaviour, IInteractible {
    [SerializeField]
    private GameObject? _target;

    [SerializeField]
    private GameObject[] _thingsToTurnOn = {};

    private bool _hasComponent = false;
    public bool HasComponent => _hasComponent;

    public bool IsInteractible => _hasComponent == false;

    public Vector3 InteractionWorldPosition => transform.position;

    private void Awake()
    {
        if(_target == null)
            throw new System.Exception($"Missing target on {gameObject.name}.");

        foreach(var thing in _thingsToTurnOn)
            thing.SetActive(false);
    }

    public void DoInteraction(Character interactor)
    {
        if(interactor.Carry == null)
            throw new System.Exception($"Character {interactor.gameObject.name} missing Carry.");
        if(interactor.Carry.HeldObject == null) return;
        if(DoesCharacterCarryTarget(interactor) == false) return;
        
        ICarryable component = interactor.Carry.HeldObject;
        LockInTarget(component);
    }

    public string GetInteractionName(Character interactor) {
        if(DoesCharacterCarryTarget(interactor))
            return "Place Component";
        else
            return "";
    }

    public void ForceLockInTarget(){
        if(Application.IsPlaying(this) == false)
            throw new System.Exception("Can only call this in play mode.");
        if(_target == null)
            throw new System.Exception($"Missing target on {gameObject.name}.");
        ICarryable? component = _target.GetComponent<ICarryable>();
        if(component == null)
            throw new System.Exception($"Missing ICarryable on {_target.name}.");
        LockInTarget(component);
    }

    private void LockInTarget(ICarryable component){
        if(component.Carrier != null)
            component.Carrier.ForceDrop();

        component.LockInPlace(this.transform);
        _hasComponent = true;

        foreach(var thing in _thingsToTurnOn)
            thing.SetActive(true);
    }

    private bool DoesCharacterCarryTarget(Character interactor){
        if(interactor.Carry == null)
            throw new System.Exception($"Character {interactor.gameObject.name} missing Carry.");
        if(_target == null)
            throw new System.Exception($"Missing target on {gameObject.name}.");

        if(interactor.Carry.HeldObject == null) return false;
        if(interactor.Carry.HeldObject.gameObject != _target) return false;
        return true;
    }
}