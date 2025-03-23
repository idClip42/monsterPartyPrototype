using System.Collections.Generic;
using System.Linq;
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
    public string DebugHeader => "Simple Monster";

    public void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        if(_navMeshAgent != null){
            infoTarget["Speed"] = $"{this._navMeshAgent.velocity.magnitude:F2} m/s";
        }
        infoTarget["Nearby Sounds"] = _currentSoundInfo.Length.ToString();
        infoTarget["Audible Sounds"] = _currentSoundInfo.Count(s=>s.isAudible).ToString();
        infoTarget["State"] = this._state.ToString();

        SimpleMonsterState? currentBehavior = this._state switch{
            State.Wander => _wanderBehavior,
            State.Chase => _chaseBehavior,
            State.Search => _searchBehavior,
            _ => throw new System.Exception($"Unrecognized monster state: {this._state}")
        };
        if(currentBehavior != null)
            currentBehavior.FillInDebugInfo(infoTarget);
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
        if(_headBehavior == null)
            throw new System.Exception($"Missing head behavior on {this.gameObject.name}");
        _wanderBehavior.Start(_navMeshAgent, _headBehavior.CurrentKnowledge);
    }

    private void Update()
    {
        if(_headBehavior == null)
            throw new System.Exception($"Missing head behavior on {this.gameObject.name}");

        UpdateBehavior();

        var currentKnowledge = _headBehavior.CurrentKnowledge;
        if(currentKnowledge.visibleTarget != null)
            TryKill(currentKnowledge.visibleTarget);
    }

    private void UpdateBehavior(){
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

        SimpleMonsterState currentBehavior = this._state switch {
            State.Wander => _wanderBehavior,
            State.Chase => _chaseBehavior,
            State.Search => _searchBehavior,
            _ => throw new System.Exception($"Unrecognzied monster state '{this._state}'")
        };

        _headBehavior.OnUpdate(Time.deltaTime, currentBehavior);
        var currentKnowledge = _headBehavior.CurrentKnowledge;
        
        State newState = currentBehavior.OnUpdate(Time.deltaTime, currentKnowledge, _navMeshAgent);
        SimpleMonsterState nextBehavior = (newState != this._state) ?
            newState switch {
                State.Wander => _wanderBehavior,
                State.Chase => _chaseBehavior,
                State.Search => _searchBehavior,
                _ => throw new System.Exception($"Unrecognized monster state '{newState}'")
            } :
            currentBehavior;

        this._currentSoundInfo = _hearing.CheckForSounds();
        if(nextBehavior.AllowInterruption && this._currentSoundInfo.Any(s=>s.isAudible)){
            // Get the closest audible noise
            SimpleMonsterHearing.SoundInfo? targetSound = null;
            foreach(var sound in this._currentSoundInfo){
                if(targetSound == null)
                    targetSound = sound;
                if(sound.distanceToSound < targetSound.Value.distanceToSound)
                    targetSound = sound;
            }
            if(targetSound == null)
                throw new System.Exception("Target sound should not be null.");
            if(targetSound.Value.isAudible == false)
                throw new System.Exception("Target sound should be audible.");

            // Redirect next state
            newState = State.Search;
            nextBehavior = _searchBehavior;

            // Alert head behavior to sound
            _headBehavior.AttractAttention(targetSound.Value.soundLocation);
        }

        if(newState != this._state){
            currentBehavior.Stop(_navMeshAgent);
            nextBehavior.Start(_navMeshAgent, currentKnowledge);
            this._state = newState;
        }
    }

    private void TryKill(Character target){
        if(this._state == State.Chase){
            Vector3 targetPos = target.transform.position;
            Vector3 myPos = transform.position;
            Vector3 posDiff = targetPos - myPos;
            float sqrDistance = posDiff.sqrMagnitude;
            float threshold = this._killRadius * this._killRadius;
            if(sqrDistance < threshold){
                target.Kill();
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

        using(new Handles.DrawingScope(_headConfig.eye.color)){
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
        }

        // Draw sound paths for sounds
        foreach(var sound in _currentSoundInfo){
            Color color = sound.isAudible ? Color.yellow : Color.cyan;
            using(new Handles.DrawingScope(color)){
                for (int i = 1; i < sound.pathToSound.Length; i++)
                    Handles.DrawLine(sound.pathToSound[i-1], sound.pathToSound[i]);
            }
        }
    }
#endif
}
