using System.Collections.Generic;
using UnityEngine;

#nullable enable

[DisallowMultipleComponent]
public abstract class CharacterMovementPlayer : CharacterMovement, IDebugInfoProvider
{
    private Character? _character = null;
    private CharacterCrouch? _crouch = null;
    private CameraControl? _camera = null;

    public string DebugHeader => "Player Movement";

    public void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        if(this.enabled == false){
            infoTarget["Enabled"] = "Off";
            return;
        }

        infoTarget["Speed"] = $"{CurrentVelocity.magnitude:F2} m/s";
    }

    protected virtual void Awake(){
        _character = GetComponent<Character>();
        if(_character == null)
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
        if(_character == null)
            throw new System.Exception("Null _characterBase");
        if(_crouch == null)
            throw new System.Exception("Null _crouch");
        if(_camera == null)
            throw new System.Exception("Null _camera");

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool runButtonDown = Input.GetButton("Run");

        Vector3 camForward = Vector3.ProjectOnPlane(
            _camera.transform.forward,
            Vector3.up
        ).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(
            _camera.transform.right,
            Vector3.up
        ).normalized;

        float speed;
        if(_crouch.IsCrouching)
            speed = _character.Movement.CrouchSpeed;
        else if(runButtonDown)
            speed = _character.Movement.RunSpeed;
        else
            speed = _character.Movement.WalkSpeed;

        // TODO: This isn't a perfect solution.
        // TODO: Surely there's a real solution out there somewhere.
        Vector3 input = camForward * vertical + camRight * horizontal;
        Vector3 clampedInput = Vector3.ClampMagnitude(input, 1);
        Vector3 desiredVelocity = clampedInput * speed;

        Move(
            desiredVelocity,
            Time.deltaTime
        );
    }

    protected abstract void Move(Vector3 desiredMovementVelocity, float deltaTime);
}
