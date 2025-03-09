using UnityEngine;
using UnityEngine.XR;

# nullable enable

public class SimpleMonster : MonoBehaviour
{
    [SerializeField]
    private Light? _eye;
    private Character[] _characters = {};

    void Awake()
    {
        _characters = FindObjectsByType<Character>(FindObjectsSortMode.None);

        if(_eye == null)
            throw new System.Exception("Missing eye.");
    }

    void Update()
    {
        if(_eye == null)
            throw new System.Exception("Missing eye.");

        RaycastHit hitInfo;
        Vector3 offset = Vector3.up * 1.5f;
        foreach(var target in _characters){

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
            if(!lineOfSight && hitInfo.collider.gameObject == target.gameObject)
                lineOfSight = true;

            if(lineOfSight){
                Debug.DrawLine(
                    _eye.transform.position,
                    targetPos,
                    Color.red
                );
            }
            else {
                Debug.DrawLine(
                    _eye.transform.position,
                    hitInfo.point,
                    Color.white
                );
            }
        }
    }
}
