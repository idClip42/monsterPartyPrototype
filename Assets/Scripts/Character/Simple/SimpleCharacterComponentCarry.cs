using UnityEngine;

#nullable enable

namespace MonsterParty
{
    [RequireComponent(typeof(SimpleCharacter))]
    public class SimpleCharacterComponentCarry : CharacterComponentCarry
    {
        [SerializeField]
        private Transform? _carryParent;

        protected override Transform GetCarryParent()
        {
            if (_carryParent == null)
                throw new MonsterPartyNullReferenceException(this, nameof(_carryParent));
            return _carryParent;
        }

        protected override void Awake()
        {
            base.Awake();

            if (_carryParent == null)
                throw new MonsterPartyNullReferenceException(this, nameof(_carryParent));
        }
    }
}