using System;
using UnityEngine;
using UnityEngine.AI;

#nullable enable

public class CameraControlTransition
{
    private readonly CameraControl _cameraControl;
    private NavigationManager _navManager;
    private Character? _transitionTarget = null;
    private Vector3 _positionOffset = Vector3.zero;
    private float _transitionSpeed = -1;
    private LayerMask _navMeshRaycastLayers;

    public CameraControlTransition(CameraControl owner, NavigationManager navManager){
        if(owner == null) throw new Exception("Missing camera control owner.");
        if(navManager == null) throw new Exception("Missing navManager.");
        _cameraControl = owner;
        _navManager = navManager;
    }

    public void Initialize(Character target, Vector3 posOffset, float speed, LayerMask navMeshRaycastLayers){
        _transitionTarget = target;
        _positionOffset = posOffset;
        _transitionSpeed = speed;
        _navMeshRaycastLayers = navMeshRaycastLayers;
    }

    public void Clear(){
        _transitionTarget = null;
        _positionOffset = Vector3.zero;
        _transitionSpeed = 0;
        _navMeshRaycastLayers = NavMesh.AllAreas;
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
            GetFilter(),
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

    private NavMeshQueryFilter GetFilter(){
        return new NavMeshQueryFilter(){
            areaMask=NavMesh.AllAreas, 
            agentTypeID=_navManager.CrouchingAgentTypeId
        };
    }

    private bool GetClosestNavMeshPointBelow(Vector3 position, out Vector3 navMeshPosition)
    {
        const float SAMPLE_RADIUS = 3;
        const float MAX_DIST = 30;

        // Create a ray pointing downward
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, MAX_DIST, _navMeshRaycastLayers))
        {
            // Debug.DrawLine(position, hit.point, Color.green, 5);
            
            // Sample the NavMesh near the hit point
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(
                hit.point, 
                out navMeshHit, 
                SAMPLE_RADIUS, 
                GetFilter()
            ))
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

    public sealed override bool Equals(object other) => base.Equals(other);
    public sealed override int GetHashCode() => base.GetHashCode();
    public sealed override string ToString() => base.ToString();
}
