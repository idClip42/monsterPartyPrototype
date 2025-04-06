using UnityEngine;

#nullable enable

public interface ICarryable
{
    GameObject gameObject { get; }

    Transform CarryHandle { get; }

    bool IsCarryable { get; }

    void OnPickUp(CharacterComponentCarry pickerUpper);

    void OnDrop(CharacterComponentCarry pickerUpper);

    void LockInPlace(Transform targetParent);
}
