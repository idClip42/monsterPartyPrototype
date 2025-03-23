using UnityEngine;
using UnityEngine.AI;

# nullable enable

public abstract class SimpleMonsterState{
    public struct Knowledge {
        public Character? visibleTarget;
        public Vector3? lastSeenPosition;
        public Vector3? lastSeenVelocity;
    }

    public abstract string DebugInfo { get; }
    public abstract SimpleMonster.State NextState { get; }
    public abstract float ProgressToNextState { get; }
    public abstract bool AllowInterruption { get; }

    public abstract void Start(NavMeshAgent agent, Knowledge currentKnowledge);
    public abstract void Stop(NavMeshAgent agent);
    public abstract SimpleMonster.State OnUpdate(float deltaTime, Knowledge currentKnowledge, NavMeshAgent agent);
}