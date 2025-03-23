using UnityEditor;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(SimpleCharacterComponentMovementPlayer))]
[RequireComponent(typeof(SimpleCharacterComponentMovementAI))]
[RequireComponent(typeof(SimpleCharacterComponentCrouch))]
[RequireComponent(typeof(SimpleCharacterComponentInteract))]
[RequireComponent(typeof(SimpleCharacterComponentNoiseLevel))]
public class SimpleCharacter : Character
{
    [SerializeField]
    private GameObject? _model;

    private Bounds _meshBounds;
    public float ModelHeight => _meshBounds.size.y;

    private SimpleCharacterComponentCrouch? _crouch = null;
    private SimpleCharacterComponentInteract? _interact = null;
    private SimpleCharacterComponentMovementPlayer? _playerMovement = null;
    private SimpleCharacterComponentMovementAI? _aiMovement = null;
    private SimpleCharacterComponentNoiseLevel? _noiseLevel = null;

    public override CharacterComponentCrouch? Crouch => _crouch;
    public override CharacterComponentInteract? Interact => _interact;
    public override CharacterComponentMovementPlayer? PlayerMovement => _playerMovement;
    public override CharacterComponentMovementAI? AIMovement => _aiMovement;
    public override CharacterComponentNoiseLevel? NoiseLevel => _noiseLevel;

    protected override void Awake()
    {
        base.Awake();

        if(_model == null)
            throw new System.Exception($"Missing model on {gameObject.name}");

        _crouch = GetComponent<SimpleCharacterComponentCrouch>();
        if(_crouch == null)
            throw new System.Exception($"Missing SimpleCharacterCrouch on {this.gameObject.name}");
        _interact = GetComponent<SimpleCharacterComponentInteract>();
        if(_interact == null)
            throw new System.Exception($"Missing SimpleCharacterInteract on {this.gameObject.name}");
        _playerMovement = GetComponent<SimpleCharacterComponentMovementPlayer>();
        if(_playerMovement == null)
            throw new System.Exception($"Missing SimpleCharacterMovementPlayer on {this.gameObject.name}");
        _aiMovement = GetComponent<SimpleCharacterComponentMovementAI>();
        if(_aiMovement == null)
            throw new System.Exception($"Missing SimpleCharacterMovementAI on {this.gameObject.name}");
        _noiseLevel = GetComponent<SimpleCharacterComponentNoiseLevel>();
        if(_noiseLevel == null)
            throw new System.Exception($"Missing SimpleCharacterNoiseLevel on {this.gameObject.name}");

        Renderer[] renderers = GetComponentsInChildren<Renderer>(false);
        _meshBounds.center = transform.position;
        foreach(var rend in renderers){
            _meshBounds.Encapsulate(rend.bounds);
        }
    }

    void Update()
    {
        if(_model == null)
            throw new System.Exception($"Missing model on {gameObject.name}");
            
        Vector3 projectedMovementVelocity = Vector3.ProjectOnPlane(
            this.CurrentVelocity,
            Vector3.up
        );
        if(projectedMovementVelocity != Vector3.zero){
            Vector3 direction = projectedMovementVelocity.normalized;
            _model.transform.forward = direction;
        }
    }

    protected override void HandleDeath(Entity deadEntity)
    {
        base.HandleDeath(deadEntity);
        transform.Rotate(90, 0, 0);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        using(new Handles.DrawingScope(Color.white)){
            Gizmos.DrawWireCube(
                transform.position + Vector3.up * _meshBounds.extents.y, 
                _meshBounds.size
            );
        }
    }
#endif
}
