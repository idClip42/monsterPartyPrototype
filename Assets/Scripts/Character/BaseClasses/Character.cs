using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

#nullable enable

[RequireComponent(typeof(CharacterMovementPlayer))]
[RequireComponent(typeof(CharacterMovementAI))]
[RequireComponent(typeof(CharacterCrouch))]
[RequireComponent(typeof(CharacterInteract))]
[RequireComponent(typeof(CharacterNoiseLevel))]
[DisallowMultipleComponent]
public abstract class Character : Entity, IDebugInfoProvider
{
    [System.Serializable]
    public class MovementConfig{
        [SerializeField]
        [Range(1,10)]
        private float _crouchSpeed = 2.0f;
        
        [SerializeField]
        [Range(1,10)]
        private float _walkSpeed = 3.0f;
        
        [SerializeField]
        [Range(1,10)]
        private float _runSpeed = 5.0f;

        public float CrouchSpeed => _crouchSpeed;
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
    }

    public enum StateType { Player, AI };

    [SerializeField]
    private MovementConfig? _movementConfig;

    private Transform[] _lookRaycastTargets = {};

    public abstract CharacterCrouch? Crouch { get; }
    public abstract CharacterInteract? Interact { get; }
    public abstract CharacterMovementPlayer? PlayerMovement { get; }
    public abstract CharacterMovementAI? AIMovement { get; }
    public abstract CharacterNoiseLevel? NoiseLevel { get; }

    public MovementConfig Movement { get{
        if(_movementConfig == null)
            throw new Exception("Missing movement config. Should never happen.");
        return _movementConfig;
    }}
    public IReadOnlyCollection<Transform> LookRaycastTargets => _lookRaycastTargets;

    public string DebugHeader => "Character";

    public void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        infoTarget["State"] = this._state.ToString();
        infoTarget["Raycast Targets"] = _lookRaycastTargets.Length.ToString();
    }

    public Vector3 CurrentVelocity { get{
        if(PlayerMovement != null && PlayerMovement.enabled){
            return PlayerMovement.CurrentVelocity;
        }
        else if(AIMovement != null && AIMovement.enabled){
            return AIMovement.CurrentVelocity;
        }
        else {
            return Vector3.zero;
        }
    }}

    private StateType _state = StateType.AI;
    public StateType State {
        get {
            return _state;
        }
        set {
            if(PlayerMovement == null) throw new Exception("Null _playerMovement");
            if(AIMovement == null) throw new Exception("Null _aiMovement");

            _state = value;
            if(this.Alive == false) return;
            switch (_state)
            {
                case StateType.Player:
                    PlayerMovement.enabled = true;
                    AIMovement.enabled = false;
                    break;
                case StateType.AI:
                    PlayerMovement.enabled = false;
                    AIMovement.enabled = true;
                    break;
                default:
                    throw new Exception($"Unknown state enum for {this.gameObject.name}: {_state}");
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        _lookRaycastTargets = GetComponentsInChildren<CharacterLookRaycastTarget>(false)
            .Select(item => item.gameObject.transform).ToArray();

        this.OnDeath += HandleDeath;
    }

    public void Kill(){
        this.Die();
    }

    protected virtual void HandleDeath(Entity deadEntity){        
        if(deadEntity != this)
            throw new Exception("This function should only be called for own death.");
        if(PlayerMovement == null) 
            throw new Exception("Null _playerMovement");
        if(AIMovement == null) 
            throw new Exception("Null _aiMovement");

        PlayerMovement.enabled = false;
        AIMovement.enabled = false;
    }
}
