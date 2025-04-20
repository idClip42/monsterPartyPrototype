using System.Collections.Generic;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterComponentMovementPlayer : CharacterComponentMovementPlayer
{
    [SerializeField]
    [Range(0,1)]
    private float _runSpeedPercentage = 1;

    [SerializeField]
    [Range(0,1)]
    private float _walkSpeedPercentage = 0.6f;

    private CharacterController? _characterController = null;

    public sealed override Vector3 CurrentVelocity => _characterController ? 
        _characterController.velocity : 
        Vector3.zero;

    public bool IsRunning { get {
        if(this.enabled == false) 
            return false;

        if(this.Character != null && this.Character.Crouch != null){
            if(this.Character.Crouch.IsCrouching == true)
                return false;
        }

        bool runButtonDown = Input.GetButton("Run");
        return runButtonDown;
    }}

    public override void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        base.FillInDebugInfo(infoTarget);
        infoTarget["IsRunning"] = IsRunning.ToString();
    }

    protected sealed override void Awake()
    {
        base.Awake();

        _characterController = GetComponent<CharacterController>();
        if(_characterController == null)
            throw new System.Exception($"Null character controller on {this.gameObject.name}");
    }

    private void OnEnable()
    {
        if(_characterController == null) return;
        _characterController.enabled = true;
    }

    private void OnDisable()
    {
        if(_characterController == null) return;
        _characterController.enabled = false;
    }

    protected sealed override void Move(Vector3 desiredMovementVelocity, float deltaTime)
    {
        if(_characterController == null) throw new System.Exception("Null _characterController");
        _characterController.Move(
            desiredMovementVelocity * deltaTime +
            (Physics.gravity * deltaTime)
        );
    }

    public sealed override float GetDesiredSpeed()
    {
        if(this.Character == null)
            throw new System.Exception();
        if(this.IsRunning)
            return this.BaseMaxSpeed * _runSpeedPercentage;
        else
            return this.BaseMaxSpeed * _walkSpeedPercentage;
    }
}
