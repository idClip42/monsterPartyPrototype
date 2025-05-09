using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace MonsterParty
{
    public abstract class CharacterComponentMovement : CharacterComponent
    {
        [SerializeField]
        [Range(1, 10)]
        private float _maxSpeed = 5.0f;

        private ISpeedLimiter[] _speedLimiters = { };

        public abstract Vector3 CurrentVelocity { get; }

        protected float BaseMaxSpeed => _maxSpeed;

        public override void FillInDebugInfo(Dictionary<string, string> infoTarget)
        {
            infoTarget["Speed Limiters"] = _speedLimiters.Length.ToString();
            infoTarget["Max Speed"] = GetMaxMoveSpeed().ToString();
        }

        protected override void Awake()
        {
            base.Awake();
            _speedLimiters = GetComponents<ISpeedLimiter>();
        }

        public float GetMaxMoveSpeed()
        {
            if (this.Character == null)
                throw new MonsterPartyNullReferenceException(this, nameof(Character));

            float maxSpeedMultiplier = _speedLimiters.Aggregate(
                1.0f,
                (acc, test) =>
                {
                    if (test.IsLimitingMaxSpeed == false)
                        return acc;
                    if (test.MaxSpeedPercentageLimit >= acc)
                        return acc;
                    return test.MaxSpeedPercentageLimit;
                }
            );

            return _maxSpeed * maxSpeedMultiplier;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            using (new Handles.DrawingScope(Color.white))
            {
                if (CurrentVelocity != Vector3.zero)
                {
                    Handles.DrawLine(
                        transform.position,
                        transform.position + CurrentVelocity
                    );
                }
            }
        }
#endif
    }
}