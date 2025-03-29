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
        public readonly Vector3 position;
        public readonly bool standable;
        public readonly bool crouchable;

        public NavSpot(Vector3 position, bool standable, bool crouchable){
            this.position = position;
            this.standable = standable;
            this.crouchable = crouchable;
        }
    }

    private const float INTERVAL = 3;

    [SerializeField]
    private NavMeshSurface? _standingNavMesh;
    [SerializeField]
    private NavMeshSurface? _crouchingNavMesh;

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
                crouchingNavPoints.Contains(pt)
            )
        ).ToList();
        tempNavPoints.AddRange(
            crouchingNavPoints
                .Where(pt => !standingNavPoints.Contains(pt))
                .Select(pt => new NavSpot( pt, false, true ))
        );

        _navPoints = tempNavPoints.ToArray();
    }

    public Vector3 GetRandomDestination(bool excludeStanding, bool excludeCrouching){
        if(_navPoints == null) 
            throw new System.Exception("No nav points array!");
        var filteredPoints = _navPoints.Where(pt => {
            if(excludeStanding && pt.standable) return false;
            if(excludeCrouching && pt.crouchable) return false;
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
        for(float x = bounds.min.x; x <= bounds.max.x; x += INTERVAL){
            for(float y = bounds.min.y; y <= bounds.max.y; y += INTERVAL){
                for(float z = bounds.min.z; z <= bounds.max.z; z += INTERVAL){
                    Vector3 testPosition = new Vector3(x,y,z);
                    bool foundOne = NavMesh.SamplePosition(
                        testPosition,
                        out hit,
                        INTERVAL / 2,
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
            float radius = pt.standable ? 0.5f : 0.3f;
            // TODO: Color strength or line thickness based on distance from walls?
            using(new Handles.DrawingScope(color)){
                Handles.DrawWireDisc(pt.position, Vector3.up, radius);
            }
        }
    }
#endif

    public sealed override bool Equals(object other) => base.Equals(other);
    public sealed override int GetHashCode() => base.GetHashCode();
    public sealed override string ToString() => base.ToString();
}
