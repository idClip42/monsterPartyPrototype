using UnityEngine;
using UnityEngine.AI;

# nullable enable

public class SimpleMonsterStateSearch : SimpleMonsterState
{
    [System.Serializable]
    public class Config {
        [SerializeField]
        [Range(1,6)]
        public float speed = 2;

        [SerializeField]
        [Range(1,20)]
        public float acceleration = 8;

        [SerializeField]
        [Range(5, 20)]
        public float minWaitAfterSearchTime = 5;

        [SerializeField]
        [Range(5, 20)]
        public float maxWaitAfterSearchTime = 10;

        [SerializeField]
        [Range(0, 1)]
        public float minWaitAfterSearchTimeDist = 0.1f;
    }

    private Config _config;
    private float _waitTime = 0f;
    private float _waitAfterSearchTimer = 0f;
    private Vector3? _targetPosition = null;

    public override SimpleMonster.State NextState { get {
        return SimpleMonster.State.Wander;
    }}

    public override float ProgressToNextState { get {
        return (_waitTime - _waitAfterSearchTimer) / _waitTime;
    }}

    public override bool AllowInterruption { get {
        // If the delay timer is going,
        // it means we're waiting around
        // and can be interrupted.
        // If it's not, we're still investigating
        // the last character we saw.
        return _waitAfterSearchTimer < _waitTime;
    }}

    public override string DebugInfo => $"Search: {_waitAfterSearchTimer}s";

    public SimpleMonsterStateSearch(Config config){
        this._config = config;
    }

    public override void Start(NavMeshAgent agent, Knowledge currentKnowledge)
    {
        agent.speed = _config.speed;
        agent.acceleration = _config.acceleration;
        _waitTime = Random.Range(_config.minWaitAfterSearchTime, _config.maxWaitAfterSearchTime);
        _waitAfterSearchTimer = _waitTime;

        _targetPosition = currentKnowledge.lastSeenPosition;
        if(_targetPosition == null)
            Debug.LogWarning("Search behavior started with null target position");
        else
            agent.SetDestination(_targetPosition.Value);
    }

    public override void Stop(NavMeshAgent agent)
    {
        
    }

    public override SimpleMonster.State OnUpdate(float deltaTime, Knowledge currentKnowledge, NavMeshAgent agent)
    {
        if(currentKnowledge.visibleTarget != null){
            return SimpleMonster.State.Chase;
        }

        if(currentKnowledge.lastSeenPosition != null){
            bool hasNewTargetPosition = currentKnowledge.lastSeenPosition != _targetPosition;
            if(hasNewTargetPosition){
                _targetPosition = currentKnowledge.lastSeenPosition;
                agent.SetDestination(_targetPosition.Value);
            }
        }

        if(agent.remainingDistance < _config.minWaitAfterSearchTimeDist){
            _waitAfterSearchTimer -= deltaTime;
            if(_waitAfterSearchTimer <= 0){
                return SimpleMonster.State.Wander;
            }
        }
        else {
            // Resetting the timer as long as we have distance to travel
            _waitAfterSearchTimer = _waitTime;
        }

        return SimpleMonster.State.Search;
    }
}