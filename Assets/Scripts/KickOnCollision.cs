using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

[RequireComponent(typeof(CharacterController))]
public class KickOnCollision : MonoBehaviour
{
    [Tooltip("Force to apply to the object when kicked.")]
    public float _kickForce = 10f;

    [Tooltip("Angle above the horizontal (in degrees) to kick the object.")]
    public float _kickAngle = 30f;

    [Tooltip("Delay before an object can be kicked again.")]
    public float _kickDelay = 0.2f;

    [Tooltip("Layers of objects that can be kicked.")]
    public LayerMask _kickableLayers = Physics.AllLayers;

    private CharacterController? _controller;

    private List<Rigidbody> _objectsToNotKick = new List<Rigidbody>();

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if the object is on the kickable layers
        if ((_kickableLayers.value & (1 << hit.gameObject.layer)) == 0)
            return;

        Rigidbody rb = hit.collider.attachedRigidbody;

        if(rb == null) return;
        if(rb.isKinematic) return;
        if(_objectsToNotKick.Contains(rb)) return;

        Vector3 direction = hit.collider.transform.position - transform.position;

        // Project on the ground plane (XZ)
        direction.y = 0f;
        direction.Normalize();

        // Rotate the direction upwards by the kickAngle
        Quaternion tilt = Quaternion.AngleAxis(-_kickAngle, Vector3.Cross(Vector3.up, direction));
        Vector3 kickDirection = tilt * direction;

        // Apply the force
        rb.AddForce(kickDirection * _kickForce, ForceMode.Impulse);

        this._objectsToNotKick.Add(rb);

        Debug.DrawLine(
            rb.transform.position,
            rb.transform.position + kickDirection * _kickForce,
            Color.yellow,
            0.5f
        );

        StartCoroutine(RemoveKickedObjectAfterDelay(rb, this._kickDelay));
    }

    private IEnumerator RemoveKickedObjectAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Remove the object from the list
        this._objectsToNotKick.Remove(rb);

        // Debug.Log("Removed kicked object from the list.");
    }

}
