using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.PackageManager;

#nullable enable

[DisallowMultipleComponent]
public abstract class Character : Entity, IDebugInfoProvider
{
    public enum StateType { Player, AI, Dead };

    private CharacterMovementPlayer? _playerMovement = null;
    private CharacterMovementAI? _aiMovement = null;
    private Transform[] _lookRaycastTargets = {};

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
                case StateType.Dead:
                    _playerMovement.enabled = false;
                    _aiMovement.enabled = false;
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

    private void HandleDeath(Entity deadEntity){
        if(deadEntity != this)
            throw new Exception("This function should only be called for own death.");
        this.State = StateType.Dead;
    }
}
