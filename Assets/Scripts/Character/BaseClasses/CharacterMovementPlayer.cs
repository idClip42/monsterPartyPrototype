using System.Linq;
using UnityEngine;
using UnityEditor;

public abstract class CharacterMovementPlayer : MonoBehaviour
{
    private Character _characterBase;
    private CharacterCrouch _crouch;
    private CameraControl _camera;
    private IInteractible[] _interactibles;

    [SerializeField]
    private float _walkSpeed = 3.0f;
    [SerializeField]
    private float _runSpeed = 5.0f;
    [SerializeField]
    private float _interactionDistance = 1.25f;
    
    protected virtual void Awake(){
        _characterBase = GetComponent<Character>();
        if(_characterBase == null)
            throw new System.Exception($"Null character base on {this.gameObject.name}");

        _crouch = GetComponent<CharacterCrouch>();
        if(_crouch == null)
            throw new System.Exception($"Null crouch on {this.gameObject.name}");

        _camera = FindFirstObjectByType<CameraControl>();
        if(_camera == null)
            throw new System.Exception($"Null camera");
        
        _interactibles = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Where(item => item is IInteractible)
            .Select(item => item as IInteractible)
            .ToArray();
    }

    void Update()
    {
        HandleMovement();

        if(Input.GetButtonDown("Interact")){
            var interactible = GetInteractibleWithinReach();
            if(interactible != null){
                interactible.DoInteraction(_characterBase);
            }
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isRunning = Input.GetButton("Run") && !_crouch.IsCrouching;

        Vector3 camForward = Vector3.ProjectOnPlane(
            _camera.transform.forward,
            Vector3.up
        ).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(
            _camera.transform.right,
            Vector3.up
        ).normalized;

        Vector3 moveDirection = (camForward * vertical + camRight * horizontal).normalized;
        float speed = isRunning ? _runSpeed : _walkSpeed;
        float inputMagnitude = new Vector2(horizontal, vertical).magnitude;

        Move(
            moveDirection * speed * inputMagnitude,
            Time.deltaTime
        );
    }

    protected abstract void Move(Vector3 desiredMovementVelocity, float deltaTime);

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
            interactible.GetInteractionName(_characterBase)
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
