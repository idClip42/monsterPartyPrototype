using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

#nullable enable

public enum State { Player, AI };

public abstract class Character : MonoBehaviour
{
    private CharacterMovementPlayer? _playerMovement = null;
    private CharacterMovementAI? _aiMovement = null;

    private ICharacterComponent[] _components = {};

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

    public void Awake()
    {
        _playerMovement = GetComponent<CharacterMovementPlayer>();
        if(_playerMovement == null)
            throw new Exception($"Missing CharacterMovementPlayer on {this.gameObject.name}");
            
        _aiMovement = GetComponent<CharacterMovementAI>();
        if(_aiMovement == null)
            throw new Exception($"Missing CharacterMovementAI on {this.gameObject.name}");

        _components = GetComponents<ICharacterComponent>();
    }

    #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Color prevColor = Handles.color;
            Handles.color = Color.white;

            string text = @$"
{this.gameObject.name}
{this._state}
{String.Join('\n', _components.Select(c=>$"{c.DebugName}: {c.DebugInfo}"))}
            ".Trim();

            Handles.Label(
                transform.position + Vector3.up * 1f,
                text
            );
            Handles.color = prevColor;
        }
    #endif
}
