using UnityEngine;
using UnityEditor;
using System.Linq;

#nullable enable

public abstract class CharacterInteract : MonoBehaviour, ICharacterComponent
{
    private Character? _characterBase = null;
    private IInteractible?[] _interactibles = {};

    [SerializeField]
    private float _interactionDistance = 1.25f;

    public string DebugName => "Interaction";
    public string DebugInfo { get {
        IInteractible? interactible = GetInteractibleWithinReach();
        if(interactible == null) return "None";
        return interactible.gameObject.name;
    }}

    void Awake()
    {
        _characterBase = GetComponent<Character>();
        if(_characterBase == null)
            throw new System.Exception($"Null character base on {this.gameObject.name}");
        
        _interactibles = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Where(item => item != null && item is IInteractible)
            .Select(item => item as IInteractible)
            .ToArray();
    }

    void Update()
    {
        if(_characterBase == null) throw new System.Exception("Null _characterBase");
        if(_characterBase.State == State.Player){
            if(Input.GetButtonDown("Interact")){
                var interactible = GetInteractibleWithinReach();
                if(interactible != null){
                    interactible.DoInteraction(_characterBase);
                }
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(!enabled) return;
        if(_characterBase == null) return;

        var interactible = GetInteractibleWithinReach();
        if(interactible == null) return;

        Color prevColor = Handles.color;
        Handles.color = Color.white;
        Handles.Label(
            interactible.InteractionWorldPosition,
            interactible.GetInteractionName(_characterBase)
        );
        Handles.color = prevColor;
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
