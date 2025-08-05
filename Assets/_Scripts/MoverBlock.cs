using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoverBlock : MonoBehaviour
{
    public enum MoveDirection { Horizontal, Vertical }

    [Header("Movement Settings")]
    public MoveDirection direction = MoveDirection.Horizontal;
    public float travelDistance = 3f;
    public float speed = 2f;

    private Vector2 startPoint;
    private Vector2 targetPoint;
    private bool movingToTarget = true;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Prevent physics forces from affecting the object

        startPoint = rb.position;

        if (direction == MoveDirection.Horizontal)
            targetPoint = startPoint + Vector2.right * travelDistance;
        else
            targetPoint = startPoint + Vector2.up * travelDistance;
    }

    void FixedUpdate()
    {
        Vector2 currentTarget = movingToTarget ? targetPoint : startPoint;
        Vector2 newPosition = Vector2.MoveTowards(rb.position, currentTarget, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition); // Moves along physics path, but immune to forces

        if (Vector2.Distance(rb.position, currentTarget) < 0.01f)
            movingToTarget = !movingToTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 previewStart = Application.isPlaying ? startPoint : (Vector2)transform.position;
        Vector2 previewEnd = direction == MoveDirection.Horizontal
            ? previewStart + Vector2.right * travelDistance
            : previewStart + Vector2.up * travelDistance;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(previewStart, previewEnd);
        Gizmos.DrawSphere(previewStart, 0.1f);
        Gizmos.DrawSphere(previewEnd, 0.1f);
    }
}
