using UnityEngine;

#nullable enable

namespace MonsterParty
{
    public interface ICarryable
    {
        GameObject gameObject { get; }

        Transform GetCarryHandle();

        CharacterComponentCarry? Carrier { get; }

        float Mass { get; }

        bool IsCarryable { get; }

        void OnPickUp(CharacterComponentCarry pickerUpper);

        void OnDrop(CharacterComponentCarry pickerUpper);

        void LockInPlace(Transform targetParent);
    }
}