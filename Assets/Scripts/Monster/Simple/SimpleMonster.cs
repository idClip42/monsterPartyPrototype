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
    private SimpleMonsterHearing.Config? _hearingConfig;

    [SerializeField]
    private SimpleMonsterBarks.Config? _barksConfig;

    [SerializeField]
    [Range(0.5f, 3)]
    private float _killRadius = 1;

    [SerializeField]
    private AudioSource? _killSoundEffect = null;

    [SerializeField]
    private GameObject[] _setInactiveOnDeath = {};

    private NavigationManager? _navManager = null;
    private NavMeshAgent? _navMeshAgent = null;

    private State _state = State.Wander;
    private SoundInfo[] _currentSoundInfo = {};

    private SimpleMonsterHead? _headBehavior = null;
    private SimpleMonsterStateWander? _wanderBehavior = null;
    private SimpleMonsterStateChase? _chaseBehavior = null;
    private SimpleMonsterStateSearch? _searchBehavior = null;
    private SimpleMonsterHearing? _hearing = null;
    private SimpleMonsterBarks? _barks = null;

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

    protected sealed override void Awake()
    {
        base.Awake();

        if(_headConfig == null)
            throw new System.Exception($"Missing head config on {this.gameObject.name}");
        if(_wanderConfig == null)
            throw new System.Exception($"Missing wander config on {this.gameObject.name}");
        if(_chaseConfig == null)
            throw new System.Exception($"Missing chase config on {this.gameObject.name}");
        if(_searchConfig == null)
            throw new System.Exception($"Missing search config on {this.gameObject.name}");
        if(_barksConfig == null)
            throw new System.Exception($"Missing barks config on {this.gameObject.name}");
        if(_hearingConfig == null)
            throw new System.Exception($"Missing hearing config on {this.gameObject.name}");

        _navManager = FindFirstObjectByType<NavigationManager>();
        if (_navManager == null)
            throw new System.Exception($"Null _navManager on {this.gameObject.name}");
        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        _wanderBehavior = new SimpleMonsterStateWander(_wanderConfig, _navManager);
        _chaseBehavior = new SimpleMonsterStateChase(_chaseConfig);
        _searchBehavior = new SimpleMonsterStateSearch(_searchConfig);

        var charactersArray = FindObjectsByType<Character>(FindObjectsSortMode.None);
        if(charactersArray.Length == 0){
            Debug.LogWarning($"{this.gameObject.name} found no Characters in the scene.");
        }
        _headBehavior = new SimpleMonsterHead(this, _headConfig, charactersArray);

        INoiseSource[] noiseSources = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<INoiseSource>()
            .ToArray();
        _hearing = new SimpleMonsterHearing(this, noiseSources, _navManager, _hearingConfig);

        _barks = new SimpleMonsterBarks(_barksConfig);

        this.OnDeath += HandleDeath;
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
        if(this.Alive == false) return;

        if(_headBehavior == null)
            throw new System.Exception($"Missing head behavior on {this.gameObject.name}");

        UpdateBehavior();

        if(_headBehavior.CurrentKnowledge.visibleTarget != null)
            TryKill(_headBehavior.CurrentKnowledge.visibleTarget);
    }

    private void UpdateBehavior(){
        if(_headBehavior == null)
            throw new System.Exception($"Missing head behavior on {this.gameObject.name}");
        if(_hearing == null)
            throw new System.Exception($"Missing hearing on {this.gameObject.name}");
        if(_barks == null)
            throw new System.Exception($"Missing barks on {this.gameObject.name}");
        if(_searchBehavior == null)
            throw new System.Exception($"Missing search behavior on {this.gameObject.name}");
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        // Handle current behavior and
        // get prospective behavior change.
        SimpleMonsterState currentBehavior = StateToBehavior(this._state);
        _headBehavior.OnUpdate(Time.deltaTime, currentBehavior);
        State newState = currentBehavior.OnUpdate(Time.deltaTime, _headBehavior.CurrentKnowledge, _navMeshAgent);
        SimpleMonsterState nextBehavior = StateToBehavior(newState);

        // Check if we want to redirect
        // our behavior change based on
        // hearing something.
        this._currentSoundInfo = _hearing.CheckForSounds();
        bool heardSomething = false;
        if(nextBehavior.AllowInterruption && this._currentSoundInfo.Any(s=>s.isAudible)){
            // Get the closest audible noise.
            SoundInfo targetSound = 
                SoundInfo.GetNearest(this._currentSoundInfo);
            // Double check what we got is audible.
            if(targetSound.isAudible == false)
                throw new System.Exception("Target sound should be audible.");
            // Redirect next state.
            newState = State.Search;
            nextBehavior = _searchBehavior;
            // Alert head behavior to sound.
            _headBehavior.AttractAttention(targetSound.soundLocation);
            heardSomething = true;
        }

        // If we want to change behavior,
        // stop the old one and start the
        // new one.
        if(newState != this._state){
            currentBehavior.Stop(_navMeshAgent);
            nextBehavior.Start(_navMeshAgent, _headBehavior.CurrentKnowledge);

            if(heardSomething){
                _barks.PlayOnHearTarget();
            }
            else if(newState == State.Chase){
                _barks.PlayOnSeeTarget();
            }
            else if(newState == State.Search && this._state == State.Chase){
                _barks.PlayOnLoseTarget();
            }
            else if(newState == State.Wander){
                _barks.PlayOnReturnToWander();
            }

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
                if(_killSoundEffect != null)
                    _killSoundEffect.Play();
                else
                    Debug.LogWarning("Null kill sound effect");
            }
        }
    }

    private SimpleMonsterState StateToBehavior(State state){
        if(_wanderBehavior == null)
            throw new System.Exception($"Missing wander behavior on {this.gameObject.name}");
        if(_chaseBehavior == null)
            throw new System.Exception($"Missing chase behavior on {this.gameObject.name}");
        if(_searchBehavior == null)
            throw new System.Exception($"Missing search behavior on {this.gameObject.name}");

        return state switch {
            State.Wander => _wanderBehavior,
            State.Chase => _chaseBehavior,
            State.Search => _searchBehavior,
            _ => throw new System.Exception($"Unrecognized monster state '{state}'")
        };
    }

    public void Kill(){
        this.Die();
    }

    protected virtual void HandleDeath(Entity deadEntity){
        if(_barks == null)
            throw new System.Exception($"Missing barks on {this.gameObject.name}");
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        Light[] allLights = GetComponentsInChildren<Light>();
        foreach(Light l in allLights)
            l.enabled = false;

        _barks.PlayOnDeath();

        _navMeshAgent.isStopped = true;
        _state = State.Wander;

        foreach(var go in _setInactiveOnDeath)
            go.SetActive(false);
    }

#if UNITY_EDITOR
    protected sealed override void OnDrawGizmos() {
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
                foreach(var blocker in sound.blockers){
                    Handles.DrawWireDisc(
                        blocker.transform.position,
                        blocker.transform.forward,
                        1
                    );
                    Handles.Label(
                        blocker.transform.position,
                        $"+{blocker.DistancePenalty}m"
                    );
                }
            }
        }
    }
#endif
}
