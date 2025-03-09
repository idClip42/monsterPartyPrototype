using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SimpleCharacterCrouch))]
public class SimpleCharacterMovementPlayer : CharacterMovementPlayer
{
    private CharacterController _characterController;

    protected override void Awake()
    {
        base.Awake();

        _characterController = GetComponent<CharacterController>();
        if(_characterController == null)
            throw new System.Exception($"Null character controller on {this.gameObject.name}");
    }

    void OnEnable()
    {
        _characterController.enabled = true;
    }

    void OnDisable()
    {
        _characterController.enabled = false;
    }

    protected override void Move(Vector3 desiredMovementVelocity, float deltaTime)
    {
        _characterController.Move(
            desiredMovementVelocity * deltaTime +
            (Physics.gravity * deltaTime)
        );
    }
}
