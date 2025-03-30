using UnityEditor;
using UnityEngine;

#nullable enable

public class DoorOpener : MonoBehaviour
{
    [SerializeField]
    [Range(0.1f, 2)]
    private float _range = 1;
    private Door?[] _doors = {};

    private void Awake() {
        _doors = FindObjectsByType<Door>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        float sqrThreshold = _range * _range;
        foreach(var door in _doors){
            if(door == null)
                throw new System.Exception("Null door");
            if(door.IsOpen == true)
                continue;
            Vector3 diff = this.transform.position - door.transform.position;
            float sqrMag = diff.sqrMagnitude;
            if(sqrMag < sqrThreshold)
                door.OpenDoor(this.transform);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        using(new Handles.DrawingScope()){
            Handles.DrawWireDisc(
                transform.position,
                Vector3.up,
                _range
            );
        }
    }
#endif
}
