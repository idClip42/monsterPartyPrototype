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

    public override SimpleMonster.State NextState { get {
        return SimpleMonster.State.Wander;
    }}

    public override float ProgressToNextState { get {
        return (_waitTime - _waitAfterSearchTimer) / _waitTime;
    }}

    public override string DebugInfo => $"Search: {_waitAfterSearchTimer}s";

    public SimpleMonsterStateSearch(Config config){
        this._config = config;
    }

    public override void Start(NavMeshAgent agent)
    {
        agent.speed = _config.speed;
        agent.acceleration = _config.acceleration;
        _waitTime = Random.Range(_config.minWaitAfterSearchTime, _config.maxWaitAfterSearchTime);
        _waitAfterSearchTimer = _waitTime;
    }

    public override void Stop(NavMeshAgent agent)
    {
        
    }

    public override SimpleMonster.State OnUpdate(float deltaTime, Knowledge currentKnowledge, NavMeshAgent agent)
    {
        if(currentKnowledge.visibleTarget != null){
            return SimpleMonster.State.Chase;
        }

        if(agent.remainingDistance < _config.minWaitAfterSearchTimeDist){
            _waitAfterSearchTimer -= deltaTime;
            if(_waitAfterSearchTimer <= 0){
                return SimpleMonster.State.Wander;
            }
        }

        return SimpleMonster.State.Search;
    }
}