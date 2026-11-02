using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] public Animator animator;         
    [SerializeField] public Transform AttackPoint;     
    [SerializeField] public LayerMask enemyLayers;     

    [SerializeField] public float attackRange = 2f;  
    [SerializeField] public int attackDamage = 40;    
    
    [SerializeField] public float attackRate = 2f;    
    [SerializeField] private float nextAttackTime = 0f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
            {
            Attack();
            nextAttackTime = Time.time + 1f / attackRate;
            }
    }

    void Attack()
    {
       if (animator != null)
        {
            animator.ResetTrigger("Attack1");
            animator.SetTrigger("Attack1");
        }

        if (AttackPoint == null)
            return;
       
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(AttackPoint.position, attackRange, enemyLayers);

       
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
        }
    }
}