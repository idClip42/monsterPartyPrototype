using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

#nullable enable

public abstract class Entity : MonoBehaviour
{
    private IDebugInfoProvider[] _debugInfoComponents = {};

    protected virtual void Awake() {
        _debugInfoComponents = GetComponents<IDebugInfoProvider>();
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        Color prevColor = Handles.color;
        Handles.color = Color.white;

        string text = @$"
{this.gameObject.name}
{String.Join('\n', _debugInfoComponents.Select(c=>$"{c.DebugName}: {c.DebugInfo}"))}
        ".Trim();

        Handles.Label(
            transform.position + Vector3.up * 1f,
            text
        );
        Handles.color = prevColor;
    }
#endif
}
