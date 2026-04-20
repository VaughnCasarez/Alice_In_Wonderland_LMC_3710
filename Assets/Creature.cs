using UnityEngine;
using UnityEngine.InputSystem;
public class Creature : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 rotationOffset; // set by FlockManager at spawn

    private FlockManager flockManager;
    private TrailRenderer trailRenderer;

    public void Initialize(FlockManager manager)
    {
        flockManager = manager;

        velocity = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized * Random.Range(flockManager.minSpeed, flockManager.maxSpeed);

        trailRenderer = gameObject.AddComponent<TrailRenderer>();
        trailRenderer.time = 2f;
        trailRenderer.startWidth = 0.15f;
        trailRenderer.endWidth = 0.02f;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));

        Color start = new Color(1f, 1f, 1f, 0.4f);
        Color end   = new Color(1f, 1f, 1f, 0f);

        trailRenderer.startColor = start;
        trailRenderer.endColor   = end;
        trailRenderer.enabled    = false;
    }

    public void UpdateCreatures(float deltaTime)
    {
        velocity += acceleration * deltaTime;

        float speed = velocity.magnitude;
        if (speed > flockManager.maxSpeed)
            velocity = velocity.normalized * flockManager.maxSpeed;
        else if (speed < flockManager.minSpeed)
            velocity = velocity.normalized * flockManager.minSpeed;

        transform.position += velocity * deltaTime;

        Bounds();

        // Face the direction of travel, then apply the card's persistent tilt offset
        if (velocity.magnitude > 0.1f)
        {
            Quaternion travelRotation = Quaternion.LookRotation(velocity);
            Quaternion tiltOffset    = Quaternion.Euler(rotationOffset);
            transform.rotation       = travelRotation * tiltOffset;
        }

        // Spin gently around the card's local Y axis (like a tumbling card)
        transform.Rotate(0f, flockManager.CardSpinSpeed * deltaTime, 0f, Space.Self);

        acceleration = Vector3.zero;
    }

    public void Force(Vector3 force)
    {
        acceleration += force;
    }

    private void Bounds()
    {
        Vector3 pos        = transform.position;
        Vector3 boundsSize = flockManager.worldBounds;

        if (pos.x > boundsSize.x / 2)       { pos.x = boundsSize.x / 2;  velocity.x *= -1; }
        else if (pos.x < -boundsSize.x / 2) { pos.x = -boundsSize.x / 2; velocity.x *= -1; }

        if (pos.y > boundsSize.y / 2)       { pos.y = boundsSize.y / 2;  velocity.y *= -1; }
        else if (pos.y < -boundsSize.y / 2) { pos.y = -boundsSize.y / 2; velocity.y *= -1; }

        if (pos.z > boundsSize.z / 2)       { pos.z = boundsSize.z / 2;  velocity.z *= -1; }
        else if (pos.z < -boundsSize.z / 2) { pos.z = -boundsSize.z / 2; velocity.z *= -1; }

        transform.position = pos;
    }

    public void EnableTrail(bool enabled)
    {
        if (trailRenderer != null)
            trailRenderer.enabled = enabled;
    }

    public void Scatter()
    {
        Vector3 bounds = flockManager.worldBounds;
        transform.position = new Vector3(
            Random.Range(-bounds.x / 2, bounds.x / 2),
            Random.Range(-bounds.y / 2, bounds.y / 2),
            Random.Range(-bounds.z / 2, bounds.z / 2)
        );

        velocity = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized * Random.Range(flockManager.minSpeed, flockManager.maxSpeed);

        // Re-randomize the tilt on scatter for variety
        rotationOffset = new Vector3(
            Random.Range(-15f, 15f),
            Random.Range(0f, 360f),
            Random.Range(-15f, 15f)
        );
    }
}