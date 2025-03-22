using UnityEngine;
using UnityEngine.AI;

# nullable enable

public class SimpleMonsterStateChase : SimpleMonsterState
{
    [System.Serializable]
    public class Config {
        [SerializeField]
        [Range(1,6)]
        public float speed = 4;

        [SerializeField]
        [Range(1,20)]
        public float acceleration = 8;
    }

    private Config _config;
    private Character? _targetCharacter = null;

    public override string DebugInfo => $"Chase: {_targetCharacter?.gameObject.name}";

    public SimpleMonsterStateChase(Config config){
        this._config = config;
    }

    public override void Start(NavMeshAgent agent) 
    {
        agent.speed = _config.speed;
        agent.acceleration = _config.acceleration;
    }

    public override void Stop(NavMeshAgent agent) 
    { 
        _targetCharacter = null; 
    }

    public override SimpleMonster.State OnUpdate(float deltaTime, Knowledge currentKnowledge, NavMeshAgent agent)
    {
        _targetCharacter = currentKnowledge.visibleTarget;
        if(_targetCharacter == null){
            return SimpleMonster.State.Search;
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