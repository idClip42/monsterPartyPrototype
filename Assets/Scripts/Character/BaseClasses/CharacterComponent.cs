using System.Collections.Generic;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(Character))]
public abstract class CharacterComponent : MonoBehaviour, IDebugInfoProvider
{
    private Character? _character = null;
    public Character? Character => _character;

    public abstract string DebugHeader { get; }
    public abstract void FillInDebugInfo(Dictionary<string, string> infoTarget);

    protected virtual void Awake()
    {
        _character = GetComponent<Character>();
        if(_character == null)
            throw new System.Exception($"Null character on {this.gameObject.name}");
    }
}
