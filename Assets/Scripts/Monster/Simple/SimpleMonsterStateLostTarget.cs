using UnityEngine;
using UnityEngine.AI;

# nullable enable

public class SimpleMonsterStateLostTarget : SimpleMonsterState
{
    [System.Serializable]
    public class Config {
        [SerializeField]
        [Range(5, 20)]
        public float minWaitAfterLostTime = 5;
        [SerializeField]
        [Range(5, 20)]
        public float maxWaitAfterLostTime = 10;
        [SerializeField]
        [Range(0, 1)]
        public float minWaitAfterLostTimeDist = 0.1f;
    }

    private Config _config;
    private float _waitAfterLostTimer = 0f;

    public override string DebugInfo => $"LostTarget: {_waitAfterLostTimer}s";

    public SimpleMonsterStateLostTarget(Config config){
        this._config = config;
    }

    public override void Start(NavMeshAgent agent)
    {
        _waitAfterLostTimer = Random.Range(_config.minWaitAfterLostTime, _config.maxWaitAfterLostTime);
    }

    public override void Stop(NavMeshAgent agent)
    {
        
    }

    public override SimpleMonster.State OnUpdate(float deltaTime, Knowledge currentKnowledge, NavMeshAgent agent)
    {
        if(currentKnowledge.visibleTarget != null){
            return SimpleMonster.State.Chase;
        }

        if(agent.remainingDistance < _config.minWaitAfterLostTimeDist){
            _waitAfterLostTimer -= deltaTime;
            if(_waitAfterLostTimer <= 0){
                return SimpleMonster.State.Wander;
            }
        }

        return SimpleMonster.State.LostTarget;
    }
}