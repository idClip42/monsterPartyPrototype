using UnityEditor;
using UnityEngine;

#nullable enable

public class SimpleMonsterKillZone : MonoBehaviour {
    [SerializeField]
    private SimpleMonster? _killTarget = null;

    [SerializeField]
    [Range(0.1f, 3.0f)]
    private float _killRange = 1.0f;

    private void Awake()
    {
        if(_killTarget == null)
            throw new System.Exception($"{gameObject.name} missing kill target");
    }

    private void Update()
    {
        if(_killTarget == null)
            throw new System.Exception($"{gameObject.name} missing kill target");

        if(_killTarget.Alive == false) return;

        float rangeSqr = _killRange * _killRange;
        Vector3 diff = transform.position - _killTarget.transform.position;
        float distSqr = diff.sqrMagnitude;

        if(distSqr < rangeSqr)
            _killTarget.Kill();
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        using(new Handles.DrawingScope(Color.red)){
            Handles.DrawWireDisc(
                transform.position,
                Vector3.up,
                _killRange
            );
        }
    }
#endif
}