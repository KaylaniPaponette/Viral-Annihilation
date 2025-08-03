using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // --- OLD CODE ---
    // Audio components
    //private AudioSource source;
    //public AudioClip DeathClip;

    // --- ADDED this line ---
    [Header("Sound")]
    public int deathSfxIndex; // The index of the death sound in the SoundManager

    // Animation components
    private Animator animator;
    private bool isDead = false; // Prevent multiple triggers

    private void Awake()
    {
        //source = GetComponent<AudioSource>();   --- REMOVED this line ---
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if already dead to prevent multiple triggers
        if (isDead) return;

        // Check for player collision
        if (collision.collider.GetComponent<Player>())
        {
            enemyDie();
        }
        // Check for obstacle collision
        else if (collision.collider.CompareTag("Obstacle"))
        {
            enemyDie();
        }
        // Ignore collisions with other enemies
        else if (collision.collider.GetComponent<Enemy>() != null)
        {
            return;
        }
    }

    void enemyDie()
    {
        if (isDead) return;
        isDead = true; // Prevent multiple calls


        // --- UPDATED CODE ---
        // Play death sound through the SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(deathSfxIndex);
        }

        // Trigger death animation instead of particle system
        if (animator)
        {
            animator.SetTrigger("Death");
        }

        // Start coroutine to destroy after animation and sound
        StartCoroutine(DestroyAfterDeathSequence());
    }

    private IEnumerator DestroyAfterDeathSequence()
    {
        // Wait for the longest between animation and audio
        float destructionDelay = 0f;

        // Get animation length if animator exists
        if (animator)
        {
            // Need to wait a frame for the animation to start
            yield return null;
            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            destructionDelay = Mathf.Max(destructionDelay, animationLength);
        }

        // --- UPDATED CODE ---
        // Get audio length from the SoundManager
        if (SoundManager.Instance != null && deathSfxIndex >= 0 && deathSfxIndex < SoundManager.Instance.sfxSound.Length)
        {
            AudioClip clip = SoundManager.Instance.sfxSound[deathSfxIndex];
            destructionDelay = Mathf.Max(destructionDelay, clip.length);
        }

        // Wait for the calculated time
        yield return new WaitForSeconds(destructionDelay);

        // Destroy the enemy
        Destroy(gameObject);
    }
}


/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//public class Enemy : MonoBehaviour
//{
//    private Animator animator;
//    private bool isDead = false; // Prevent multiple triggers

//    private void Start()
//    {
//        animator = GetComponent<Animator>();
//    }

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        Player player = collision.collider.GetComponent<Player>();

//        if (!isDead && player != null)
//        {
//            Destroy(player.gameObject); // Destroy the player
//            StartCoroutine(Die());
//        }
//    }

//    private IEnumerator Die()
//    {
//        isDead = true; // Prevents multiple triggers
//        animator.SetTrigger("Death"); // Trigger death animation

//        Wait for the animation to finish

//       yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

//        Destroy(gameObject); // Destroy after animation
//    }
//}



public class Enemy : MonoBehaviour
{

    AudioSource source;
    public AudioClip DeathClip;

    public GameObject EnemyParticles;


    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //bool hasCollidedWithPlayer = collision.collider.GetComponent<Player>() != null;

        if (collision.collider.GetComponent<Player>())
        {

            enemyDie();
        }

        if (collision.collider.CompareTag("Obstacle"))
        {
            enemyDie();
        }

        if (collision.collider.GetComponent<Enemy>() != null)
        {
            return;
        }

    }

    void enemyDie()
    {
        if (source && DeathClip)
        {
            source.PlayOneShot(DeathClip); // Plays the clip without changing source.clip
        }

        Instantiate(EnemyParticles, transform.position, Quaternion.identity);
        GetComponent<SpriteRenderer>().enabled = false; // Hide the enemy

        Destroy(gameObject, DeathClip.length); // Destroy after sound finishes
    }
}
*/


