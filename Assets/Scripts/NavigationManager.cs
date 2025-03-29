using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

#nullable enable

[DisallowMultipleComponent]
public class NavigationManager : MonoBehaviour
{
    private const float INTERVAL = 3;

    [SerializeField]
    private NavMeshSurface? _standingNavMesh;
    [SerializeField]
    private NavMeshSurface? _crouchingNavMesh;

    private Vector3[]? _standingNavPoints = null;
    private Vector3[]? _crouchingNavPoints = null;

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

    public int StandingNavPointsCount => _standingNavPoints != null ? _standingNavPoints.Length : -1;
    public int CrouchingNavPointsCount => _crouchingNavPoints != null ? _crouchingNavPoints.Length : -1;

    private void Awake()
    {
        Refresh();
    }

    public void Refresh(){
        if(_standingNavMesh == null)
            throw new System.Exception("Null _standingNavMesh");
        if(_crouchingNavMesh == null)
            throw new System.Exception("Null _crouchingNavMesh");

        _standingNavPoints = GetPossiblePointsOnNavMesh(_standingNavMesh);
        _crouchingNavPoints = GetPossiblePointsOnNavMesh(_crouchingNavMesh);
    }

    public Vector3 GetRandomDestinationStanding(){
        if(_standingNavPoints == null) throw new System.Exception("No standing nav points array!");
        if(_standingNavPoints.Length == 0) throw new System.Exception("No standing nav points!");
        return _standingNavPoints[Random.Range(0, _standingNavPoints.Length)];
    }

    public Vector3 GetRandomDestinationCrouching(){
        if(_crouchingNavPoints == null) throw new System.Exception("No crouching nav points array!");
        if(_crouchingNavPoints.Length == 0) throw new System.Exception("No crouching nav points!");
        return _crouchingNavPoints[Random.Range(0, _crouchingNavPoints.Length)];
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
        if(_standingNavPoints == null || _crouchingNavPoints == null)
            Refresh();

        if(_standingNavPoints == null) throw new System.Exception("No standing nav points array!");
        if(_crouchingNavPoints == null) throw new System.Exception("No crouching nav points array!");

        using(new Handles.DrawingScope(Color.green)){
            foreach(var pt in _standingNavPoints){
                Handles.DrawWireDisc(pt, Vector3.up, 0.5f);
            }
        }
        using(new Handles.DrawingScope(Color.magenta)){
            foreach(var pt in _crouchingNavPoints){
                Handles.DrawWireDisc(pt, Vector3.up, 0.25f);
            }
        }
    }
#endif

    public sealed override bool Equals(object other) => base.Equals(other);
    public sealed override int GetHashCode() => base.GetHashCode();
    public sealed override string ToString() => base.ToString();
}
