using UnityEngine;

#nullable enable

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterComponentMovementPlayer : CharacterComponentMovementPlayer
{
    private CharacterController? _characterController = null;

    public sealed override Vector3 CurrentVelocity => _characterController ? 
        _characterController.velocity : 
        Vector3.zero;

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
}
