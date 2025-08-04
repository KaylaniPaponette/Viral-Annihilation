using UnityEngine;

public class StickyBlock : MonoBehaviour
{
    [Tooltip("Only stick objects with this tag (leave empty to stick everything)")]
    public string stickyTagFilter = "";

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D otherRb = collision.collider.attachedRigidbody;

        if (otherRb != null && CanStick(collision.collider))
        {
            // Only attach if not already stuck
            if (collision.collider.GetComponent<FixedJoint2D>() == null)
            {
                FixedJoint2D joint = collision.collider.gameObject.AddComponent<FixedJoint2D>();
                joint.connectedBody = GetComponent<Rigidbody2D>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = transform.InverseTransformPoint(collision.transform.position);
                joint.enableCollision = true; // Optional: allow collisions between connected objects
            }
        }
    }

    private bool CanStick(Collider2D collider)
    {
        return string.IsNullOrEmpty(stickyTagFilter) || collider.CompareTag(stickyTagFilter);
    }
}
