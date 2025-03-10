using UnityEngine;

#nullable enable

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SimpleCharacterCrouch))]
public class SimpleCharacterMovementPlayer : CharacterMovementPlayer
{
    private CharacterController? _characterController = null;

    protected override void Awake()
    {
        base.Awake();

        _characterController = GetComponent<CharacterController>();
        if(_characterController == null)
            throw new System.Exception($"Null character controller on {this.gameObject.name}");
    }

    void OnEnable()
    {
        if(_characterController == null) return;
        _characterController.enabled = true;
    }

    void OnDisable()
    {
        if(_characterController == null) return;
        _characterController.enabled = false;
    }

    protected override void Move(Vector3 desiredMovementVelocity, float deltaTime)
    {
        if(_characterController == null) throw new System.Exception("Null _characterController");
        _characterController.Move(
            desiredMovementVelocity * deltaTime +
            (Physics.gravity * deltaTime)
        );
    }
}
