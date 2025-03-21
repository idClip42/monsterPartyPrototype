using UnityEngine;
using UnityEngine.AI;

# nullable enable

public class SimpleMonsterStateChase : SimpleMonsterState
{
    private Character? _targetCharacter = null;

    public override string DebugInfo => $"Chase: {_targetCharacter?.gameObject.name}";

    public override void Start(NavMeshAgent agent) { }

    public override void Stop(NavMeshAgent agent) 
    { 
        _targetCharacter = null; 
    }

    public override SimpleMonster.State OnUpdate(float deltaTime, Knowledge currentKnowledge, NavMeshAgent agent)
    {
        _targetCharacter = currentKnowledge.visibleTarget;
        if(_targetCharacter == null){
            return SimpleMonster.State.LostTarget;
        }

        if(currentKnowledge.lastSeenPosition == null){
            Debug.LogWarning("We shouldn't be missing a last seen position. Canceling Chase.");
            return SimpleMonster.State.Wander;
        }

        agent.SetDestination(
            currentKnowledge.lastSeenPosition.Value
        );
        return SimpleMonster.State.Chase;
    }
}