using UnityEngine;
using UnityEditor;

public enum State { Player, AI };

public abstract class Character : MonoBehaviour
{
    private CharacterMovementPlayer _playerMovement;
    private CharacterMovementAI _aiMovement;
    private CharacterCrouch _crouch;

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
            
        _crouch = GetComponent<CharacterCrouch>();
        if(_crouch == null)
            throw new System.Exception($"Missing CharacterCrouch on {this.gameObject.name}");

        CharacterInteract interact = GetComponent<CharacterInteract>();
        if(interact == null)
            throw new System.Exception($"Missing CharacterInteract on {this.gameObject.name}");
        
        _state = State.AI;
    }

    #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Color prevColor = Handles.color;
            Handles.color = Color.white;

            string text = @$"
    {this.gameObject.name}
    {this._state}
    {this._aiMovement?.CurrentBehavior}
    Crouch: {this._crouch?.IsCrouching}
            ".Trim();

            Handles.Label(
                transform.position + Vector3.up * 2f,
                text
            );
            Handles.color = prevColor;
        }
    #endif
}
