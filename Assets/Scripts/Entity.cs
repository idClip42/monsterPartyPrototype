using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

#nullable enable

[DisallowMultipleComponent]
public abstract class Entity : MonoBehaviour
{
    public delegate void DeathHandler(Entity deadEntity);
    public DeathHandler? OnDeath;

    private IDebugInfoProvider[] _debugInfoComponents = {};
    private Dictionary<string, Dictionary<string, string>> _debugDictionary = new Dictionary<string, Dictionary<string, string>>();

    private bool _alive = true;
    public bool Alive => this._alive;

    public string GetDebugInfoString()
    {
        string result = $"{this.gameObject.name} ({(this.Alive ? "Alive" : "Dead")})";

        foreach(var comp in _debugInfoComponents){
            string header = comp.DebugHeader;
            result += $"\n\n[{header}]";
            _debugDictionary[header].Clear();
            comp.FillInDebugInfo(_debugDictionary[header]);
            foreach(var el in _debugDictionary[header])
                result += $"\n    {el.Key}: {el.Value}";
        }
        
        return result;
    }

    protected virtual void Awake() {
        _debugInfoComponents = GetComponents<IDebugInfoProvider>();
        foreach(var comp in _debugInfoComponents)
            _debugDictionary.Add(comp.DebugHeader, new Dictionary<string, string>());
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        using(new Handles.DrawingScope(Color.white)){
            Handles.Label(
                transform.position + Vector3.up * 1.5f,
                GetDebugInfoString()
            );
        }
    }
#endif

    public void Kill(){
        if(_alive == false)
            throw new Exception("You cannot kill that which is already dead!");
        Debug.Log($"Entity '{gameObject.name}' has been killed.");
        _alive = false;
        OnDeath?.Invoke(this);
    }

#if UNITY_EDITOR
    public virtual void Resurrect(){
        if(_alive == true)
            throw new Exception("You cannot resurrect that which is not dead!");
        Debug.Log($"Entity '{gameObject.name}' has been resurrected.");
        _alive = true;
    }
#endif

    public sealed override bool Equals(object other) => base.Equals(other);
    public sealed override int GetHashCode() => base.GetHashCode();
    public sealed override string ToString() => base.ToString();
}
