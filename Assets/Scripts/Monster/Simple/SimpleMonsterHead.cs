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

    private Character? _targetCharacter = null;
    private Vector3? _targetCharacterLastSeenPosition = null;
    private Vector3? _targetCharacterLastSeenVelocity = null;

    public float MaxSightDistance => _config.maxSightDistance;
    public float FieldOfView => _config.eye ? _config.eye.spotAngle : 0;

    public SimpleMonsterState.Knowledge CurrentKnowledge => new SimpleMonsterState.Knowledge(){
        visibleTarget = _targetCharacter,
        lastSeenPosition = _targetCharacterLastSeenPosition,
        lastSeenVelocity = _targetCharacterLastSeenVelocity
    };

    public SimpleMonsterHead(SimpleMonster monster, Config config, Character[] characters){
        this._monster = monster;
        this._config = config;
        this._characters = characters;

        if(this._config.eye == null)
            throw new System.Exception("Missing eye.");
        this._config.eye.enabled = true;
    }

    public void OnUpdate(float deltaTime){
        if(this._config.eye == null)
            throw new System.Exception("Missing eye.");
        this._config.eye.range = this._config.maxSightDistance;
        this._config.eye.spotAngle = this._config.fieldOfView;
        this._config.eye.color = this._monster.CurrentState switch{
            SimpleMonster.State.Wander => this._config.wanderLightColor,
            SimpleMonster.State.Chase => this._config.chaseLightColor,
            SimpleMonster.State.Search => this._config.searchLightColor,
            _ => throw new System.Exception($"Unrecognized state '{this._monster.CurrentState}'")
        };

        MoveHead(deltaTime);
        LookForCharacters(deltaTime);
    }

    private void MoveHead(float deltaTime)
    {
        if (_config.head == null)
            throw new System.Exception("Missing head.");

        Vector3? lookTarget;
        switch (_config.headFollowBehavior)
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
                throw new System.Exception($"Unrecognized behavior: {_config.headFollowBehavior}");
        }

        if (lookTarget != null && this._targetCharacter != null)
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

    private void LookForCharacters(float deltaTime)
    {
        if (_config.eye == null)
            throw new System.Exception("Missing eye.");

        Character? closestVisibleCharacter = null;
        float closestDistance = float.MaxValue;

        RaycastHit hitInfo;
        foreach (var targetCharacter in _characters)
        {
            if(targetCharacter.Alive == false)
                continue;

            foreach (Transform target in targetCharacter.LookRaycastTargets)
            {
                Vector3 targetPos = target.position;
                Vector3 toTarget = targetPos - _config.eye.transform.position;
                float distance = toTarget.magnitude;

                if(distance > MaxSightDistance)
                    continue;

                LayerMask raycastMask = _config.generalLookRaycastMask;
                if(this._monster.CurrentState == SimpleMonster.State.Chase && 
                    targetCharacter == this._targetCharacter
                ){
                    raycastMask = _config.chaseLookRaycastMask;
                }

                // If we hit nothing, we have a clear line of sight
                // to the target character.
                bool lineOfSight = !Physics.Raycast(
                    _config.eye.transform.position,
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

                        Debug.DrawLine(
                            _config.eye.transform.position,
                            targetPos,
                            Color.red
                        );
                    }
                    else
                    {
                        // Target is outside the vision cone
                        Debug.DrawLine(
                            _config.eye.transform.position,
                            targetPos,
                            Color.yellow
                        );
                    }
                }
                else
                {
                    Debug.DrawLine(
                        _config.eye.transform.position,
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
                _config.eye.transform.position,
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