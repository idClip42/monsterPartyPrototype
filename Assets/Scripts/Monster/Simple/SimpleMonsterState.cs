using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

# nullable enable

namespace MonsterParty
{
    public abstract class SimpleMonsterState
    {
        public struct Knowledge
        {
            public Character? visibleTarget;
            public Vector3? lastSeenPosition;
            public Vector3? lastSeenVelocity;
            public override string ToString()
            {
                return $"Knowledge: Target '{visibleTarget?.gameObject.name}', Pos {lastSeenPosition}, Vel {lastSeenVelocity}.";
            }
        }

        public abstract void FillInDebugInfo(Dictionary<string, string> infoTarget);
        public abstract SimpleMonster.State NextState { get; }
        public abstract float ProgressToNextState { get; }
        public abstract bool AllowInterruption { get; }

        public abstract void Start(NavMeshAgent agent, Knowledge currentKnowledge);
        public abstract void Stop(NavMeshAgent agent);
        public abstract SimpleMonster.State OnUpdate(float deltaTime, Knowledge currentKnowledge, NavMeshAgent agent);

        public sealed override bool Equals(object other) => base.Equals(other);
        public sealed override int GetHashCode() => base.GetHashCode();
        public sealed override string ToString() => base.ToString();
    }
}