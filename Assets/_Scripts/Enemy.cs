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
//        PlayerController player = collision.collider.GetComponent<PlayerController>();

//        if (!isDead && player != null)
//        {
//            //Destroy(player.gameObject); // Destroy the player
//            StartCoroutine(Die());
//        }
//    }

//    private IEnumerator Die()
//    {
//        isDead = true; // Prevents multiple triggers
//        animator.SetTrigger("Death"); // Trigger death animation

//        // Wait for the animation to finish
//        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

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



