using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Sound")]
    public int deathSfxIndex;

    private Animator animator;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // Check for player
        if (other.GetComponent<Player>())
        {
            enemyDie();
        }
        // Check for obstacle
        else if (other.CompareTag("Obstacle"))
        {
            enemyDie();
        }
        // Ignore other enemies
        else if (other.GetComponent<Enemy>() != null)
        {
            return;
        }
    }

    void enemyDie()
    {
        if (isDead) return;
        isDead = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(deathSfxIndex);
        }

        if (animator)
        {
            animator.SetTrigger("Death");
        }

        StartCoroutine(DestroyAfterDeathSequence());
    }

    private IEnumerator DestroyAfterDeathSequence()
    {
        float destructionDelay = 0f;

        if (animator)
        {
            yield return null;
            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            destructionDelay = Mathf.Max(destructionDelay, animationLength);
        }

        if (SoundManager.Instance != null && deathSfxIndex >= 0 && deathSfxIndex < SoundManager.Instance.sfxSound.Length)
        {
            AudioClip clip = SoundManager.Instance.sfxSound[deathSfxIndex];
            destructionDelay = Mathf.Max(destructionDelay, clip.length);
        }

        yield return new WaitForSeconds(destructionDelay);
        Destroy(gameObject);
    }
}
