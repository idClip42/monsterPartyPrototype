using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

#nullable enable

[RequireComponent(typeof(CharacterComponentMovementPlayer))]
[RequireComponent(typeof(CharacterComponentMovementAI))]
[RequireComponent(typeof(CharacterComponentCrouch))]
[RequireComponent(typeof(CharacterComponentInteract))]
[RequireComponent(typeof(CharacterComponentNoiseLevel))]
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

    [SerializeField]
    private AudioSource? _deathScream = null;

    private Transform[] _lookRaycastTargets = {};

    public abstract CharacterComponentCrouch? Crouch { get; }
    public abstract CharacterComponentInteract? Interact { get; }
    public abstract CharacterComponentMovementPlayer? PlayerMovement { get; }
    public abstract CharacterComponentMovementAI? AIMovement { get; }
    public abstract CharacterComponentNoiseLevel? NoiseLevel { get; }
    public abstract CharacterComponentCarry? Carry { get; }

    public MovementConfig Movement { get{
        if(_movementConfig == null)
            throw new Exception("Missing movement config. Should never happen.");
        return _movementConfig;
    }}
    public IReadOnlyCollection<Transform> LookRaycastTargets => _lookRaycastTargets;

    public string DebugHeader => "Character";

    public virtual void FillInDebugInfo(Dictionary<string, string> infoTarget)
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
    public StateType State => _state;

    public void SetState(StateType newState)
    {
        if(PlayerMovement == null) 
            throw new Exception("Null PlayerMovement");
        if(AIMovement == null) 
            throw new Exception("Null AIMovement");

        _state = newState;
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

    protected override void Awake()
    {
        base.Awake();

        _lookRaycastTargets = GetComponentsInChildren<CharacterLookRaycastTarget>(false)
            .Select(item => item.gameObject.transform).ToArray();

        this.OnDeath += HandleDeath;
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

        if(_deathScream != null)
            _deathScream.Play();
        else
            Debug.LogWarning("Missing death scream");
    }
}
