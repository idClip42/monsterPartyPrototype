using UnityEngine;

#nullable enable

[RequireComponent(typeof(DoorOpener))]
public class SimpleCharacterComponentMovementAI : CharacterComponentMovementAI
{
    private CharacterController? _characterController = null;
    private DoorOpener? _doorOpener = null;

    protected sealed override void Awake()
    {
        base.Awake();

        _characterController = GetComponent<CharacterController>();

        _doorOpener = GetComponent<DoorOpener>();
        if(_doorOpener == null)
            throw new MonsterPartyException($"Missing DoorOpener on {this.gameObject.name}");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if(_doorOpener == null)
            throw new MonsterPartyException($"Missing DoorOpener on {this.gameObject.name}");
        _doorOpener.enabled = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if(_doorOpener == null)
            throw new MonsterPartyException($"Missing DoorOpener on {this.gameObject.name}");
        _doorOpener.enabled = false;
    }

    protected sealed override void Update()
    {
        base.Update();

        if(_characterController != null && _characterController.enabled == true)
            throw new MonsterPartyException("CharacterController is enabled while AI is going.");
    }
}
