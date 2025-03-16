using UnityEngine;

#nullable enable

[DisallowMultipleComponent]
public abstract class CharacterMovementPlayer : MonoBehaviour, IDebugInfoProvider
{
    private Character? _characterBase = null;
    private CharacterCrouch? _crouch = null;
    private CameraControl? _camera = null;

    [SerializeField]
    private float _walkSpeed = 3.0f;
    [SerializeField]
    private float _runSpeed = 5.0f;

    public string DebugName => "Player Movement";
    public string DebugInfo { get {
        if(this.enabled == false) return "Off";
        return "On";
    }}

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

    private void Update()
    {
        if(_crouch == null) throw new System.Exception("Null _crouch");
        if(_camera == null) throw new System.Exception("Null _camera");

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
