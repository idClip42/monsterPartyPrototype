using UnityEngine;
using UnityEngine.AI;

# nullable enable

[RequireComponent(typeof(NavMeshAgent))]
[DisallowMultipleComponent]
public class SimpleMonster : Entity, IDebugInfoProvider
{
    public enum State { Wander, Chase, LostTarget }
    private enum HeadFollowBehavior { LastKnownPos, CurrentPos, LastKnownPlusMovementDirection }

    [SerializeField]
    private Transform? _head;

    [SerializeField]
    private Light? _eye;

    [SerializeField]
    private HeadFollowBehavior _headFollowBehavior = HeadFollowBehavior.LastKnownPlusMovementDirection;

    [SerializeField]
    private SimpleMonsterStateWander.Config? _wanderConfig;

    [SerializeField]
    private SimpleMonsterStateLostTarget.Config? _lostTargetConfig;

    [SerializeField]
    [Range(0, 90)]
    private float _headSwingMaxAngle = 60;
    [SerializeField]
    [Range(1, 10)]
    private float _headSwingPeriod = 1;

    private Character[] _characters = { };

    private NavigationManager? _navManager = null;
    private NavMeshAgent? _navMeshAgent = null;

    private State _state = State.Wander;
    private float _headSwingTimer = 0;

    private Character? _targetCharacter = null;
    private Vector3? _targetCharacterLastSeenPosition = null;
    private Vector3? _targetCharacterLastSeenVelocity = null;

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

        _characters = FindObjectsByType<Character>(FindObjectsSortMode.None);

        if (_head == null)
            throw new System.Exception("Missing head.");

        if (_eye == null)
            throw new System.Exception("Missing eye.");
        _eye.enabled = true;

        _navManager = FindFirstObjectByType<NavigationManager>();
        if (_navManager == null)
            throw new System.Exception($"Null _navManager on {this.gameObject.name}");

        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

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
        MoveHead(Time.deltaTime);
        LookForCharacters(Time.deltaTime);

        if(_wanderBehavior == null)
            throw new System.Exception($"Missing wander behavior on {this.gameObject.name}");
        if(_chaseBehavior == null)
            throw new System.Exception($"Missing chase behavior on {this.gameObject.name}");
        if(_lostTargetBehavior == null)
            throw new System.Exception($"Missing lost target behavior on {this.gameObject.name}");
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        var currentKnowledge = new SimpleMonsterState.Knowledge(){
            visibleTarget = _targetCharacter,
            lastSeenPosition = _targetCharacterLastSeenPosition,
            lastSeenVelocity = _targetCharacterLastSeenVelocity
        };

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
    }

    private void MoveHead(float deltaTime)
    {
        if (_head == null)
            throw new System.Exception("Missing head.");

        Vector3? lookTarget;
        switch (_headFollowBehavior)
        {
            case HeadFollowBehavior.LastKnownPos:
                if(this._targetCharacterLastSeenPosition == null)
                    lookTarget = null;
                else
                    lookTarget = this._targetCharacterLastSeenPosition;
                break;
            case HeadFollowBehavior.CurrentPos:
                if(this._targetCharacter == null)
                    lookTarget = null;
                else
                    lookTarget = this._targetCharacter.transform.position;
                break;
            case HeadFollowBehavior.LastKnownPlusMovementDirection:
                if(this._targetCharacterLastSeenPosition == null || this._targetCharacterLastSeenVelocity == null)
                    lookTarget = null;
                else
                    lookTarget = this._targetCharacterLastSeenPosition.Value + this._targetCharacterLastSeenVelocity.Value;
                break;
            default:
                throw new System.Exception($"Unrecognized behavior: {_headFollowBehavior}");
        }

        if (lookTarget != null)
        {
            // Move head to look at target
            Vector3 atTarget = (lookTarget - this._head.position).Value.normalized;
            Vector3 projected = Vector3.ProjectOnPlane(atTarget, Vector3.up).normalized;
            this._head.transform.forward = projected;
        }
        else
        {
            // Swing left and right
            _headSwingTimer += deltaTime;
            float sinCurve = Mathf.Sin(_headSwingTimer * Mathf.PI * 2f / _headSwingPeriod);
            float angle = sinCurve * _headSwingMaxAngle;
            this._head.localRotation = Quaternion.Euler(0, angle, 0);
        }
    }

    private void LookForCharacters(float deltaTime)
    {
        if (_eye == null)
            throw new System.Exception("Missing eye.");

        Character? closestVisibleCharacter = null;
        float closestDistance = float.MaxValue;

        RaycastHit hitInfo;
        foreach (var targetCharacter in _characters)
        {
            foreach (Transform target in targetCharacter.LookRaycastTargets)
            {
                Vector3 targetPos = target.position;
                Vector3 toTarget = targetPos - _eye.transform.position;
                float distance = toTarget.magnitude;

                // If we hit nothing, we have a clear line of sight
                // to the target character.
                bool lineOfSight = !Physics.Raycast(
                    _eye.transform.position,
                    toTarget / distance,
                    out hitInfo,
                    distance
                );

                // If we hit something
                // and what we hit was the target character,
                // still a clear line of sight.
                if (!lineOfSight && hitInfo.collider.gameObject == target.gameObject)
                    lineOfSight = true;

                if (lineOfSight)
                {
                    Vector3 lightDirection = _eye.transform.forward;
                    Vector3 targetDirection = toTarget / distance;
                    float spotAngle = _eye.spotAngle;

                    float spotAngleInRadians = spotAngle * Mathf.Deg2Rad;
                    float cosHalfSpotAngle = Mathf.Cos(spotAngleInRadians / 2);
                    float dotProduct = Vector3.Dot(lightDirection, targetDirection);
                    if (dotProduct > cosHalfSpotAngle)
                    {
                        // Target is within the vision cone
                        if (distance < closestDistance)
                        {
                            closestVisibleCharacter = targetCharacter;
                            closestDistance = distance;
                        }

                        Debug.DrawLine(
                            _eye.transform.position,
                            targetPos,
                            Color.red
                        );
                    }
                    else
                    {
                        // Target is outside the vision cone
                        Debug.DrawLine(
                            _eye.transform.position,
                            targetPos,
                            Color.yellow
                        );
                    }
                }
                else
                {
                    Debug.DrawLine(
                        _eye.transform.position,
                        hitInfo.point,
                        Color.white
                    );
                }
            }
        }

        if (closestVisibleCharacter != null)
        {
            this._targetCharacter = closestVisibleCharacter;
            this._targetCharacterLastSeenPosition = closestVisibleCharacter.transform.position;
            this._targetCharacterLastSeenVelocity = closestVisibleCharacter.CurrentVelocity;
        }
        else
        {
            this._targetCharacter = null;
        }

        if (this._targetCharacterLastSeenPosition != null)
        {
            Debug.DrawLine(
                _eye.transform.position,
                this._targetCharacterLastSeenPosition.Value,
                Color.red
            );

            if(this._targetCharacterLastSeenVelocity != null){
                Debug.DrawLine(
                    this._targetCharacterLastSeenPosition.Value,
                    this._targetCharacterLastSeenPosition.Value + this._targetCharacterLastSeenVelocity.Value,
                    Color.red
                );
            }
        }
    }
}
