using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

#nullable enable

[DisallowMultipleComponent]
public abstract class Character : Entity, IDebugInfoProvider
{
    public enum BrainType { Player, AI };

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

    private BrainType _state = BrainType.AI;
    public BrainType Brain {
        get {
            return _state;
        }
        set {
            if(_playerMovement == null) throw new Exception("Null _playerMovement");
            if(_aiMovement == null) throw new Exception("Null _aiMovement");

            _state = value;
            switch (_state)
            {
                case BrainType.Player:
                    _playerMovement.enabled = true;
                    _aiMovement.enabled = false;
                    break;
                case BrainType.AI:
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
    }
}
