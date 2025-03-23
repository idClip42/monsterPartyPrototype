using System.Collections.Generic;
using UnityEngine;

#nullable enable

[DisallowMultipleComponent]
public abstract class CharacterMovementPlayer : CharacterMovement
{
    private CameraControl? _camera = null;

    public override string DebugHeader => "Player Movement";

    public override void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        if(this.enabled == false){
            infoTarget["Enabled"] = "Off";
            return;
        }

        infoTarget["Speed"] = $"{CurrentVelocity.magnitude:F2} m/s";
    }

    protected override void Awake(){
        base.Awake();

        _camera = FindFirstObjectByType<CameraControl>();
        if(_camera == null)
            throw new System.Exception($"Null camera");
    }

    private void Update()
    {
        if(this.Character == null)
            throw new System.Exception("Null _characterBase");
        if(this.Character.Crouch == null)
            throw new System.Exception("Null _character.Crouch");
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
        if(this.Character.Crouch.IsCrouching)
            speed = this.Character.Movement.CrouchSpeed;
        else if(runButtonDown)
            speed = this.Character.Movement.RunSpeed;
        else
            speed = this.Character.Movement.WalkSpeed;

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
