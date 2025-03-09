using UnityEngine;
using UnityEngine.AI;

# nullable enable

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleMonster : MonoBehaviour
{
    [SerializeField]
    private Light? _eye;

    [SerializeField]
    [Range(10, 100)]
    private float _minRedirectTime = 20;
    [SerializeField]
    [Range(10, 100)]
    private float _maxRedirectTime = 60;

    private Character[] _characters = {};

    private NavigationManager? _navManager = null;
    private NavMeshAgent? _navMeshAgent = null;

    void Awake()
    {
        _characters = FindObjectsByType<Character>(FindObjectsSortMode.None);

        if (_eye == null)
            throw new System.Exception("Missing eye.");

        _navManager = FindFirstObjectByType<NavigationManager>();
        if (_navManager == null)
            throw new System.Exception($"Null _navManager on {this.gameObject.name}");

        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        if (_minRedirectTime > _maxRedirectTime)
            throw new System.Exception("Invalid redirect times");
    }

    void Start()
    {
        NewDestination();
    }

    void Update()
    {
        if (_eye == null)
            throw new System.Exception("Missing eye.");

        RaycastHit hitInfo;
        Vector3 offset = Vector3.up * 1.5f;
        foreach (var target in _characters)
        {
            Vector3 targetPos = target.transform.position + offset;
            Vector3 toTarget = targetPos - _eye.transform.position;
            float distance = toTarget.magnitude;

            // If we hit nothing, we have a clear line of sight
            // to the target character.
            bool lineOfSight = !Physics.Raycast(
                _eye.transform.position,
                toTarget / distance,
                out hitInfo,
                distance
            );

            // If we hit something
            // and what we hit was the target character,
            // still a clear line of sight.
            if (!lineOfSight && hitInfo.collider.gameObject == target.gameObject)
                lineOfSight = true;

            if (lineOfSight)
            {
                Vector3 lightDirection = _eye.transform.forward;
                Vector3 targetDirection = toTarget / distance;
                float spotAngle = _eye.spotAngle;

                float spotAngleInRadians = spotAngle * Mathf.Deg2Rad;
                float cosHalfSpotAngle = Mathf.Cos(spotAngleInRadians / 2);
                float dotProduct = Vector3.Dot(lightDirection, targetDirection);
                if (dotProduct > cosHalfSpotAngle)
                {
                    // Target is within the vision cone
                    Debug.DrawLine(
                        _eye.transform.position,
                        targetPos,
                        Color.red
                    );
                }
                else
                {
                    // Target is outside the vision cone
                    Debug.DrawLine(
                        _eye.transform.position,
                        targetPos,
                        Color.yellow
                    );
                }
            }
            else
            {
                Debug.DrawLine(
                    _eye.transform.position,
                    hitInfo.point,
                    Color.white
                );
            }
        }
    }

    private void NewDestination()
    {
        if (_navManager == null)
            throw new System.Exception($"Null _navManager on {this.gameObject.name}");
        if (_navMeshAgent == null)
            throw new System.Exception($"Null nav mesh agent on {this.gameObject.name}");

        _navMeshAgent.SetDestination(
            _navManager.GetRandomDestinationStanding()
        );
    }
}
