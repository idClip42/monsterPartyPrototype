using System.Collections.Generic;
using UnityEditor;
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

    public sealed override string DebugHeader => "AI Movement";

    public sealed override void FillInDebugInfo(Dictionary<string, string> infoTarget)
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
        infoTarget["Max Speed"] = $"{MaxSpeed:F2} m/s";
        infoTarget["Agent Type"] = NavMesh.GetSettingsNameFromID(CurrentAgentTypeId);
        
        base.FillInDebugInfo(infoTarget);
    }

    public sealed override Vector3 CurrentVelocity { get{
        if(this._navMeshAgent == null)
            return Vector3.zero;
        if(_navMeshAgent.enabled == false) 
            return Vector3.zero;
        return this._navMeshAgent.velocity;
    }}

    public float MaxSpeed { get {
        if(this._navMeshAgent == null)
            return 0;
        if(this._navMeshAgent.enabled == false) 
            return 0;
        return this._navMeshAgent.speed;
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

    protected virtual void OnEnable()
    {
        if(_navMeshAgent == null) return;
        _navMeshAgent.enabled = true;
    }

    protected virtual void OnDisable()
    {
        if(_navMeshAgent == null) return;
        _navMeshAgent.enabled = false;
    }

    protected virtual void Update()
    {
        if(this._behavior == Behavior.Follow){
            UpdateFollow();
        }
        else if(this._behavior == Behavior.HoldPosition) {
            // UpdateHoldPosition();
        }
        else {
            throw new System.Exception($"Unhandled behavior: {this._behavior}");
        }
    }

    private void UpdateFollow(){
        if(this.Character == null)
            throw new System.Exception($"Null character on {this.gameObject.name}");
        if(this.Character.Crouch == null)
            throw new System.Exception($"Null crouch on {this.gameObject.name}");
        if(_navMeshAgent == null) 
            throw new System.Exception("Null _navMeshAgent");
        if(_behaviorTarget == null) 
            throw new System.Exception("Null _behaviorTarget");
        if(_behaviorTarget.Crouch == null) 
            throw new System.Exception("Null _behaviorTarget.Crouch");

        if(_navMeshAgent.enabled == false)
            throw new System.Exception("Nav mesh agent not enabled");

        this._navMeshAgent.SetDestination(this._behaviorTarget.transform.position);
        this.Character.Crouch.SetCrouching(this._behaviorTarget.Crouch.IsCrouching);
        float followSpeed = GetFollowSpeed(this._behaviorTarget);
        _navMeshAgent.speed = Mathf.Clamp(followSpeed, 0, GetMaxMoveSpeed());
    }

    // private void UpdateHoldPosition(){
    //     if(this.Character == null)
    //         throw new System.Exception($"Null character on {this.gameObject.name}");
    //     if(this.Character.Crouch == null)
    //         throw new System.Exception($"Null crouch on {this.gameObject.name}");
    //     if(_navMeshAgent == null) 
    //         throw new System.Exception("Null _navMeshAgent");

    //     if(this.Character.Crouch.IsCrouching)
    //         _navMeshAgent.speed = this.Character.Movement.CrouchSpeed;
    //     else
    //         _navMeshAgent.speed = this.Character.Movement.WalkSpeed;
    // }

    private void OnCrouchToggle(bool isCrouching){
        if(_navMeshAgent == null) 
            throw new System.Exception("Null _navMeshAgent");
        this._navMeshAgent.agentTypeID = CurrentAgentTypeId;
    }

    private void SetBehavior(Behavior behavior, Character? target){
        if(_navMeshAgent == null) 
            throw new System.Exception("Null _navMeshAgent");
        if(_navMeshAgent.enabled == false) 
            throw new System.Exception("Nav mesh agent not enabled");

        if(behavior == Behavior.Follow && target == null)
            throw new System.Exception("Attempted to set Follow behavior without setting a target.");

        this._behavior = behavior;
        this._behaviorTarget = target;

        // Stop any navigating
        _navMeshAgent.isStopped = this._behavior == Behavior.HoldPosition;
    }

    public bool IsInteractible(Character interactor) {
        return this.Character ? 
            this.Character.Alive : 
            false;
    }

    public string GetInteractionName(Character interactor) {
        switch (_behavior)
        {
            case Behavior.HoldPosition:
                return "Follow";
            case Behavior.Follow:
                if(_behaviorTarget == interactor){
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
                if(_behaviorTarget == interactor){
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

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        const float RADIUS = 0.5f;

        if(_behavior == Behavior.Follow){
            if(_behaviorTarget == null)
                throw new System.Exception("Missing behavior target!");

            Vector3 myPos = transform.position;
            Vector3 theirPos = _behaviorTarget.transform.position;
            Vector3 distance = theirPos - myPos;
            Vector3 direction = distance.normalized;
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;

            using(new Handles.DrawingScope(Color.white)){
                Handles.DrawWireDisc(myPos, Vector3.up, RADIUS);
                Handles.DrawWireDisc(theirPos, Vector3.up, RADIUS);
                Handles.DrawLine(myPos + right * RADIUS, theirPos + right * RADIUS);
                Handles.DrawLine(myPos - right * RADIUS, theirPos - right * RADIUS);
            }
        }
    }
#endif

    private float GetFollowSpeed(Character target){
        if(target.Crouch == null) 
            throw new System.Exception("Null _behaviorTarget.Crouch");

        if(target.State == Character.StateType.AI){
            if(target.AIMovement == null) 
                throw new System.Exception("Null _behaviorTarget.AIMovement");
            return target.AIMovement.MaxSpeed;
        }
        else if(target.State == Character.StateType.Player){
            if(target.PlayerMovement == null) 
                throw new System.Exception("Null this.Character.PlayerMovement");
            return Mathf.Min(
                target.PlayerMovement.GetDesiredSpeed(),
                this.GetMaxMoveSpeed()
            );
        }
        else {
            throw new System.Exception($"Unhandled state type '{target.State}'");
        }
    }
}
