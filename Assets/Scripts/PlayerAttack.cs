using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Attack Points")]
    [SerializeField] private Transform horizontalAttackPoint;
    [SerializeField] private Transform upAttackPoint;
    [SerializeField] private Transform downAttackPoint;

    [Header("Attack Settings")]
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int attackDamage = 40;
    [SerializeField] private float attackRate = 2f;

    [Header("Pogo Settings")]
    [SerializeField] private float pogoForce = 12f;

    [Header("Attack Visuals")]
    [SerializeField] private SpriteRenderer upAttackVisual;
    [SerializeField] private SpriteRenderer downAttackVisual;
    [SerializeField] private float visualDuration = 0.2f;

    private float nextAttackTime;

    // Direction saved when the attack starts
    private int attackDirection = 1;

    // Visual effect coroutines
    private Coroutine upVisualCoroutine;
    private Coroutine downVisualCoroutine;


    // --------------------------------
    // INITIALIZATION
    // --------------------------------

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        // Hide attack visuals when the game starts
        if (upAttackVisual != null)
            upAttackVisual.enabled = false;

        if (downAttackVisual != null)
            downAttackVisual.enabled = false;
    }


    // --------------------------------
    // INPUT
    // --------------------------------

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.eKey.wasPressedThisFrame)
            return;

        if (Time.time < nextAttackTime)
            return;

        // Save the direction when the attack begins
        attackDirection = playerMovement.IsFacingRight ? 1 : -1;

        // Determine attack direction
        if (Keyboard.current.upArrowKey.isPressed)
        {
            AttackUp();
        }
        else if (Keyboard.current.downArrowKey.isPressed)
        {
            AttackDown();
        }
        else
        {
            AttackHorizontal();
        }

        nextAttackTime = Time.time + 1f / attackRate;
    }


    // --------------------------------
    // HORIZONTAL ATTACK
    // --------------------------------

    private void AttackHorizontal()
    {
        if (animator != null)
            animator.SetTrigger("Attack1");

        if (horizontalAttackPoint == null)
            return;

        // Calculate attack position independently
        // of the player's flipping
        Vector2 attackPosition = new Vector2(
            transform.position.x +
            Mathf.Abs(horizontalAttackPoint.localPosition.x) * attackDirection,

            transform.position.y +
            horizontalAttackPoint.localPosition.y
        );

        DamageEnemies(attackPosition, false);
    }


    // --------------------------------
    // UPWARD ATTACK
    // --------------------------------

    private void AttackUp()
    {
        if (upAttackPoint == null)
            return;

        if (animator != null)
            animator.SetTrigger("AttackUp");

        // Display upward attack visual
        upVisualCoroutine = ShowAttackVisual(
            upAttackVisual,
            upVisualCoroutine
        );

        DamageEnemies(upAttackPoint.position, false);
    }


    // --------------------------------
    // DOWNWARD ATTACK
    // --------------------------------

    private void AttackDown()
    {
        // Only allow downward attacks in the air
        if (playerMovement.IsPlayerGrounded())
            return;

        if (downAttackPoint == null)
            return;

        if (animator != null)
            animator.SetTrigger("AttackDown");

        // Display downward attack visual
        downVisualCoroutine = ShowAttackVisual(
            downAttackVisual,
            downVisualCoroutine
        );

        DamageEnemies(downAttackPoint.position, true);
    }


    // --------------------------------
    // ATTACK VISUALS
    // --------------------------------

    private Coroutine ShowAttackVisual(
        SpriteRenderer visual,
        Coroutine currentRoutine)
    {
        if (visual == null)
            return null;

        // Stop previous coroutine if necessary
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        // Show sprite
        visual.enabled = true;

        // Start disappearance timer
        return StartCoroutine(HideVisualAfterDelay(visual));
    }

    private IEnumerator HideVisualAfterDelay(SpriteRenderer visual)
    {
        yield return new WaitForSeconds(visualDuration);

        if (visual != null)
            visual.enabled = false;
    }


    // --------------------------------
    // ENEMY DETECTION AND DAMAGE
    // --------------------------------

    private void DamageEnemies(Vector2 attackPosition, bool canPogo)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPosition,
            attackRange,
            enemyLayers
        );

        // Prevent damaging the same enemy multiple times
        // if it has multiple colliders
        HashSet<EnemyHealth> damagedEnemies =
            new HashSet<EnemyHealth>();

        bool pogoTriggered = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth =
                enemy.GetComponentInParent<EnemyHealth>();

            if (enemyHealth == null ||
                !damagedEnemies.Add(enemyHealth))
                continue;

            // Damage enemy
            enemyHealth.TakeDamage(attackDamage);

            // Pogo bounce
            if (canPogo && !pogoTriggered)
            {
                playerMovement.RB.linearVelocity = new Vector2(
                    playerMovement.RB.linearVelocity.x,
                    pogoForce
                );

                pogoTriggered = true;
            }
        }
    }
}