using UnityEngine;

public class FragileBlock : MonoBehaviour
{
    [Header("Optional VFX or SFX")]
    public GameObject breakEffect;
    public int breakSfxIndex;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<Player>())
        {
            Break();
        }
    }

    void Break()
    {
        // Play sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(breakSfxIndex);
        }

        // Spawn break particles
        if (breakEffect)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }

        // Destroy the block
        Destroy(gameObject);
    }
}
