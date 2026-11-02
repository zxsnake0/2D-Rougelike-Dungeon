using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth = 5;
    public float knockback = 6f;

    [SerializeField] public Animator animator;  


     private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }


    public void TakeDamage(int amount)
    {
        if (animator != null)
        {
            animator.ResetTrigger("Hurt");
            animator.SetTrigger("Hurt");
        }

        Rigidbody2D playerRb = gameObject.GetComponent<Rigidbody2D>();

        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);

        playerRb.AddForce(transform.position * knockback, ForceMode2D.Impulse);

        currentHealth -= amount;
        
        if (currentHealth <= 0)
        {
            Debug.Log("Player has died!");
            
            Destroy(gameObject);
        }
    }
}
