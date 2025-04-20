using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#nullable enable

[DisallowMultipleComponent]
public abstract class CharacterComponentMovementPlayer : CharacterComponentMovement
{
    private CameraControl? _camera = null;
    private NavMeshAgent? _navMeshAgent = null;

    public sealed override string DebugHeader => "Player Movement";

    public override void FillInDebugInfo(Dictionary<string, string> infoTarget)
    {
        if(this.enabled == false){
            infoTarget["Enabled"] = "Off";
            return;
        }

        infoTarget["Speed"] = $"{CurrentVelocity.magnitude:F2} m/s";

        base.FillInDebugInfo(infoTarget);
    }

    protected override void Awake(){
        base.Awake();

        _camera = FindFirstObjectByType<CameraControl>();
        if(_camera == null)
            throw new System.Exception($"Null camera");

        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if(this.Character == null)
            throw new System.Exception("Null _characterBase");
        if(this.Character.Crouch == null)
            throw new System.Exception("Null _character.Crouch");
        if(_camera == null)
            throw new System.Exception("Null _camera");

        if(_navMeshAgent != null && _navMeshAgent.enabled == true)
            throw new System.Exception("NavMeshAgent is enabled while in Player mode");

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

        float speed = GetDesiredSpeed();
        speed = Mathf.Clamp(speed, 0, GetMaxMoveSpeed());

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

    public abstract float GetDesiredSpeed();
}
