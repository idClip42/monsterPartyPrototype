using UnityEngine;

# nullable enable

public class SimpleMonsterHead{
    public enum HeadFollowBehavior { LastKnownPos, CurrentPos, LastKnownPlusMovementDirection }

    [System.Serializable]
    public class Config{
        [SerializeField]
        public Transform? head;

        [SerializeField]
        public Light? eye;

        [SerializeField]
        public Color wanderLightColor = Color.white;

        [SerializeField]
        public Color chaseLightColor = Color.red;

        [SerializeField]
        public Color searchLightColor = Color.yellow;

        [SerializeField]
        [Range(10, 100)]
        public float maxSightDistance = 20;

        [SerializeField]
        [Range(1, 179)]
        public float fieldOfView = 100;

        [SerializeField]
        public HeadFollowBehavior headFollowBehavior = HeadFollowBehavior.LastKnownPlusMovementDirection;

        [SerializeField]
        [Range(0, 90)]
        public float headSwingMaxAngle = 60;

        [SerializeField]
        [Range(1, 10)]
        public float headSwingPeriod = 3;

        [SerializeField]
        public LayerMask generalLookRaycastMask = Physics.AllLayers;

        [SerializeField]
        public LayerMask chaseLookRaycastMask = Physics.AllLayers;
    }

    private SimpleMonster _monster;
    private Config _config;
    private Character[] _characters = { };
    private float _headSwingTimer = 0;
    private SimpleMonsterState.Knowledge _currentKnowledge;

    public float MaxSightDistance => _config.maxSightDistance;
    public float FieldOfView => _config.eye ? _config.eye.spotAngle : 0;

    public SimpleMonsterState.Knowledge CurrentKnowledge => _currentKnowledge;

    public SimpleMonsterHead(SimpleMonster monster, Config config, Character[] characters){
        this._monster = monster;
        this._config = config;
        this._characters = characters;

        if(this._config.eye == null)
            throw new System.Exception("Missing eye.");
        this._config.eye.enabled = true;
    }

    public void OnUpdate(float deltaTime, SimpleMonsterState currentStateInfo){
        if(this._config.eye == null)
            throw new System.Exception("Missing eye.");
        this._config.eye.range = this._config.maxSightDistance;
        this._config.eye.spotAngle = this._config.fieldOfView;
       
        Color currentColor = ColorFromState(this._monster.CurrentState);
        Color nextColor = ColorFromState(currentStateInfo.NextState);
        Color transitionColor = Color.Lerp(currentColor, nextColor, currentStateInfo.ProgressToNextState);
        this._config.eye.color = transitionColor;

        MoveHead(deltaTime);
        LookForCharacters(deltaTime);
    }

    public void AttractAttention(Vector3 targetSpot){
        this._currentKnowledge.visibleTarget = null;
        this._currentKnowledge.lastSeenPosition = targetSpot;
        this._currentKnowledge.lastSeenVelocity = Vector3.zero;
    }

    private void MoveHead(float deltaTime)
    {
        SimpleMonsterState.Knowledge newThing = new();
        newThing.visibleTarget = null;

        if (_config.head == null)
            throw new System.Exception("Missing head.");

        Vector3? lookTarget = GetLookTarget();
        if (lookTarget != null && this._currentKnowledge.visibleTarget != null)
        {
            // Move head to look at target
            Vector3 atTarget = (lookTarget - this._config.head.position).Value.normalized;
            Vector3 projected = Vector3.ProjectOnPlane(atTarget, Vector3.up).normalized;
            this._config.head.transform.forward = projected;
        }
        else
        {
            // Swing left and right
            _headSwingTimer += deltaTime;
            float sinCurve = Mathf.Sin(_headSwingTimer * Mathf.PI * 2f / _config.headSwingPeriod);
            float angle = sinCurve * _config.headSwingMaxAngle;
            this._config.head.localRotation = Quaternion.Euler(0, angle, 0);
        }
    }

