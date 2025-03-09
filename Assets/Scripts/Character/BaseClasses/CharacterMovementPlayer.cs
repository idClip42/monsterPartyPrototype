using System.Linq;
using UnityEngine;
using UnityEditor;

public abstract class CharacterMovementPlayer : MonoBehaviour
{
    private Character _characterBase;
    private CharacterCrouch _crouch;
    private CameraControl _camera;

    [SerializeField]
    private float _walkSpeed = 3.0f;
    [SerializeField]
    private float _runSpeed = 5.0f;
    
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
    }

    void Update()
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
}
