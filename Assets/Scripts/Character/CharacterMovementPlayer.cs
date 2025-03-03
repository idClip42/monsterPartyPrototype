using System.Linq;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovementPlayer : MonoBehaviour
{
    private CharacterController _characterController;
    private Camera _camera;
    private IInteractible[] _interactibles;

    [SerializeField]
    private float _speed = 4.0f;

    [SerializeField]
    private float _interactionDistance = 1.25f;
    
    void Awake(){
        _characterController = GetComponent<CharacterController>();
        if(_characterController == null)
            throw new System.Exception($"Null character controller on {this.gameObject.name}");

        _camera = Camera.main;
        if(_camera == null)
            throw new System.Exception($"Null camera");
        
        _interactibles = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Where(item => item is IInteractible)
            .Select(item => item as IInteractible)
            .ToArray();
    }

    void OnEnable()
    {
        _characterController.enabled = true;
    }

    void OnDisable()
    {
        _characterController.enabled = false;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 camForward = Vector3.ProjectOnPlane(
            _camera.transform.forward,
            Vector3.up
        ).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(
            _camera.transform.right,
            Vector3.up
        ).normalized;

        _characterController.Move(
            ((camForward * vertical + camRight * horizontal) * _speed * Time.deltaTime) +
            (Physics.gravity * Time.deltaTime)
        );

        if(Input.GetButtonDown("Interact")){
            var interactible = GetInteractibleWithinReach();
            if(interactible != null){
                var charBase = GetComponent<CharacterBase>();
                if(charBase == null)
                    throw new System.Exception($"No CharacteBase on {this.gameObject.name}");
                interactible.DoInteraction(charBase);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(!enabled) return;

        var interactible = GetInteractibleWithinReach();
        if(interactible == null) return;

        Color prevColor = Handles.color;
        Handles.color = Color.white;
        Handles.Label(
            interactible.InteractionWorldPosition,
            interactible.InteractionName
        );
        Handles.color = prevColor;
    }
#endif

    private IInteractible GetInteractibleWithinReach(){
        if(_interactibles == null) return null;

        Vector3 refPos = this.transform.position + Vector3.up * 1.0f;
        IInteractible closest = null;
        float closestDistSq = float.MaxValue;

        foreach (var interactible in _interactibles)
        {
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
