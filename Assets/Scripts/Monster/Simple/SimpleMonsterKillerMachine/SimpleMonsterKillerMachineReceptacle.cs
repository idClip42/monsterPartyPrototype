using UnityEngine;

#nullable enable

public class SimpleMonsterKillerMachineReceptacle : MonoBehaviour, IInteractible {
    [SerializeField]
    private GameObject? _target;

    private bool _hasComponent = false;
    public bool HasComponent => _hasComponent;

    public bool IsInteractible => _hasComponent == false;

    public Vector3 InteractionWorldPosition => transform.position;

    void Awake()
    {
        if(_target == null)
            throw new System.Exception($"Missing target on {gameObject.name}.");
    }

    public void DoInteraction(Character interactor)
    {
        if(interactor.Carry == null)
            throw new System.Exception($"Character {interactor.gameObject.name} missing Carry.");
        if(interactor.Carry.HeldObject == null) return;
        if(DoesCharacterCarryTarget(interactor) == false) return;
        
        ICarryable component = interactor.Carry.HeldObject;
        interactor.Carry.ForceDrop();
        component.LockInPlace(this.transform);
        _hasComponent = true;
    }

    public string GetInteractionName(Character interactor) {
        if(DoesCharacterCarryTarget(interactor))
            return "Place Component";
        else
            return "";
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