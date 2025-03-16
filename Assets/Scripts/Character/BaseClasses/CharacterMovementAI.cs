using UnityEngine;
using UnityEngine.AI;

#nullable enable

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterCrouch))]
[DisallowMultipleComponent]
public abstract class CharacterMovementAI : CharacterMovement, IInteractible, IDebugInfoProvider
{
    public enum Behavior { HoldPosition, Follow }

    private int CurrentAgentTypeId { get {
        if(_crouch == null) return 0;
        if(_navManager == null) return 0;
        return _crouch.IsCrouching ?
            _navManager.CrouchingAgentTypeId : 
            _navManager.StandingAgentTypeId;
    }}

    private NavigationManager? _navManager = null;
    private NavMeshAgent? _navMeshAgent = null;
    private CharacterCrouch? _crouch = null;
    private Behavior _behavior = Behavior.HoldPosition;
    private Transform? _behaviorTarget = null;
    public string CurrentBehavior => $"{_behavior} : {_behaviorTarget?.gameObject?.name}";

    public Vector3 InteractionWorldPosition => this.gameObject.transform.position + Vector3.up;

    public string DebugName => "AI Movement";
    public string DebugInfo { get {
        if(this.enabled == false) return "Off";
        if(_navMeshAgent == null) throw new System.Exception("Null _navMeshAgent");
        return $"{_behavior}, {_behaviorTarget?.gameObject?.name}, {CurrentVelocity.magnitude:F2} m/s, Agent Type: '{NavMesh.GetSettingsNameFromID(CurrentAgentTypeId)}'";
    }}

    public override Vector3 CurrentVelocity { get{
        if(this._navMeshAgent == null)
            return Vector3.zero;
        return this._navMeshAgent.velocity;
    }}

    private void Awake(){
        _navManager = FindFirstObjectByType<NavigationManager>();
        if(_navManager == null)
            throw new System.Exception($"Null _navManager on {this.gameObject.name}");

        _navMeshAgent = GetComponent<NavMeshAgent>();
        if(_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        _crouch = GetComponent<CharacterCrouch>();
        if(_crouch == null)
            throw new System.Exception($"Null crouch on {this.gameObject.name}");
        _crouch.OnCrouchToggle += OnCrouchToggle;
    }

    private void OnEnable()
    {
        if(_navMeshAgent == null) return;
        _navMeshAgent.enabled = true;
    }

    private void OnDisable()
    {
        if(_navMeshAgent == null) return;
        _navMeshAgent.enabled = false;
    }

    private void Update()
    {
        if(_navMeshAgent == null) throw new System.Exception("Null _navMeshAgent");
        if(this._behavior == Behavior.Follow){
            if(_behaviorTarget == null) throw new System.Exception("Null _behaviorTarget");
            this._navMeshAgent.SetDestination(this._behaviorTarget.transform.position);
        }
    }

    private void OnCrouchToggle(bool isCrouching){
        if(_navMeshAgent == null) throw new System.Exception("Null _navMeshAgent");
        if(_crouch == null) throw new System.Exception("Null _crouch");
        this._navMeshAgent.agentTypeID = CurrentAgentTypeId;
    }

    private void SetBehavior(Behavior behavior, Transform? target){
        if(_navMeshAgent == null) throw new System.Exception("Null _navMeshAgent");

        if(behavior == Behavior.Follow && target == null)
            throw new System.Exception("Attempted to set Follow behavior without setting a target.");

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
