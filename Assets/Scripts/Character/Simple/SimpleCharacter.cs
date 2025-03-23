using UnityEditor;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(SimpleCharacterMovementPlayer))]
[RequireComponent(typeof(SimpleCharacterMovementAI))]
[RequireComponent(typeof(SimpleCharacterCrouch))]
[RequireComponent(typeof(SimpleCharacterInteract))]
[RequireComponent(typeof(SimpleCharacterNoiseLevel))]
public class SimpleCharacter : Character
{
    [SerializeField]
    private GameObject? _model;

    private Bounds _meshBounds;
    public float ModelHeight => _meshBounds.size.y;

    protected override void Awake()
    {
        base.Awake();

        if(_model == null)
            throw new System.Exception($"Missing model on {gameObject.name}");

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
