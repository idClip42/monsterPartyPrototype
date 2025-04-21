using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(SimpleCharacterComponentMovementPlayer))]
[RequireComponent(typeof(SimpleCharacterComponentMovementAI))]
[RequireComponent(typeof(SimpleCharacterComponentCrouch))]
[RequireComponent(typeof(SimpleCharacterComponentInteract))]
[RequireComponent(typeof(SimpleCharacterComponentCarry))]
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
    private SimpleCharacterComponentCarry? _carry = null;

    public override CharacterComponentCrouch GetCrouchComponent()
    {
        if(_crouch == null)
            throw new MonsterPartyNullReferenceException("_crouch");
        return _crouch;
    }

    public override CharacterComponentInteract GetInteractComponent()
    {
        if(_interact == null)
            throw new MonsterPartyNullReferenceException("_interact");
        return _interact;
    }

    public override CharacterComponentMovementPlayer GetPlayerMovementComponent()
    {
        if(_playerMovement == null)
            throw new MonsterPartyNullReferenceException("_playerMovement");
        return _playerMovement;
    }

    public override CharacterComponentMovementAI GetAiMovementComponent()
    {
        if(_aiMovement == null)
            throw new MonsterPartyNullReferenceException("_aiMovement");
        return _aiMovement;
    }

    public override CharacterComponentNoiseLevel GetNoiseLevelComponent()
    {
        if(_noiseLevel == null)
            throw new MonsterPartyNullReferenceException("_noiseLevel");
        return _noiseLevel;
    }

    public override CharacterComponentCarry GetCarryComponent()
    {
        if(_carry == null)
            throw new MonsterPartyNullReferenceException("_carry");
        return _carry;
    }

    public sealed override void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        base.FillInDebugInfo(infoTarget);
        infoTarget["Model"] = _model ? _model.name : "None";
        infoTarget["Model Height"] = ModelHeight.ToString("F2");
    }

    protected sealed override void Awake()
    {
        base.Awake();

        if(_model == null)
            throw new MonsterPartyNullReferenceException(this, "_model");

        _crouch = GetComponent<SimpleCharacterComponentCrouch>();
        if(_crouch == null)
            throw new MonsterPartyGetComponentException<SimpleCharacterComponentCrouch>(this);

        _interact = GetComponent<SimpleCharacterComponentInteract>();
        if(_interact == null)
            throw new MonsterPartyGetComponentException<SimpleCharacterComponentInteract>(this);

        _playerMovement = GetComponent<SimpleCharacterComponentMovementPlayer>();
        if(_playerMovement == null)
            throw new MonsterPartyGetComponentException<SimpleCharacterComponentMovementPlayer>(this);

        _aiMovement = GetComponent<SimpleCharacterComponentMovementAI>();
        if(_aiMovement == null)
            throw new MonsterPartyGetComponentException<SimpleCharacterComponentMovementAI>(this);

        _noiseLevel = GetComponent<SimpleCharacterComponentNoiseLevel>();
        if(_noiseLevel == null)
            throw new MonsterPartyGetComponentException<SimpleCharacterComponentNoiseLevel>(this);

        _carry = GetComponent<SimpleCharacterComponentCarry>();
        if(_carry == null)
            throw new MonsterPartyGetComponentException<SimpleCharacterComponentCarry>(this);

        Renderer[] renderers = GetComponentsInChildren<Renderer>(false);
        _meshBounds.center = transform.position;
        foreach(var rend in renderers){
            _meshBounds.Encapsulate(rend.bounds);
        }
    }

    void Update()
    {
        if(_model == null)
            throw new MonsterPartyNullReferenceException(this, "_model");
            
        Vector3 projectedMovementVelocity = Vector3.ProjectOnPlane(
            this.GetCurrentMovementComponent().CurrentVelocity,
            Vector3.up
        );
        if(projectedMovementVelocity != Vector3.zero){
            Vector3 direction = projectedMovementVelocity.normalized;
            _model.transform.forward = direction;
        }
    }

    protected sealed override void HandleDeath(Entity deadEntity)
    {
        base.HandleDeath(deadEntity);
        transform.Rotate(90, 0, 0);
    }

#if UNITY_EDITOR
    public override void Resurrect()
    {
        transform.Rotate(-90, 0, 0);
        base.Resurrect();
    }
#endif

#if UNITY_EDITOR
    protected sealed override void OnDrawGizmos() => base.OnDrawGizmos();

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
