using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    public Animator animator; // Assign this in the Inspector

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>(); // Get animator if not assigned
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayDeathAnimation();
        }
    }

    void PlayDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Death"); // Ensure you have a "Death" trigger in Animator
            StartCoroutine(DestroyAfterAnimation());
        }
        else
        {
            Destroy(gameObject); // Destroy immediately if no animation
        }
    }

    IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject); // Destroy after animation finishes
    }
}

