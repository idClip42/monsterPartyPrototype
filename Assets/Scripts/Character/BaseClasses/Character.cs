using UnityEngine;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

#nullable enable

public enum State { Player, AI };

[DisallowMultipleComponent]
public abstract class Character : Entity, IDebugInfoProvider
{
    private CharacterMovementPlayer? _playerMovement = null;
    private CharacterMovementAI? _aiMovement = null;
    private Transform[] _lookRaycastTargets = {};

    public IReadOnlyCollection<Transform> LookRaycastTargets => _lookRaycastTargets;

    public string DebugName => "Character";
    public string DebugInfo { get {
        return $"{this._state}, {_lookRaycastTargets.Length} Raycast Targets";
    }}

    private State _state = State.AI;
    public State State {
        get {
            return _state;
        }
        set {
            if(_playerMovement == null) throw new Exception("Null _playerMovement");
            if(_aiMovement == null) throw new Exception("Null _aiMovement");

            _state = value;
            switch (_state)
            {
                case State.Player:
                    _playerMovement.enabled = true;
                    _aiMovement.enabled = false;
                    break;
                case State.AI:
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

        _lookRaycastTargets = GetComponentsInChildren<CharacterLookRaycastTarget>().Select(item => item.gameObject.transform).ToArray();
    }
}
