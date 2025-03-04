using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    private enum State { Orbit, Transition };

    [SerializeField]
    private float _horizontalSpeed = 10;
    [SerializeField]
    private float _verticalSpeed = 8;
    [SerializeField]
    private float _distanceFromTarget = 5;
    [SerializeField]
    private float _targetHeightOffset = 1;
    [SerializeField]
    private float _verticalMin = -80;
    [SerializeField]
    private float _verticalMax = 80;
    [SerializeField]
    private float _transitionSpeed = 20;

    private CharacterManager _characterManager;

    private State _currentState = State.Orbit;
    private float _horizontalAngle = 0;
    private float _verticalAngle = 0;

    private CharacterBase _transitionTarget;
    private Action _transitionEndCallback;

    void Awake()
    {
        _characterManager = FindFirstObjectByType<CharacterManager>();
        if(_characterManager == null)
            throw new System.Exception("Could not find character manager");
    }

    void Update()
    {
        if(_currentState == State.Orbit){
            OrbitControl();
        }
        else if(_currentState == State.Transition){
            CameraTransition();
        }
    }

    private void OrbitControl(){
        // TODO: This is for the non-physics movement of the CharacterController
        // TODO: If and when this changes, this'll need to move to FixedUpdate()
        if(_characterManager.SelectedCharacter != null){
            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");
            _horizontalAngle += mouseX * _horizontalSpeed;
            _verticalAngle += mouseY * _verticalSpeed;
            _verticalAngle = Mathf.Clamp(_verticalAngle, _verticalMin, _verticalMax);

            Vector3 direction = GetDirectionFromYawPitch();
            Vector3 positionOffset = direction * _distanceFromTarget;
            Vector3 characterAxis = GetCharacterAxis(_characterManager.SelectedCharacter);
            Vector3 targetPosition = characterAxis + positionOffset;

            // TODO: If the player character has colliders on it,
            // TODO: This will be screwy. We'll need to figure out layers.

            // Raycast from the target position to the camera
            RaycastHit hit;
            if (Physics.Raycast(characterAxis, direction, out hit, _distanceFromTarget))
            {
                var prevTgtPos = targetPosition;
                
                // If the ray hits something, adjust the camera position
                targetPosition = hit.point + -direction * 0.2f; // Push the camera slightly in front of the hit point

                Debug.DrawLine(
                    prevTgtPos,
                    targetPosition
                );
            }

            // Set the camera's position to the calculated position
            this.transform.position = targetPosition;
            this.transform.forward = -direction;
        }
    }

    private void CameraTransition(){
        if(_transitionTarget == null) throw new System.Exception("Missing transition target.");
        if(_transitionEndCallback == null) throw new System.Exception("Missing transition end callback.");

        NavMeshPath path = new NavMeshPath();

        bool foundNavMeshStartPos = GetClosestNavMeshPointBelow(
            transform.position, 
            out Vector3 navMeshStartPos
        );
        if(!foundNavMeshStartPos){
            Debug.LogWarning("Couldn't find closest nav mesh position.");
            _transitionEndCallback();
            _transitionEndCallback = null;
            _currentState = State.Orbit;
        }

        float heightOffset = transform.position.y - navMeshStartPos.y;

        Vector3 direction = GetDirectionFromYawPitch();
        Vector3 positionOffset = direction * _distanceFromTarget;
        Vector3 characterAxis = GetCharacterAxis(_transitionTarget);
        Vector3 targetPosition = characterAxis + positionOffset;
        bool foundNavMeshEndPos = GetClosestNavMeshPointBelow(
            targetPosition, 
            out Vector3 navMeshEndPosition
        );
        if(!foundNavMeshEndPos){
            Debug.LogWarning("Couldn't find closest nav mesh position.");
            _transitionEndCallback();
            _transitionEndCallback = null;
            _currentState = State.Orbit;
        }

        NavMesh.CalculatePath(
            navMeshStartPos, 
            navMeshEndPosition,
            NavMesh.AllAreas,
            path
        );

        if(path.corners.Length == 0) {
            Debug.LogWarning("path.corners.Length = 0");
            _transitionEndCallback();
            _transitionEndCallback = null;
            _currentState = State.Orbit;
        }

        float distanceToTravel = Time.deltaTime * _transitionSpeed;
        Vector3 targetPoint = GetPointAlongPath(path, distanceToTravel);
        transform.position = new Vector3(
            targetPoint.x,
            targetPoint.y + heightOffset,
            targetPoint.z
        );
        
        if(targetPoint == path.corners[path.corners.Length - 1]){
            // We're done
            _transitionEndCallback();
            _transitionEndCallback = null;
            _currentState = State.Orbit;
        }
    }

    public void SendCameraToNewCharacter(CharacterBase target, Action arrivalCallback){
        _currentState = State.Transition;
        _transitionTarget = target;
        _transitionEndCallback = arrivalCallback;
    }

    private Vector3 GetCharacterAxis(CharacterBase target){
        Vector3 characterAxis = target.transform.position +
            Vector3.up * _targetHeightOffset;
        return characterAxis;
    }

    private Vector3 GetDirectionFromYawPitch()
    {
        return Quaternion.Euler(_verticalAngle, _horizontalAngle, 0) * Vector3.forward;
    }

    private Vector3 GetPointAlongPath(NavMeshPath path, float distanceToMove)
    {
        float distanceTraveled = 0f;

        // Find the segment of the path the camera should be on
        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 start = path.corners[i - 1];
            Vector3 end = path.corners[i];
            float segmentLength = Vector3.Distance(start, end);

            // If the distanceToMove falls within this segment, interpolate the position
            if (distanceTraveled + segmentLength >= distanceToMove)
            {
                float segmentProgress = (distanceToMove - distanceTraveled) / segmentLength;
                return Vector3.Lerp(start, end, segmentProgress);
            }

            // Otherwise, add the segment's length and continue to the next segment
            distanceTraveled += segmentLength;
        }

        // If we have moved to the end of the path, return the last corner
        return path.corners[path.corners.Length - 1];
    }

    private bool GetClosestNavMeshPointBelow(Vector3 position, out Vector3 navMeshPosition)
    {
        const float SAMPLE_RADIUS = 2;

        // Create a ray pointing downward
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit))
        {
            // Sample the NavMesh near the hit point
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(hit.point, out navMeshHit, SAMPLE_RADIUS, NavMesh.AllAreas))
            {
                navMeshPosition = navMeshHit.position;
                return true; // This is the closest point on the NavMesh
            }
        }
        navMeshPosition = Vector3.zero;
        return false;
    }
}
