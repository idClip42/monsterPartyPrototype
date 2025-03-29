using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

#nullable enable

[DisallowMultipleComponent]
public class NavigationManager : MonoBehaviour
{
    private struct NavSpot {
        private const float RAYCAST_MAX_DIST = 30;
        private static Vector3[] _raycastDirs = new Vector3[]{
            Vector3.forward,
            Vector3.left,
            Vector3.right,
            Vector3.back
        };

        public readonly Vector3 position;
        public readonly bool standable;
        public readonly bool crouchable;
        public readonly float closestWallDistance;

        public NavSpot(Vector3 position, bool standable, bool crouchable, LayerMask raycastMask){
            this.position = position;
            this.standable = standable;
            this.crouchable = crouchable;

            this.closestWallDistance = float.MaxValue;
            RaycastHit hitInfo; 
            foreach(var dir in _raycastDirs){
                if(Physics.Raycast(
                    this.position,
                    dir,
                    out hitInfo,
                    RAYCAST_MAX_DIST,
                    raycastMask
                )){
                    if(hitInfo.distance < this.closestWallDistance)
                        this.closestWallDistance = hitInfo.distance;
                }
            }
        }
    }

    [SerializeField]
    private float _interval = 3;

    [SerializeField]
    private NavMeshSurface? _standingNavMesh;

    [SerializeField]
    private NavMeshSurface? _crouchingNavMesh;

    [SerializeField]
    private LayerMask _distanceFromWallRaycastMask = Physics.AllLayers;

    [SerializeField]
    [Range(1, 10)]
    private float _gizmosSpotMaxDist = 6;

    [SerializeField]
    [Range(0.1f, 5)]
    private float _gizmosSpotRange = 3;

    private NavSpot[]? _navPoints = null;

    public int StandingAgentTypeId { get {
        if(_standingNavMesh == null)
            throw new System.Exception("Null _standingNavMesh");
        return _standingNavMesh.agentTypeID;
    }} 

    public int CrouchingAgentTypeId { get {
        if(_crouchingNavMesh == null)
            throw new System.Exception("Null _crouchingNavMesh");
        return _crouchingNavMesh.agentTypeID;
    }} 

    public int NavPointsCount => _navPoints != null ? _navPoints.Length : -1;

    private void Awake()
    {
        Refresh();
    }

    public void Refresh(){
        if(_standingNavMesh == null)
            throw new System.Exception("Null _standingNavMesh");
        if(_crouchingNavMesh == null)
            throw new System.Exception("Null _crouchingNavMesh");

        var standingNavPoints = GetPossiblePointsOnNavMesh(_standingNavMesh);
        var crouchingNavPoints = GetPossiblePointsOnNavMesh(_crouchingNavMesh);

        var tempNavPoints = standingNavPoints.Select(
            pt => new NavSpot(
                pt, 
                true, 
                crouchingNavPoints.Contains(pt),
                _distanceFromWallRaycastMask
            )
        ).ToList();
        tempNavPoints.AddRange(
            crouchingNavPoints
                .Where(pt => !standingNavPoints.Contains(pt))
                .Select(pt => new NavSpot( pt, false, true, _distanceFromWallRaycastMask))
        );

        _navPoints = tempNavPoints.ToArray();
    }

    public Vector3 GetRandomDestination(bool excludeStanding, bool excludeCrouching, float minDistFromWall){
        if(_navPoints == null) 
            throw new System.Exception("No nav points array!");
        var filteredPoints = _navPoints.Where(pt => {
            if(excludeStanding && pt.standable) return false;
            if(excludeCrouching && pt.crouchable) return false;
            if(pt.closestWallDistance < minDistFromWall) return false;
            return true;
        });
        if(filteredPoints.Count() == 0)
            throw new System.Exception("Filter returned no nav points!");
        return filteredPoints.ToArray()[Random.Range(0, filteredPoints.Count())].position;
    }

    private Vector3[] GetPossiblePointsOnNavMesh(NavMeshSurface navMeshSurface){
        List<Vector3> tempList = new List<Vector3>();
        NavMeshHit hit;
        Bounds bounds = navMeshSurface.navMeshData.sourceBounds;
        for(float x = bounds.min.x; x <= bounds.max.x; x += _interval){
            for(float y = bounds.min.y; y <= bounds.max.y; y += _interval){
                for(float z = bounds.min.z; z <= bounds.max.z; z += _interval){
                    Vector3 testPosition = new Vector3(x,y,z);
                    bool foundOne = NavMesh.SamplePosition(
                        testPosition,
                        out hit,
                        _interval / 2,
                        new NavMeshQueryFilter(){
                            agentTypeID=navMeshSurface.agentTypeID,
                            areaMask=NavMesh.AllAreas
                        }
                    );
                    if(foundOne){
                        tempList.Add(hit.position);
                    }
                }
            }
        }
        return tempList.ToArray();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if(_navPoints == null)
            Refresh();
        if(_navPoints == null) 
            throw new System.Exception("No nav points array!");

        foreach(var pt in _navPoints){
            Color color = pt.standable ? Color.green : Color.magenta;
            float lerpVal = Mathf.InverseLerp(_gizmosSpotMaxDist - _gizmosSpotRange, _gizmosSpotMaxDist, pt.closestWallDistance);
            color = Color.Lerp(Color.black, color, lerpVal);

            using(new Handles.DrawingScope(color)){
                float radius = pt.standable ? 0.5f : 0.3f;
                Handles.DrawWireDisc(pt.position, Vector3.up, radius);
            }
        }
    }
#endif

    public sealed override bool Equals(object other) => base.Equals(other);
    public sealed override int GetHashCode() => base.GetHashCode();
    public sealed override string ToString() => base.ToString();
}
