using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

# nullable enable

[RequireComponent(typeof(NavMeshAgent))]
[DisallowMultipleComponent]
public class SimpleMonster : Entity, IDebugInfoProvider
{
    public enum State { Wander, Chase, Search }

    [SerializeField]
    private SimpleMonsterHead.Config _headConfig;

    [SerializeField]
    private SimpleMonsterStateWander.Config? _wanderConfig;

    [SerializeField]
    private SimpleMonsterStateChase.Config? _chaseConfig;

    [SerializeField]
    private SimpleMonsterStateSearch.Config? _searchConfig;

    [SerializeField]
    [Range(0.5f, 3)]
    private float _killRadius = 1;

    private NavigationManager? _navManager = null;
    private NavMeshAgent? _navMeshAgent = null;

    private State _state = State.Wander;

    private SimpleMonsterHead? _headBehavior = null;
    private SimpleMonsterStateWander? _wanderBehavior = null;
    private SimpleMonsterStateChase? _chaseBehavior = null;
    private SimpleMonsterStateSearch? _searchBehavior = null;

    public State CurrentState => this._state;
    public string DebugName => "Simple Monster";

    public string DebugInfo
    {
        get
        {
            string speedInfo = this._navMeshAgent ?
                $"{this._navMeshAgent.velocity.magnitude:F2} m/s." :
                "0 m/s.";
            switch (this._state)
            {
                case State.Wander:
                    if(_wanderBehavior == null)
                        return "Missing wander behavior";
                    return $"{speedInfo} {_wanderBehavior.DebugInfo}";
                case State.Chase:
                    if(_chaseBehavior == null)
                        return "Missing chase behavior";
                    return $"{speedInfo} {_chaseBehavior.DebugInfo}";
                case State.Search:
                    if(_searchBehavior == null)
                        return "Missing search behavior";
                    return $"{speedInfo} {_searchBehavior.DebugInfo}";
                default:
                    throw new System.Exception($"Unrecognized monster state: {this._state}");
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        var characters = FindObjectsByType<Character>(FindObjectsSortMode.None);

        if (_headConfig.head == null)
            throw new System.Exception("Missing head.");

        if (_headConfig.eye == null)
            throw new System.Exception("Missing eye.");
        _headConfig.eye.enabled = true;

        _navManager = FindFirstObjectByType<NavigationManager>();
        if (_navManager == null)
            throw new System.Exception($"Null _navManager on {this.gameObject.name}");

        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        _headBehavior = new SimpleMonsterHead(
            this,
            _headConfig, 
            FindObjectsByType<Character>(FindObjectsSortMode.None)
        );

        if(_wanderConfig == null)
            throw new System.Exception($"Missing wander config on {this.gameObject.name}");
        if(_chaseConfig == null)
            throw new System.Exception($"Missing chase config on {this.gameObject.name}");
        if(_searchConfig == null)
            throw new System.Exception($"Missing search config on {this.gameObject.name}");

        _wanderBehavior = new SimpleMonsterStateWander(_wanderConfig, _navManager);
        _chaseBehavior = new SimpleMonsterStateChase(_chaseConfig);
        _searchBehavior = new SimpleMonsterStateSearch(_searchConfig);
    }

    private void Start()
    {
        if(_wanderBehavior == null)
            throw new System.Exception($"Missing wander behavior on {this.gameObject.name}");
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");
        _wanderBehavior.Start(_navMeshAgent);
    }

    private void Update()
    {
        if(_headBehavior == null)
            throw new System.Exception($"Missing head behavior on {this.gameObject.name}");
        if(_wanderBehavior == null)
            throw new System.Exception($"Missing wander behavior on {this.gameObject.name}");
        if(_chaseBehavior == null)
            throw new System.Exception($"Missing chase behavior on {this.gameObject.name}");
        if(_searchBehavior == null)
            throw new System.Exception($"Missing search behavior on {this.gameObject.name}");
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        _headBehavior.OnUpdate(Time.deltaTime);
        var currentKnowledge = _headBehavior.CurrentKnowledge;

        State newState;
        switch (this._state)
        {
            case State.Wander:
                newState = _wanderBehavior.OnUpdate(Time.deltaTime, currentKnowledge, _navMeshAgent);
                break;
            case State.Chase:
                newState = _chaseBehavior.OnUpdate(Time.deltaTime, currentKnowledge, _navMeshAgent);
                break;
            case State.Search:
                newState = _searchBehavior.OnUpdate(Time.deltaTime, currentKnowledge, _navMeshAgent);
                break;
            default:
                throw new System.Exception($"Unrecognized monster state: {this._state}");
        }

        if(newState != this._state){
            switch (this._state)
            {
                case State.Wander:
                    _wanderBehavior.Stop(_navMeshAgent);
                    break;
                case State.Chase:
                    _chaseBehavior.Stop(_navMeshAgent);
                    break;
                case State.Search:
                    _searchBehavior.Stop(_navMeshAgent);
                    break;
                default:
                    throw new System.Exception($"Unrecognized monster state: {this._state}");
            }

            switch (newState)
            {
                case State.Wander:
                    _wanderBehavior.Start(_navMeshAgent);
                    break;
                case State.Chase:
                    _chaseBehavior.Start(_navMeshAgent);
                    break;
                case State.Search:
                    _searchBehavior.Start(_navMeshAgent);
                    break;
                default:
                    throw new System.Exception($"Unrecognized monster state: {this._state}");
            }

            this._state = newState;
        }

        if(this._state == State.Chase && currentKnowledge.visibleTarget){
            Vector3 targetPos = currentKnowledge.visibleTarget.transform.position;
            Vector3 myPos = transform.position;
            Vector3 posDiff = targetPos - myPos;
            float sqrDistance = posDiff.sqrMagnitude;
            float threshold = this._killRadius * this._killRadius;
            if(sqrDistance < threshold){
                currentKnowledge.visibleTarget.Kill();
            }
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos() {
        base.OnDrawGizmos();

        Color prevColor = Handles.color;
        Handles.color = Color.red;

        Handles.DrawWireDisc(
            transform.position,
            Vector3.up,
            _killRadius
        );
        
        Handles.color = prevColor;
    }
#endif
}
