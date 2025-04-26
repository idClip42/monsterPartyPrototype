using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace MonsterParty
{
    [DisallowMultipleComponent]
    public abstract class CharacterComponentCrouch : CharacterComponent, ISpeedLimiter
    {
        [SerializeField]
        [Range(0, 1)]
        private float _crouchSpeedPercentage = 0.4f;

        private bool _isCrouching = false;
        public bool IsCrouching => _isCrouching;
        private bool _canUncrouch = false;

        public sealed override string DebugHeader => "Crouch";

        public bool IsLimitingMaxSpeed => IsCrouching;

        public float MaxSpeedPercentageLimit => _crouchSpeedPercentage;

        public override void FillInDebugInfo(Dictionary<string, string> infoTarget)
        {
            infoTarget["State"] = _isCrouching ? "Crouching" : "Standing";
            if (_isCrouching)
                infoTarget["Can Stand"] = _canUncrouch.ToString();
        }

        public delegate void CrouchToggleHandler(bool isCrouching);
        public CrouchToggleHandler? OnCrouchToggle;

        protected override void Awake() => base.Awake();

        private void Update()
        {
            if (this.Character == null) throw new MonsterPartyNullReferenceException(this, nameof(this.Character));

            _canUncrouch = CanUncrouch();

            if (this.Character.State == Character.StateType.Player)
            {
                if (Input.GetButtonDown("Crouch"))
                {
                    ToggleCrouch();
                }
            }
        }

        public void SetCrouching(bool isCrouching)
        {
            if (isCrouching == this._isCrouching) return;
            ToggleCrouch();
        }

        private void ToggleCrouch()
        {
            if (this.Character == null) throw new MonsterPartyNullReferenceException(this, nameof(Character));

            if (this._isCrouching && !_canUncrouch)
                return;

            this._isCrouching = !this._isCrouching;
            if (this._isCrouching)
            {
                EnableCrouch();
                Debug.Log($"Character '{gameObject.name}' crouched. Max move speed is now {this.Character.GetCurrentMovementComponent().GetMaxMoveSpeed()}.");
            }
            else
            {
                DisableCrouch();
                Debug.Log($"Character '{gameObject.name}' uncrouched. Max move speed is now {this.Character.GetCurrentMovementComponent().GetMaxMoveSpeed()}.");
            }
            OnCrouchToggle?.Invoke(this._isCrouching);
        }

        protected abstract void EnableCrouch();
        protected abstract void DisableCrouch();

        protected abstract bool CanUncrouch();
    }
}