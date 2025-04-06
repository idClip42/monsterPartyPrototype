using UnityEngine;

#nullable enable

[RequireComponent(typeof(CharacterController))]
public class KickOnCollision : MonoBehaviour
{
    [Tooltip("Force to apply to the object when kicked.")]
    public float kickForce = 2f;

    [Tooltip("Angle above the horizontal (in degrees) to kick the object.")]
    public float kickAngle = 30f;

    [Tooltip("Layers of objects that can be kicked.")]
    public LayerMask kickableLayers = Physics.AllLayers;

    private CharacterController? controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if the object is on the kickable layers
        if ((kickableLayers.value & (1 << hit.gameObject.layer)) == 0)
            return;

        Rigidbody rb = hit.collider.attachedRigidbody;

        if(rb == null) return;
        if(rb.isKinematic) return;
        if(rb.GetComponent<ICarryable>() != null) return;

        Vector3 direction = hit.collider.transform.position - transform.position;

        // Project on the ground plane (XZ)
        direction.y = 0f;
        direction.Normalize();

        // Rotate the direction upwards by the kickAngle
        Quaternion tilt = Quaternion.AngleAxis(-kickAngle, Vector3.Cross(Vector3.up, direction));
        Vector3 kickDirection = tilt * direction;

        // Apply the force
        rb.AddForce(kickDirection * kickForce, ForceMode.Impulse);

        Debug.DrawLine(
            rb.transform.position,
            rb.transform.position + kickDirection * kickForce,
            Color.yellow,
            0.5f
        );
    }
}
