using UnityEngine;

#nullable enable

public class SimpleCharacterComponentMovementAI : CharacterComponentMovementAI
{
    private CharacterController? _characterController = null;

    protected sealed override void Awake()
    {
        base.Awake();
        _characterController = GetComponent<CharacterController>();
    }

    protected sealed override void Update()
    {
        base.Update();

        if(_characterController != null && _characterController.enabled == true)
            throw new System.Exception("CharacterController is enabled while AI is going.");
    }
}
