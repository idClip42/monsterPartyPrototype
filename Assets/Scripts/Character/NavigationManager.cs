using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

#nullable enable

public class NavigationManager : MonoBehaviour
{
    [SerializeField]
    private NavMeshSurface? _standingNavMesh;
    [SerializeField]
    private NavMeshSurface? _crouchingNavMesh;

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

    void Awake()
    {
        if(_standingNavMesh == null)
            throw new System.Exception("Null _standingNavMesh");
        if(_crouchingNavMesh == null)
            throw new System.Exception("Null _crouchingNavMesh");
    }
}
