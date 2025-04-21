using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

# nullable enable

namespace MonsterParty
{
    public class SimpleMonsterStateChase : SimpleMonsterState
    {
        [System.Serializable]
        public class Config
        {
            [SerializeField]
            [Range(1, 6)]
            public float speed = 4;

            [SerializeField]
            [Range(1, 20)]
            public float acceleration = 8;

            [SerializeField]
            [Range(0, 2)]
            public float searchDelay = 1;

            [System.Serializable]
            public enum ChaseTargetMode { ActualPosition, LastSeenPosition };

            [SerializeField]
            public ChaseTargetMode chaseTargetMode = ChaseTargetMode.ActualPosition;
        }

        private Config _config;
        private Character? _targetCharacter = null;
        private float _searchDelayTimer = 0f;

        public sealed override SimpleMonster.State NextState
        {
            get
            {
                if (_targetCharacter == null)
                    return SimpleMonster.State.Search;
                return SimpleMonster.State.Chase;
            }
        }

        public sealed override float ProgressToNextState
        {
            get
            {
                return (_config.searchDelay - _searchDelayTimer) / _config.searchDelay;
            }
        }

        public sealed override bool AllowInterruption => false;

        public sealed override void FillInDebugInfo(Dictionary<string, string> infoTarget)
        {
            infoTarget["Target"] = _targetCharacter ? _targetCharacter.gameObject.name : "None";
            infoTarget["Timer"] = $"{_searchDelayTimer:F2}s";
        }

        public SimpleMonsterStateChase(Config config)
        {
            this._config = config;
        }

        public sealed override void Start(NavMeshAgent agent, Knowledge currentKnowledge)
        {
            agent.speed = _config.speed;
            agent.acceleration = _config.acceleration;
            _searchDelayTimer = _config.searchDelay;
        }

        public sealed override void Stop(NavMeshAgent agent)
        {
            _targetCharacter = null;
        }

        public sealed override SimpleMonster.State OnUpdate(float deltaTime, Knowledge currentKnowledge, NavMeshAgent agent)
        {
            if (currentKnowledge.visibleTarget == null)
            {
                // If we have no line of sight to a target,
                // we start counting down our timer
                _searchDelayTimer -= deltaTime;
                if (_searchDelayTimer <= 0)
                {
                    // And when it hits zero,
                    // we stop chasing.
                    return SimpleMonster.State.Search;
                }
            }
            else
            {
                // Always reset the search delay timer
                // if we have a line of sight to a target
                _searchDelayTimer = _config.searchDelay;

                // Store our most-recently seen target
                _targetCharacter = currentKnowledge.visibleTarget;
            }

            Vector3 chaseTargetPosition;
            if (_config.chaseTargetMode == Config.ChaseTargetMode.ActualPosition)
            {
                if (_targetCharacter == null)
                {
                    Debug.LogWarning("We shouldn't be missing a target character. Canceling Chase.");
                    return SimpleMonster.State.Wander;
                }
                chaseTargetPosition = _targetCharacter.transform.position;
            }
            else if (_config.chaseTargetMode == Config.ChaseTargetMode.LastSeenPosition)
            {
                if (currentKnowledge.lastSeenPosition == null)
                {
                    Debug.LogWarning("We shouldn't be missing a last seen position. Canceling Chase.");
                    return SimpleMonster.State.Wander;
                }
                chaseTargetPosition = currentKnowledge.lastSeenPosition.Value;
            }
            else
            {
                throw new MonsterPartyUnhandledEnumException<Config.ChaseTargetMode>(_config.chaseTargetMode);
            }
            agent.SetDestination(chaseTargetPosition);
            return SimpleMonster.State.Chase;
        }
    }
}