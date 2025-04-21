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
            throw new MonsterPartyNullReferenceException(this, $"_character");
    }

    public sealed override bool Equals(object other) => base.Equals(other);
    public sealed override int GetHashCode() => base.GetHashCode();
    public sealed override string ToString() => base.ToString();
}
