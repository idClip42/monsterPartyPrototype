using System;
using UnityEngine;
using UnityEngine.AI;

#nullable enable

public class CameraControlTransition
{
    private readonly CameraControl? _cameraControl = null;
    private Character? _transitionTarget = null;
    private Vector3 _positionOffset = Vector3.zero;
    private float _transitionSpeed = -1;

    public CameraControlTransition(CameraControl? owner){
        if(owner == null) throw new Exception("Missing camera control owner.");
        _cameraControl = owner;
    }

    public void Initialize(Character target, Vector3 posOffset, float speed){
        _transitionTarget = target;
        _positionOffset = posOffset;
        _transitionSpeed = speed;
    }

    public void Clear(){
        _transitionTarget = null;
        _positionOffset = Vector3.zero;
        _transitionSpeed = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <returns>Whether to finish transitioning</returns>
    /// <exception cref="Exception"></exception>
    public bool MoveCamera(float deltaTime){
        if(_cameraControl == null) throw new Exception("Null _cameraControl");
        if(_transitionTarget == null) throw new Exception("Missing transition target.");
        if(_transitionSpeed <= 0) throw new Exception("Invalid transition speed.");

        NavMeshPath path = new NavMeshPath();

        bool foundNavMeshStartPos = GetClosestNavMeshPointBelow(
            _cameraControl.transform.position, 
            out Vector3 navMeshStartPos
        );
        if(!foundNavMeshStartPos){
            Debug.LogWarning("Couldn't find closest nav mesh position.");
            return true;
        }

        float heightOffset = _cameraControl.transform.position.y - navMeshStartPos.y;

        Vector3 characterAxis = _cameraControl.GetCharacterAxis(_transitionTarget);
        Vector3 targetPosition = characterAxis + _positionOffset;
        bool foundNavMeshEndPos = GetClosestNavMeshPointBelow(
            targetPosition, 
            out Vector3 navMeshEndPosition
        );
        if(!foundNavMeshEndPos){
            Debug.LogWarning("Couldn't find closest nav mesh position.");
            return true;
        }

        NavMesh.CalculatePath(
            navMeshStartPos, 
            navMeshEndPosition,
            NavMesh.AllAreas,
            path
        );

        if(path.corners.Length == 0) {
            Debug.LogWarning("path.corners.Length = 0");
            return true;
        }

        float distanceToTravel = Time.deltaTime * _transitionSpeed;
        Vector3 targetPoint = GetPointAlongPath(path, distanceToTravel);
        _cameraControl.transform.position = new Vector3(
            targetPoint.x,
            targetPoint.y + heightOffset,
            targetPoint.z
        );
        
        if(targetPoint == path.corners[path.corners.Length - 1]){
            // We're done
            return true;
        }

        return false;
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
}
