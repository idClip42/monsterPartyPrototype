using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class CharacterMovementAI : MonoBehaviour, IInteractible, ICharacterComponent
{
    public enum Behavior { HoldPosition, Follow }

    private NavMeshAgent _navMeshAgent = null;
    private Behavior _behavior = Behavior.HoldPosition;
    private Transform _behaviorTarget = null;
    public string CurrentBehavior => $"{_behavior} : {_behaviorTarget?.gameObject?.name}";

    public Vector3 InteractionWorldPosition => this.gameObject.transform.position + Vector3.up;

    public string DebugName => "AI Movement";
    public string DebugInfo { get {
        if(this.enabled == false) return "Off";
        return $"{_behavior}, {_behaviorTarget?.gameObject?.name}, {_navMeshAgent.speed}";
    }}

    void Awake(){
        _navMeshAgent = GetComponent<NavMeshAgent>();
        if(_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");
    }

    void OnEnable()
    {
        _navMeshAgent.enabled = true;
    }

    void OnDisable()
    {
        _navMeshAgent.enabled = false;
    }

    void Update()
    {
        if(this._behavior == Behavior.Follow){
            this._navMeshAgent.SetDestination(this._behaviorTarget.transform.position);
        }
    }

    private void SetBehavior(Behavior behavior, Transform target){
        this._behavior = behavior;
        this._behaviorTarget = target;

        // Stop any navigating
        _navMeshAgent.isStopped = this._behavior == Behavior.HoldPosition;
    }

    public string GetInteractionName(Character interactor) {
        switch (_behavior)
        {
            case Behavior.HoldPosition:
                return "Follow";
            case Behavior.Follow:
                if(_behaviorTarget == interactor.transform){
                    return "Stop";
                }
                else {
                    return "Follow";
                }
            default:
                throw new System.Exception($"Unhandled behavior: {_behavior}");
        }
    }

    public void DoInteraction(Character interactor)
    {
        switch (_behavior)
        {
            case Behavior.HoldPosition:
                SetBehavior(Behavior.Follow, interactor.transform);
                break;
            case Behavior.Follow:
                if(_behaviorTarget == interactor.transform){
                    SetBehavior(Behavior.HoldPosition, null);
                }
                else {
                    SetBehavior(Behavior.Follow, interactor.transform);
                }
                break;
            default:
                throw new System.Exception($"Unhandled behavior: {_behavior}");
        }
    }
}
