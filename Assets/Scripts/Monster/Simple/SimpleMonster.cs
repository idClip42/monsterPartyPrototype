using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

# nullable enable

[RequireComponent(typeof(NavMeshAgent))]
[DisallowMultipleComponent]
public class SimpleMonster : Entity, IDebugInfoProvider
{
    public enum State { Wander, Chase, LostTarget }

    [SerializeField]
    private SimpleMonsterHead.Config _headConfig;

    [SerializeField]
    private SimpleMonsterStateWander.Config? _wanderConfig;

    [SerializeField]
    private SimpleMonsterStateLostTarget.Config? _lostTargetConfig;

    [SerializeField]
    [Range(0.5f, 3)]
    private float _killRadius = 1;

    private NavigationManager? _navManager = null;
    private NavMeshAgent? _navMeshAgent = null;

    private State _state = State.Wander;

    private SimpleMonsterHead? _headBehavior = null;
    private SimpleMonsterStateWander? _wanderBehavior = null;
    private SimpleMonsterStateChase? _chaseBehavior = null;
    private SimpleMonsterStateLostTarget? _lostTargetBehavior = null;

    public string DebugName => "Simple Monster";

    public string DebugInfo
    {
        get
        {
            switch (this._state)
            {
                case State.Wander:
                    if(_wanderBehavior == null)
                        return "Missing wander behavior";
                    return _wanderBehavior.DebugInfo;
                case State.Chase:
                    if(_chaseBehavior == null)
                        return "Missing chase behavior";
                    return _chaseBehavior.DebugInfo;
                case State.LostTarget:
                    if(_lostTargetBehavior == null)
                        return "Missing lost target behavior";
                    return _lostTargetBehavior.DebugInfo;
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
            _headConfig, 
            FindObjectsByType<Character>(FindObjectsSortMode.None)
        );

        if(_wanderConfig == null)
            throw new System.Exception($"Missing wander config on {this.gameObject.name}");
        if(_lostTargetConfig == null)
            throw new System.Exception($"Missing lost target config on {this.gameObject.name}");

        _wanderBehavior = new SimpleMonsterStateWander(_wanderConfig, _navManager);
        _chaseBehavior = new SimpleMonsterStateChase();
        _lostTargetBehavior = new SimpleMonsterStateLostTarget(_lostTargetConfig);
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
        if(_lostTargetBehavior == null)
            throw new System.Exception($"Missing lost target behavior on {this.gameObject.name}");
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
            case State.LostTarget:
                newState = _lostTargetBehavior.OnUpdate(Time.deltaTime, currentKnowledge, _navMeshAgent);
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
                case State.LostTarget:
                    _lostTargetBehavior.Stop(_navMeshAgent);
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
                case State.LostTarget:
                    _lostTargetBehavior.Start(_navMeshAgent);
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
