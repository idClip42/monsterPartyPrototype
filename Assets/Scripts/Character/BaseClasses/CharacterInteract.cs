using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

#nullable enable

[DisallowMultipleComponent]
public abstract class CharacterInteract : MonoBehaviour, IDebugInfoProvider
{
    private Character? _characterBase = null;
    private IInteractible?[] _interactibles = {};
    private IInteractible? _interactibleWithinReach = null;

    [SerializeField]
    private float _interactionDistance = 1.25f;

    public string DebugHeader => "Interaction";

    public void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        infoTarget["Within Reach"] = _interactibleWithinReach != null ?
            _interactibleWithinReach.gameObject.name :
            "None";
    }

    private void Awake()
    {
        _characterBase = GetComponent<Character>();
        if(_characterBase == null)
            throw new System.Exception($"Null character base on {this.gameObject.name}");
        
        _interactibles = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Where(item => item != null && item is IInteractible)
            .Select(item => item as IInteractible)
            .ToArray();
    }

    private void Update()
    {
        if(_characterBase == null) throw new System.Exception("Null _characterBase");

        if(_characterBase.State == Character.StateType.Player){
            _interactibleWithinReach = GetInteractibleWithinReach();
            if(Input.GetButtonDown("Interact")){
                if(_interactibleWithinReach != null){
                    _interactibleWithinReach.DoInteraction(_characterBase);
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(!enabled) return;
        if(_characterBase == null) return;
        if(_characterBase.State != Character.StateType.Player) return;
        if(_interactibleWithinReach == null) return;

        using(new Handles.DrawingScope(Color.white)){
            Handles.Label(
                _interactibleWithinReach.InteractionWorldPosition + Vector3.up * 1.0f,
                _interactibleWithinReach.GetInteractionName(_characterBase)
            );
        }
    }
#endif

    private IInteractible? GetInteractibleWithinReach(){
        Vector3 refPos = this.transform.position + Vector3.up * 1.0f;
        IInteractible? closest = null;
        float closestDistSq = float.MaxValue;

        foreach (var interactible in _interactibles)
        {
            if(interactible == null) continue;
            if(interactible.gameObject == this.gameObject) continue;
            if(interactible.IsInteractible == false) continue;
            Vector3 posDiff = interactible.InteractionWorldPosition - refPos;
            float distSq = posDiff.sqrMagnitude;
            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closest = interactible;
            }
        }

        if(Mathf.Sqrt(closestDistSq) > _interactionDistance)
            return null;

        return closest;
    }
}
