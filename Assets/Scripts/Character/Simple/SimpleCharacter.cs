using UnityEngine;

#nullable enable

[RequireComponent(typeof(SimpleCharacterMovementPlayer))]
[RequireComponent(typeof(SimpleCharacterMovementAI))]
[RequireComponent(typeof(SimpleCharacterCrouch))]
[RequireComponent(typeof(SimpleCharacterInteract))]
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

    protected override void HandleDeath(Entity deadEntity)
    {
        base.HandleDeath(deadEntity);
        transform.Rotate(90, 0, 0);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(
            transform.position + Vector3.up * _meshBounds.extents.y, 
            _meshBounds.size
        );
    }
#endif
}
