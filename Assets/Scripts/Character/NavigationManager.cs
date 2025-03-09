using UnityEngine;
using UnityEngine.AI;

#nullable enable

public class NavigationManager : MonoBehaviour
{
    [SerializeField]
    private int _standingAgentTypeIndex = 0;
    [SerializeField]
    private int _crouchingAgentTypeIndex = 1;

    private int _standingAgentTypeId = -1;
    private int _crouchingAgentTypeId = -1;

    public int StandingAgentTypeId => _standingAgentTypeId;
    public int CrouchingAgentTypeId => _crouchingAgentTypeId;

    void Awake()
    {
        _standingAgentTypeId = NavMesh.GetSettingsByIndex(_standingAgentTypeIndex).agentTypeID;
        _crouchingAgentTypeId = NavMesh.GetSettingsByIndex(_crouchingAgentTypeIndex).agentTypeID;
    }
}
