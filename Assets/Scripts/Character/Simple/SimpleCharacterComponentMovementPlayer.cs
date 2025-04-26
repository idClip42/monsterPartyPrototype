using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace MonsterParty
{
    [RequireComponent(typeof(CharacterController))]
    public class SimpleCharacterComponentMovementPlayer : CharacterComponentMovementPlayer
    {
        [SerializeField]
        [Range(0, 1)]
        private float _runSpeedPercentage = 1;

        [SerializeField]
        [Range(0, 1)]
        private float _walkSpeedPercentage = 0.6f;

        private CharacterController? _characterController = null;

        public sealed override Vector3 CurrentVelocity => (_characterController && _characterController.enabled) ?
            _characterController.velocity :
            Vector3.zero;

        public bool IsRunning
        {
            get
            {
                if (this.enabled == false)
                    return false;

                if (this.Character != null)
                {
                    if (this.Character.GetCrouchComponent().IsCrouching == true)
                        return false;
                }

                bool runButtonDown = Input.GetButton("Run");
                return runButtonDown;
            }
        }

        public override void FillInDebugInfo(Dictionary<string, string> infoTarget)
        {
            base.FillInDebugInfo(infoTarget);
            infoTarget["IsRunning"] = IsRunning.ToString();
        }

        protected sealed override void Awake()
        {
            base.Awake();

            _characterController = GetComponent<CharacterController>();
            if (_characterController == null)
                throw new MonsterPartyNullReferenceException(this, nameof(_characterController));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_characterController == null) return;
            _characterController.enabled = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_characterController == null) return;
            _characterController.enabled = false;
        }

        protected sealed override void Move(Vector3 desiredMovementVelocity, float deltaTime)
        {
            if (this.Character == null)
                throw new MonsterPartyNullReferenceException(this, nameof(Character));
            if(this.Character.Alive == false)
                throw new MonsterPartyException("Move shouldn't be called on dead character.");
            if (_characterController == null)
                throw new MonsterPartyNullReferenceException(this, nameof(_characterController));

            _characterController.Move(
                desiredMovementVelocity * deltaTime +
                (Physics.gravity * deltaTime)
            );
        }

        public sealed override float GetDesiredSpeed()
        {
            if (this.Character == null)
                throw new MonsterPartyNullReferenceException(this, nameof(Character));
            if (this.IsRunning)
                return this.BaseMaxSpeed * _runSpeedPercentage;
            else
                return this.BaseMaxSpeed * _walkSpeedPercentage;
        }
    }
}