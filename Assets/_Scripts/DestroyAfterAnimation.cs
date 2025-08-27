// ===== DestroyAfterAnimation.cs =====
using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    void Start()
    {
        // Get the length of the animation clip currently playing.
        float animationLength = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;

        // Destroy this GameObject after the animation has finished.
        Destroy(gameObject, animationLength);
    }
}