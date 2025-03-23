using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#nullable enable

[RequireComponent(typeof(NavMeshAgent))]
[DisallowMultipleComponent]
public abstract class CharacterComponentMovementAI : CharacterComponentMovement, IInteractible
{
    public enum Behavior { HoldPosition, Follow }

    private int CurrentAgentTypeId { get {
        if(this.Character == null) return 0;
        if(this.Character.Crouch == null) return 0;
        if(_navManager == null) return 0;
        return this.Character.Crouch.IsCrouching ?
            _navManager.CrouchingAgentTypeId : 
            _navManager.StandingAgentTypeId;
    }}

    private NavigationManager? _navManager = null;
    private NavMeshAgent? _navMeshAgent = null;
    private Behavior _behavior = Behavior.HoldPosition;
    private Character? _behaviorTarget = null;
    public string CurrentBehavior => $"{_behavior} : {_behaviorTarget?.gameObject?.name}";

    public Vector3 InteractionWorldPosition => this.gameObject.transform.position + Vector3.up;
    public bool IsInteractible => this.Character ? this.Character.Alive : false;

    public override string DebugHeader => "AI Movement";

    public override void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        if(this.enabled == false){
            infoTarget["Enabled"] = "Off";
            return;
        }

        if(_navMeshAgent == null){
            infoTarget["Agent"] = "Unconnected";
            return;
        }

        infoTarget["Behavior"] = _behavior.ToString();
        infoTarget["Target"] = _behaviorTarget ?
            _behaviorTarget.gameObject.name :
            "None";
        infoTarget["Speed"] = $"{CurrentVelocity.magnitude:F2} m/s";
        infoTarget["Agent Type"] = NavMesh.GetSettingsNameFromID(CurrentAgentTypeId);
    }

    public override Vector3 CurrentVelocity { get{
        if(this._navMeshAgent == null)
            return Vector3.zero;
        return this._navMeshAgent.velocity;
    }}

    protected override void Awake(){
        base.Awake();

        _navManager = FindFirstObjectByType<NavigationManager>();
        if(_navManager == null)
            throw new System.Exception($"Null _navManager on {this.gameObject.name}");

        _navMeshAgent = GetComponent<NavMeshAgent>();
        if(_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");
    }

    private void Start()
    {
        if(this.Character == null)
            throw new System.Exception($"Null character on {this.gameObject.name}");
        if(this.Character.Crouch == null)
            throw new System.Exception($"Null crouch on {this.gameObject.name}");
        this.Character.Crouch.OnCrouchToggle += OnCrouchToggle;
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
        if(this.Character == null)
            throw new System.Exception($"Null character on {this.gameObject.name}");
        if(_navMeshAgent == null) 
            throw new System.Exception("Null _navMeshAgent");
        if(this.Character.Crouch == null)
            throw new System.Exception($"Null crouch on {this.gameObject.name}");

        if(this._behavior == Behavior.Follow){
            if(_behaviorTarget == null) throw new System.Exception("Null _behaviorTarget");
            if(_behaviorTarget.Crouch == null) throw new System.Exception("Null _behaviorTarget.Crouch");

            this._navMeshAgent.SetDestination(this._behaviorTarget.transform.position);
            this.Character.Crouch.SetCrouching(this._behaviorTarget.Crouch.IsCrouching);
        }

        if(this.Character.Crouch.IsCrouching)
            _navMeshAgent.speed = this.Character.Movement.CrouchSpeed;
        else
            _navMeshAgent.speed = this.Character.Movement.RunSpeed;
    }

    private void OnCrouchToggle(bool isCrouching){
        if(_navMeshAgent == null) throw new System.Exception("Null _navMeshAgent");
        this._navMeshAgent.agentTypeID = CurrentAgentTypeId;
    }

    private void SetBehavior(Behavior behavior, Character? target){
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
                SetBehavior(Behavior.Follow, interactor);
                break;
            case Behavior.Follow:
                if(_behaviorTarget == interactor.transform){
                    SetBehavior(Behavior.HoldPosition, null);
                }
                else {
                    SetBehavior(Behavior.Follow, interactor);
                }
                break;
            default:
                throw new System.Exception($"Unhandled behavior: {_behavior}");
        }
    }
}
