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
    private SimpleMonsterHead.Config? _headConfig;

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
    private SimpleMonsterHearing.SoundInfo[] _currentSoundInfo = {};

    private SimpleMonsterHead? _headBehavior = null;
    private SimpleMonsterStateWander? _wanderBehavior = null;
    private SimpleMonsterStateChase? _chaseBehavior = null;
    private SimpleMonsterStateSearch? _searchBehavior = null;
    private SimpleMonsterHearing? _hearing = null;

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

        if(_headConfig == null)
            throw new System.Exception($"Missing head config on {this.gameObject.name}");
        if (_headConfig.head == null)
            throw new System.Exception("Missing head.");
        if (_headConfig.eye == null)
            throw new System.Exception("Missing eye.");

        var charactersArray = FindObjectsByType<Character>(FindObjectsSortMode.None);
        if(charactersArray.Length == 0){
            Debug.LogWarning($"{this.gameObject.name} found no Characters in the scene.");
        }

        _headBehavior = new SimpleMonsterHead(
            this,
            _headConfig, 
            charactersArray
        );

        _navManager = FindFirstObjectByType<NavigationManager>();
        if (_navManager == null)
            throw new System.Exception($"Null _navManager on {this.gameObject.name}");
        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        if(_wanderConfig == null)
            throw new System.Exception($"Missing wander config on {this.gameObject.name}");
        if(_chaseConfig == null)
            throw new System.Exception($"Missing chase config on {this.gameObject.name}");
        if(_searchConfig == null)
            throw new System.Exception($"Missing search config on {this.gameObject.name}");

        _wanderBehavior = new SimpleMonsterStateWander(_wanderConfig, _navManager);
        _chaseBehavior = new SimpleMonsterStateChase(_chaseConfig);
        _searchBehavior = new SimpleMonsterStateSearch(_searchConfig);
        _hearing = new SimpleMonsterHearing(this, charactersArray, _navManager);
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
        if(_hearing == null)
            throw new System.Exception($"Missing hearing on {this.gameObject.name}");
        if(_wanderBehavior == null)
            throw new System.Exception($"Missing wander behavior on {this.gameObject.name}");
        if(_chaseBehavior == null)
            throw new System.Exception($"Missing chase behavior on {this.gameObject.name}");
        if(_searchBehavior == null)
            throw new System.Exception($"Missing search behavior on {this.gameObject.name}");
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        this._currentSoundInfo = _hearing.CheckForSounds();

        SimpleMonsterState currentBehavior = this._state switch {
            State.Wander => _wanderBehavior,
            State.Chase => _chaseBehavior,
            State.Search => _searchBehavior,
            _ => throw new System.Exception($"Unrecognzied monster state '{this._state}'")
        };

        _headBehavior.OnUpdate(Time.deltaTime, currentBehavior);
        var currentKnowledge = _headBehavior.CurrentKnowledge;
        State newState = currentBehavior.OnUpdate(Time.deltaTime, currentKnowledge, _navMeshAgent);

        if(newState != this._state){
            currentBehavior.Stop(_navMeshAgent);
            SimpleMonsterState nextBehavior = newState switch {
                State.Wander => _wanderBehavior,
                State.Chase => _chaseBehavior,
                State.Search => _searchBehavior,
                _ => throw new System.Exception($"Unrecognzied monster state '{newState}'")
            };
            nextBehavior.Start(_navMeshAgent);

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
        if(_headConfig == null)
            throw new System.Exception($"Missing head config on {this.gameObject.name}");
        if(_headConfig.eye == null)
            throw new System.Exception($"Missing eye on {this.gameObject.name}");

        base.OnDrawGizmos();

        Color prevColor = Handles.color;
        // Handles.color = Color.red;
        Handles.color = _headConfig.eye.color;

        Handles.DrawWireDisc(
            transform.position,
            Vector3.up,
            _killRadius
        );

        if(_headConfig.eye != null && _headBehavior != null){
            float[] sightDebugHeights = new float[]{ 
                transform.position.y, 
                // _headConfig.eye.transform.position.y
            };
            foreach(float height in sightDebugHeights){
                Vector3 origin = new Vector3(_headConfig.eye.transform.position.x, height, _headConfig.eye.transform.position.z);
                Vector3 projectedForward = Vector3.ProjectOnPlane(_headConfig.eye.transform.forward, Vector3.up).normalized;
                Vector3 leftEdgeDirection = Quaternion.Euler(0, -_headBehavior.FieldOfView/2, 0) * projectedForward;
                Vector3 rightEdgeDirection = Quaternion.Euler(0, _headBehavior.FieldOfView/2, 0) * projectedForward;
                Handles.DrawWireArc(
                    origin,
                    Vector3.up,
                    leftEdgeDirection,
                    _headBehavior.FieldOfView,
                    _headBehavior.MaxSightDistance
                );
                Handles.DrawLine(origin, origin + leftEdgeDirection * _headBehavior.MaxSightDistance);
                Handles.DrawLine(origin, origin + rightEdgeDirection * _headBehavior.MaxSightDistance);

                // Draw some extra lines for the hell of it.
                const int INTERVAL = 5;
                for(int i = INTERVAL; i < _headBehavior.MaxSightDistance; i += INTERVAL){
                    Handles.DrawWireArc(
                        origin,
                        Vector3.up,
                        leftEdgeDirection,
                        _headBehavior.FieldOfView,
                        i
                    );
                }
            }
        }

        // Draw sound paths for sounds
        foreach(var sound in _currentSoundInfo){
            Handles.color = sound.isAudible ? Color.yellow : Color.cyan;
            for (int i = 1; i < sound.pathToSound.Length; i++)
                Handles.DrawLine(sound.pathToSound[i-1], sound.pathToSound[i]);
        }
        
        Handles.color = prevColor;
    }
#endif
}