    private Vector3? GetLookTarget(){
        switch (_config.headFollowBehavior)
        {
            case HeadFollowBehavior.LastKnownPos:
                return _currentKnowledge.lastSeenPosition;
            case HeadFollowBehavior.CurrentPos:
                return _currentKnowledge.visibleTarget?.transform.position;
            case HeadFollowBehavior.LastKnownPlusMovementDirection:
                // If there's no position,
                // we can do nothing
                if(_currentKnowledge.lastSeenPosition == null)
                    return null;
                // If there's no velocity,
                // we can at least return position
                if(_currentKnowledge.lastSeenVelocity == null)
                    return _currentKnowledge.lastSeenPosition.Value;
                // If we have both,
                // we can "lead" the player position
                return _currentKnowledge.lastSeenPosition.Value + 
                    _currentKnowledge.lastSeenVelocity.Value;
            default:
                throw new System.Exception($"Unrecognized behavior: {_config.headFollowBehavior}");
        }
    }

    private void LookForCharacters(float deltaTime)
    {
        if (_config.eye == null)
            throw new System.Exception("Missing eye.");

        Character? closestVisibleCharacter = null;
        float closestDistance = float.MaxValue;
        Vector3 eyePos = _config.eye.transform.position;

        RaycastHit hitInfo;
        foreach (var targetCharacter in _characters)
        {
            if(targetCharacter.Alive == false)
                continue;

            foreach (Transform target in targetCharacter.LookRaycastTargets)
            {
                Vector3 targetPos = target.position;
                Vector3 toTarget = targetPos - eyePos;
                float distance = toTarget.magnitude;

                if(distance > MaxSightDistance)
                    continue;

                // We use our general raycast mask...
                LayerMask raycastMask = _config.generalLookRaycastMask;
                // ...unless we already have our eyes on a target...
                bool isChasing = this._monster.CurrentState == SimpleMonster.State.Chase;
                bool isTarget = targetCharacter == this._currentKnowledge.visibleTarget;
                // ...in which case we use a special chase mask.
                if(isChasing && isTarget)
                    raycastMask = _config.chaseLookRaycastMask;

                // If we hit nothing, we have a clear line of sight
                // to the target character.
                bool lineOfSight = !Physics.Raycast(
                    eyePos,
                    toTarget / distance,
                    out hitInfo,
                    distance,
                    raycastMask
                );

                // If we hit something
                // and what we hit was the target character,
                // still a clear line of sight.
                if (!lineOfSight && hitInfo.collider.gameObject == target.gameObject)
                    lineOfSight = true;

                if (lineOfSight)
                {
                    Vector3 lightDirection = _config.eye.transform.forward;
                    Vector3 targetDirection = toTarget / distance;

                    float spotAngleInRadians = FieldOfView * Mathf.Deg2Rad;
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

                        Debug.DrawLine( eyePos, targetPos, Color.red );
                    }
                    else
                    {
                        // Target is outside the vision cone
                        Debug.DrawLine( eyePos, targetPos, Color.yellow );
                    }
                }
                else
                {
                    Debug.DrawLine( eyePos, hitInfo.point, Color.white );
                }
            }
        }

        if (closestVisibleCharacter != null)
        {
            this._currentKnowledge.visibleTarget = closestVisibleCharacter;
            this._currentKnowledge.lastSeenPosition = closestVisibleCharacter.transform.position;
            this._currentKnowledge.lastSeenVelocity = closestVisibleCharacter.CurrentVelocity;
        }
        else
        {
            this._currentKnowledge.visibleTarget = null;
        }

        if (this._currentKnowledge.lastSeenPosition != null)
        {
            Debug.DrawLine(
                eyePos,
                this._currentKnowledge.lastSeenPosition.Value,
                Color.red
            );

            if(this._currentKnowledge.lastSeenVelocity != null){
                Debug.DrawLine(
                    this._currentKnowledge.lastSeenPosition.Value,
                    this._currentKnowledge.lastSeenPosition.Value + this._currentKnowledge.lastSeenVelocity.Value,
                    Color.red
                );
            }
        }
    }

    private Color ColorFromState(SimpleMonster.State state){
        return state switch{
            SimpleMonster.State.Wander => this._config.wanderLightColor,
            SimpleMonster.State.Chase => this._config.chaseLightColor,
            SimpleMonster.State.Search => this._config.searchLightColor,
            _ => throw new System.Exception($"Unrecognized state '{state}'")
        };
    }

    public sealed override bool Equals(object other) => base.Equals(other);
    public sealed override int GetHashCode() => base.GetHashCode();
    public sealed override string ToString() => base.ToString();
}