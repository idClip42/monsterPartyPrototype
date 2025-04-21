using UnityEngine;

#nullable enable

public class SimpleMonsterKillerMachineReceptacle : MonoBehaviour, IInteractible {
    [SerializeField]
    private GameObject? _target;

    [SerializeField]
    private GameObject[] _thingsToTurnOn = {};

    private bool _hasComponent = false;
    public bool HasComponent => _hasComponent;

    public Vector3 InteractionWorldPosition => transform.position;

    private void Awake()
    {
        if(_target == null)
            throw new MonsterPartyNullReferenceException("_target");

        foreach(var thing in _thingsToTurnOn)
            thing.SetActive(false);
    }

    public bool IsInteractible(Character interactor){
        if(interactor.Carry == null)
            throw new MonsterPartyNullReferenceException("interactor.Carry");

        if(_hasComponent) return false;
        if(interactor.Carry.HeldObject == null) return false;
        if(DoesCharacterCarryTarget(interactor) == false) return false;
        return true;
    }

    public void DoInteraction(Character interactor)
    {
        if(interactor.Carry == null)
            throw new MonsterPartyNullReferenceException($"interactor.Carry");
        if(interactor.Carry.HeldObject == null) 
            throw new MonsterPartyNullReferenceException($"interactor.Carry.HeldObject");
        if(DoesCharacterCarryTarget(interactor) == false)
            throw new MonsterPartyException($"Character {interactor.gameObject.name} Carry.HeldObject in not target. Code should never have gotten here.");
        
        ICarryable component = interactor.Carry.HeldObject;
        LockInTarget(component);
    }

    public string GetInteractionName(Character interactor) {
        if(DoesCharacterCarryTarget(interactor))
            return "Place Component";
        else
            return "";
    }

#if UNITY_EDITOR
    public void ForceLockInTarget(){
        if(Application.IsPlaying(this) == false)
            throw new MonsterPartyException("Can only call this in play mode.");
        if(_target == null)
            throw new MonsterPartyNullReferenceException("_target");
        ICarryable? component = _target.GetComponent<ICarryable>();
        if(component == null)
            throw new MonsterPartyNullReferenceException("component");
        LockInTarget(component);
    }
#endif

    private void LockInTarget(ICarryable component){
        if(component.Carrier != null)
            component.Carrier.ForceDrop();

        component.LockInPlace(this.transform);
        _hasComponent = true;

        foreach(var thing in _thingsToTurnOn)
            thing.SetActive(true);

        Debug.Log($"'{component.gameObject.name}' locked into '{gameObject.name}'.");
    }

    private bool DoesCharacterCarryTarget(Character interactor){
        if(interactor.Carry == null)
            throw new MonsterPartyNullReferenceException("interactor.Carry");
        if(_target == null)
            throw new MonsterPartyNullReferenceException("_target");

        if(interactor.Carry.HeldObject == null) return false;
        if(interactor.Carry.HeldObject.gameObject != _target) return false;
        return true;
    }
}