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

        [SerializeField]
        [Range(0,2)]
        public float searchDelay = 1;
    }

    private Config _config;
    private Character? _targetCharacter = null;
    private float _searchDelayTimer = 0f;

    public override SimpleMonster.State NextState { get {
        if(_targetCharacter == null)
            return SimpleMonster.State.Search;
        return SimpleMonster.State.Chase;
    }}

    public override float ProgressToNextState { get {
        return (_config.searchDelay - _searchDelayTimer) / _config.searchDelay;
    }}

    public override string DebugInfo => $"Chase: {_targetCharacter?.gameObject.name} (Search: {_searchDelayTimer:F2}s)";

    public SimpleMonsterStateChase(Config config){
        this._config = config;
    }

    public override void Start(NavMeshAgent agent) 
    {
        agent.speed = _config.speed;
        agent.acceleration = _config.acceleration;
        _searchDelayTimer = _config.searchDelay;
    }

    public override void Stop(NavMeshAgent agent) 
    { 
        _targetCharacter = null; 
    }

    public override SimpleMonster.State OnUpdate(float deltaTime, Knowledge currentKnowledge, NavMeshAgent agent)
    {
        if(currentKnowledge.visibleTarget == null){
            // If we have no line of sight to a target,
            // we start counting down our timer
            _searchDelayTimer -= deltaTime;
            if(_searchDelayTimer <= 0){
                // And when it hits zero,
                // we stop chasing.
                return SimpleMonster.State.Search;
            }
        }
        else {
            // Always reset the search delay timer
            // if we have a line of sight to a target
            _searchDelayTimer = _config.searchDelay;
        }

        _targetCharacter = currentKnowledge.visibleTarget;

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