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
            Debug.Log("Monster starting CHASE behavior.");

            agent.speed = _config.speed;
            agent.acceleration = _config.acceleration;
            _searchDelayTimer = _config.searchDelay;

            if(currentKnowledge.visibleTarget == null)
                throw new MonsterPartyException("Chase should not start with a null visibleTarget.");
            _targetCharacter = currentKnowledge.visibleTarget;
        }

        public sealed override void Stop(NavMeshAgent agent)
        {
            Debug.Log("Monster stopping CHASE behavior.");
            
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
                    Debug.Log("Monster has lost sight of target.");
                    return SimpleMonster.State.Search;
                }
                
                if(_targetCharacter == null){
                    throw new MonsterPartyException("Monster Chase first update has a null visibleTarget. This should never happen.");
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
                // if (_targetCharacter == null)
                // {
                //     if(currentKnowledge.lastSeenPosition != null){
                //         Debug.DrawLine(
                //             agent.transform.position,
                //             currentKnowledge.lastSeenPosition.Value,
                //             Color.red
                //         );
                //         if(currentKnowledge.lastSeenVelocity != null){
                //             Debug.DrawLine(
                //                 currentKnowledge.lastSeenPosition.Value,
                //                 currentKnowledge.lastSeenPosition.Value + currentKnowledge.lastSeenVelocity.Value,
                //                 Color.red
                //             );
                //         }
                //     }
                //     else {
                //         Debug.DrawLine(
                //             agent.transform.position,
                //             agent.transform.position + Vector3.up * 10,
                //             Color.red
                //         );
                //     }
                //     throw new MonsterPartyException($"We shouldn't be missing a target character. {currentKnowledge}");
                // }
                chaseTargetPosition = _targetCharacter.transform.position;
            }
            else if (_config.chaseTargetMode == Config.ChaseTargetMode.LastSeenPosition)
            {
                if (currentKnowledge.lastSeenPosition == null)
                {
                    throw new MonsterPartyException($"We shouldn't be missing a last seen position. {currentKnowledge}");
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