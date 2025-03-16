using UnityEngine;
using UnityEngine.AI;

# nullable enable

[RequireComponent(typeof(NavMeshAgent))]
[DisallowMultipleComponent]
public class SimpleMonster : Entity, IDebugInfoProvider
{
    private enum State { Wander, Chase, LostTarget }
    private enum HeadFollowBehavior { LastKnownPos, CurrentPos, LastKnownPlusMovementDirection }

    [SerializeField]
    private Transform? _head;

    [SerializeField]
    private Light? _eye;

    [SerializeField]
    private HeadFollowBehavior _headFollowBehavior = HeadFollowBehavior.LastKnownPlusMovementDirection;

    [SerializeField]
    [Range(5, 100)]
    private float _minRedirectTime = 20;
    [SerializeField]
    [Range(5, 100)]
    private float _maxRedirectTime = 60;

    [SerializeField]
    [Range(5, 20)]
    private float _minWaitAfterLostTime = 5;
    [SerializeField]
    [Range(5, 20)]
    private float _maxWaitAfterLostTime = 10;
    [SerializeField]
    [Range(0, 1)]
    private float _minWaitAfterLostTimeDist = 0.1f;

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
    private float _newDestinationTimer = 0f;
    private float _waitAfterLostTimer = 0f;
    private Character? _targetCharacter = null;
    private Vector3? _targetCharacterLastSeenPosition = null;
    private Vector3? _targetCharacterLastSeenVelocity = null;

    public string DebugName => "Simple Monster";

    public string DebugInfo
    {
        get
        {
            switch (this._state)
            {
                case State.Wander:
                    return $"Wander: {_newDestinationTimer.ToString("F2")}s";
                case State.Chase:
                    return $"Chase: {_targetCharacter?.gameObject.name}";
                case State.LostTarget:
                    return $"LostTarget: {_targetCharacter?.gameObject.name}, {_waitAfterLostTimer}s";
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

        if (_minRedirectTime > _maxRedirectTime)
            throw new System.Exception("Invalid redirect times");
    }

    private void Start()
    {
        // Set a random timer interval on start
        SetRandomRedirectInterval();

        // Start with a destination
        NewDestination();
    }

    private void Update()
    {
        MoveHead(Time.deltaTime);
        LookForCharacters();

        switch (this._state)
        {
            case State.Wander:
                UpdateSearch();
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.LostTarget:
                UpdateLostTarget(Time.deltaTime);
                break;
            default:
                throw new System.Exception($"Unrecognized monster state: {this._state}");
        }
    }

    private void SetRandomRedirectInterval()
    {
        // Randomly select a new interval between min and max
        _newDestinationTimer = Random.Range(_minRedirectTime, _maxRedirectTime);
    }

    private void NewDestination()
    {
        if (_navManager == null)
            throw new System.Exception($"Null _navManager on {this.gameObject.name}");
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        _navMeshAgent.SetDestination(
            _navManager.GetRandomDestinationStanding()
        );
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

    private void UpdateSearch()
    {
        // Countdown timer logic to choose a new destination
        _newDestinationTimer -= Time.deltaTime;

        if (_newDestinationTimer <= 0f)
        {
            NewDestination();
            SetRandomRedirectInterval(); // Choose a new random interval
        }
    }

    private void UpdateChase()
    {
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");
        if (_targetCharacterLastSeenPosition == null)
            throw new System.Exception($"Null target character on {this.gameObject.name}");

        _navMeshAgent.SetDestination(
            this._targetCharacterLastSeenPosition.Value
        );
    }

    private void UpdateLostTarget(float deltaTime)
    {
        if(_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        if(_navMeshAgent.remainingDistance < _minWaitAfterLostTimeDist){

            // Zero out everything so that the
            // head starts looking left and right again
            this._targetCharacter = null;
            this._targetCharacterLastSeenPosition = null;
            this._targetCharacterLastSeenVelocity = null;

            _waitAfterLostTimer -= deltaTime;
            if(_waitAfterLostTimer <= 0){
                this._state = State.Wander;
                NewDestination();
                SetRandomRedirectInterval();
            }
        }
    }

    private void LookForCharacters()
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
            this._state = State.Chase;
        }
        else
        {
            if (this._state == State.Chase)
            {
                this._state = State.LostTarget;
                _waitAfterLostTimer = Random.Range(_minWaitAfterLostTime, _maxWaitAfterLostTime);
            }
            else if(this._state == State.LostTarget){

            }
            else if(this._state == State.Wander)
            {
                // Wander clears everything out (I guess?)
                this._targetCharacter = null;
                this._targetCharacterLastSeenPosition = null;
                this._targetCharacterLastSeenVelocity = null;
                this._state = State.Wander;
            }
            else{
                throw new System.Exception($"Unhandled state {this._state}");
            }
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
