using UnityEditor;
using UnityEngine;

public enum State { Player, AI };

[RequireComponent(typeof(CharacterMovementPlayer))]
[RequireComponent(typeof(CharacterMovementAI))]
public class CharacterBase : MonoBehaviour
{
    private CharacterMovementPlayer _playerMovement;
    private CharacterMovementAI _aiMovement;
    private State _state;

    public State State {
        get {
            return _state;
        }
        set {
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
                    throw new System.Exception($"Unknown state enum for {this.gameObject.name}: {_state}");
            }
        }
    }

    public void Awake()
    {
        _playerMovement = GetComponent<CharacterMovementPlayer>();
        if(_playerMovement == null)
            throw new System.Exception($"Missing CharacterMovementPlayer on {this.gameObject.name}");
        _aiMovement = GetComponent<CharacterMovementAI>();
        if(_aiMovement == null)
            throw new System.Exception($"Missing CharacterMovementAI on {this.gameObject.name}");
        
        _state = State.AI;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Color prevColor = Handles.color;
        Handles.color = Color.white;
        Handles.Label(
            transform.position + Vector3.up * 2f,
            $"{this.gameObject.name}\n{this._state}\n{this._aiMovement?.CurrentBehavior}"
        );
        Handles.color = prevColor;
    }
#endif
}
