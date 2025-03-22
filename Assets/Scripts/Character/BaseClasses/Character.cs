using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

#nullable enable

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
    private MovementConfig _movementConfig;

    private CharacterMovementPlayer? _playerMovement = null;
    private CharacterMovementAI? _aiMovement = null;
    private Transform[] _lookRaycastTargets = {};

    public MovementConfig Movement => _movementConfig;
    public IReadOnlyCollection<Transform> LookRaycastTargets => _lookRaycastTargets;

    public string DebugName => "Character";
    public string DebugInfo { get {
        return $"{this._state}, {_lookRaycastTargets.Length} Raycast Targets";
    }}

    public Vector3 CurrentVelocity { get{
        if(_playerMovement != null && _playerMovement.enabled){
            return _playerMovement.CurrentVelocity;
        }
        else if(_aiMovement != null && _aiMovement.enabled){
            return _aiMovement.CurrentVelocity;
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
            if(_playerMovement == null) throw new Exception("Null _playerMovement");
            if(_aiMovement == null) throw new Exception("Null _aiMovement");

            _state = value;
            if(this.Alive == false) return;
            switch (_state)
            {
                case StateType.Player:
                    _playerMovement.enabled = true;
                    _aiMovement.enabled = false;
                    break;
                case StateType.AI:
                    _playerMovement.enabled = false;
                    _aiMovement.enabled = true;
                    break;
                default:
                    throw new Exception($"Unknown state enum for {this.gameObject.name}: {_state}");
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        _playerMovement = GetComponent<CharacterMovementPlayer>();
        if(_playerMovement == null)
            throw new Exception($"Missing CharacterMovementPlayer on {this.gameObject.name}");
            
        _aiMovement = GetComponent<CharacterMovementAI>();
        if(_aiMovement == null)
            throw new Exception($"Missing CharacterMovementAI on {this.gameObject.name}");

        _lookRaycastTargets = GetComponentsInChildren<CharacterLookRaycastTarget>(false).Select(item => item.gameObject.transform).ToArray();

        this.OnDeath += HandleDeath;
    }

    public void Kill(){
        this.Die();
    }

    protected virtual void HandleDeath(Entity deadEntity){        
        if(deadEntity != this)
            throw new Exception("This function should only be called for own death.");
        if(_playerMovement == null) 
            throw new Exception("Null _playerMovement");
        if(_aiMovement == null) 
            throw new Exception("Null _aiMovement");

        _playerMovement.enabled = false;
        _aiMovement.enabled = false;
    }
}
