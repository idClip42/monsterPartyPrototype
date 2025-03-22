using UnityEngine;
using UnityEditor;

#nullable enable

[DisallowMultipleComponent]
public abstract class CharacterNoiseLevel : MonoBehaviour, IDebugInfoProvider
{
    private Character? _character = null;
    protected Character? Character => _character;

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
        // TODO: Do something fun with multiple arcs instead of just a disc

        Color prevColor = Handles.color;
        Handles.color = Color.white;
        Handles.DrawWireDisc(
            transform.position,
            Vector3.up,
            CurrentNoiseRadius
        );
        Handles.color = prevColor;
    }
#endif
}