using UnityEngine;
using UnityEditor;

#nullable enable

[DisallowMultipleComponent]
public abstract class CharacterNoiseLevel : MonoBehaviour, IDebugInfoProvider
{
    private Character? _character = null;
    public Character? Character => _character;

    public abstract float CurrentNoiseRadius { get; }

    public string DebugName => "Noise";
    public string DebugInfo => $"{CurrentNoiseRadius}m";

    protected virtual void Awake(){
        _character = GetComponent<Character>();
        if(_character == null)
            throw new System.Exception($"Null character on {this.gameObject.name}");
    }

#if UNITY_EDITOR
    protected virtual private void OnDrawGizmos() {
        Color prevColor = Handles.color;
        Handles.color = Color.cyan;
        
        const float ARC_DEGREES = 80;
        const int INTERVAL = 5;

        if(CurrentNoiseRadius > 0){
            Vector3 origin = transform.position;
            Vector3[] directions = new Vector3[]{
                Vector3.forward,
                Vector3.right,
                Vector3.back,
                Vector3.left
            };
            foreach(var dir in directions){
                for(int i = INTERVAL; i < CurrentNoiseRadius; i += INTERVAL){
                    Handles.DrawWireArc(
                        origin,
                        Vector3.up,
                        Quaternion.Euler(0, -ARC_DEGREES/2, 0) * dir,
                        ARC_DEGREES,
                        i
                    );
                }
                Handles.DrawWireArc(
                    origin,
                    Vector3.up,
                    Quaternion.Euler(0, -ARC_DEGREES/2, 0) * dir,
                    ARC_DEGREES,
                    CurrentNoiseRadius
                );
            }    
        }

        Handles.color = prevColor;
    }
#endif
}