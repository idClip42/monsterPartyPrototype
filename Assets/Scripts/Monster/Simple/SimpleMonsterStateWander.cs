using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

# nullable enable

public class SimpleMonsterStateWander : SimpleMonsterState
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
        [Range(5, 100)]
        public float minRedirectTime = 20;

        [SerializeField]
        [Range(5, 100)]
        public float maxRedirectTime = 60;

        [SerializeField]
        [Range(0, 3)]
        public float chaseDelay = 1;

        [SerializeField]
        [Range(0, 10)]
        public float destinationMinWallDist = 5;
    }
    
    private NavigationManager _navManager;
    private Config _config;
    private float _newDestinationTimer = 0f;
    private float _chaseDelayTimer = 0f;
    private Character? _previousFrameTarget = null;

    public sealed override SimpleMonster.State NextState { get{
        if(_chaseDelayTimer == _config.chaseDelay)
            return SimpleMonster.State.Wander;
        return SimpleMonster.State.Chase;
    }}

    public sealed override float ProgressToNextState { get {
        return (_config.chaseDelay - _chaseDelayTimer) / _config.chaseDelay;
    }}

    public sealed override bool AllowInterruption => true;

    public sealed override void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        infoTarget["Redirect Timer"] = $"{_newDestinationTimer:F2}s";
        infoTarget["Chase Timer"] = $"{_chaseDelayTimer:F2}s";
    }

    public SimpleMonsterStateWander(Config config, NavigationManager navManager){
        this._config = config;
        this._navManager = navManager;

        if (_config.minRedirectTime > _config.maxRedirectTime)
            throw new System.Exception("Invalid redirect times");
    }

    public sealed override void Start(NavMeshAgent agent, Knowledge currentKnowledge)
    {
        agent.speed = _config.speed;
        agent.acceleration = _config.acceleration;
        Redirect(agent);
    }

    public sealed override void Stop(NavMeshAgent agent)
    {
        _previousFrameTarget = null;
    }

    public sealed override SimpleMonster.State OnUpdate(float deltaTime, Knowledge currentKnowledge, NavMeshAgent agent)
    {
        // Countdown timer logic to choose a new destination
        _newDestinationTimer -= Time.deltaTime;
        if (_newDestinationTimer <= 0f)
            Redirect(agent);

        // If we currently see someone
        if(currentKnowledge.visibleTarget != null){
            // If this is the first frame we've seen them...
            bool wasNull = this._previousFrameTarget == null;
            if(wasNull){
                // ...then start the chase timer.
                this._chaseDelayTimer = this._config.chaseDelay;
            }
            // If this isn't the first frame we've seen them...
            else {
                // Decrement the timer until it's time to chase them.
                this._chaseDelayTimer -= deltaTime;
                if(this._chaseDelayTimer <= 0){
                    // Switch the state to chase.
                    return SimpleMonster.State.Chase;
                }
            }
        }
        else {
            this._chaseDelayTimer = this._config.chaseDelay;
        }
        _previousFrameTarget = currentKnowledge.visibleTarget;

        // Remain in the wander state
        return SimpleMonster.State.Wander;
    }

    private void Redirect(NavMeshAgent agent){
        _newDestinationTimer = Random.Range(
            this._config.minRedirectTime, 
            this._config.maxRedirectTime
        );
        agent.SetDestination(
            this._navManager.GetRandomDestination(false, true, _config.destinationMinWallDist)
        );
    }
}