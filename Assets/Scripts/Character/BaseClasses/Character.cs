using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

#nullable enable

namespace MonsterParty
{
    [RequireComponent(typeof(CharacterComponentMovementPlayer))]
    [RequireComponent(typeof(CharacterComponentMovementAI))]
    [RequireComponent(typeof(CharacterComponentCrouch))]
    [RequireComponent(typeof(CharacterComponentInteract))]
    [RequireComponent(typeof(CharacterComponentNoiseLevel))]
    [DisallowMultipleComponent]
    public abstract class Character : Entity, IDebugInfoProvider
    {
        public enum StateType { Player, AI };

        [SerializeField]
        private AudioSource? _deathScream = null;

        private Transform[] _lookRaycastTargets = { };

        public abstract CharacterComponentCrouch GetCrouchComponent();
        public abstract CharacterComponentInteract GetInteractComponent();
        public abstract CharacterComponentMovementPlayer GetPlayerMovementComponent();
        public abstract CharacterComponentMovementAI GetAiMovementComponent();
        public abstract CharacterComponentNoiseLevel GetNoiseLevelComponent();
        public abstract CharacterComponentCarry GetCarryComponent();

        public IReadOnlyCollection<Transform> LookRaycastTargets => _lookRaycastTargets;

        public string DebugHeader => "Character";

        public virtual void FillInDebugInfo(Dictionary<string, string> infoTarget)
        {
            infoTarget["State"] = this._state.ToString();
            infoTarget["Raycast Targets"] = _lookRaycastTargets.Length.ToString();
        }

        private StateType _state = StateType.AI;
        public StateType State => _state;

        public void SetState(StateType newState)
        {
            _state = newState;

            Debug.Log($"Character '{gameObject.name}' state set to '{newState}'.");

            if (this.Alive == false) return;

            switch (_state)
            {
                case StateType.Player:
                    GetPlayerMovementComponent().enabled = true;
                    GetAiMovementComponent().enabled = false;
                    break;
                case StateType.AI:
                    GetPlayerMovementComponent().enabled = false;
                    GetAiMovementComponent().enabled = true;
                    break;
                default:
                    throw new MonsterPartyUnhandledEnumException<StateType>(_state);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _lookRaycastTargets = GetComponentsInChildren<CharacterLookRaycastTarget>(false)
                .Select(item => item.gameObject.transform).ToArray();

            this.OnDeath += HandleDeath;
        }

        protected virtual void HandleDeath(Entity deadEntity)
        {
            if (deadEntity != this)
                throw new MonsterPartyException("This function should only be called for own death.");

            GetPlayerMovementComponent().enabled = false;
            GetAiMovementComponent().enabled = false;

            if (_deathScream != null)
                _deathScream.Play();
            else
                Debug.LogWarning("Missing death scream");

            Debug.Log($"Character '{gameObject.name}' is dead.");
        }

        public CharacterComponentMovement GetCurrentMovementComponent()
        {
            switch (_state)
            {
                case StateType.Player:
                    return GetPlayerMovementComponent();
                case StateType.AI:
                    return GetAiMovementComponent();
                default:
                    throw new MonsterPartyUnhandledEnumException<StateType>(_state);
            }
        }

#if UNITY_EDITOR
        public override void Resurrect()
        {
            base.Resurrect();
            SetState(_state);
            Debug.Log($"Character '{gameObject.name}' is resurrected.");
        }
#endif
    }
}