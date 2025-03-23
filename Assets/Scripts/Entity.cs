using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

#nullable enable

[DisallowMultipleComponent]
public abstract class Entity : MonoBehaviour
{
    public delegate void DeathHandler(Entity deadEntity);
    public DeathHandler? OnDeath;

    private IDebugInfoProvider[] _debugInfoComponents = {};

    private bool _alive = true;
    public bool Alive => this._alive;

    public string DebugInfoString => @$"
{this.gameObject.name} ({(this.Alive ? "Alive" : "Dead")})
{String.Join('\n', _debugInfoComponents.Select(c=>$"{c.DebugName}: {c.DebugInfo}"))}
        ".Trim();

    protected virtual void Awake() {
        _debugInfoComponents = GetComponents<IDebugInfoProvider>();
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        using(new Handles.DrawingScope(Color.white)){
            Handles.Label(
                transform.position + Vector3.up * 1f,
                DebugInfoString
            );
        }
    }
#endif

    protected void Die(){
        if(_alive == false)
            throw new Exception("You cannot kill that which is already dead!");
        _alive = false;
        OnDeath?.Invoke(this);
    }
}
