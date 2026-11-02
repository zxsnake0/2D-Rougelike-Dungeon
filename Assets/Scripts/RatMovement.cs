using UnityEngine;

public class RatMovement : MonoBehaviour
{
    public enum RatState { Patrol, Run, Returning }

    [Header("State Machine")]
    public RatState currentState = RatState.Patrol;

    public float patrolSpeed = 3f;
    public float runSpeed = 6f;

    [Header("Detection Settings")]
    public float chaseRadius = 8f;
    public string playerTag = "Player";

    [Header("Patrol Settings")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    public float waypointThreshold = 0.5f; // Increased slightly for smoother physics checks

    [Header("Combat Settings")]
    public int damageAmount = 1;
    public float attackCooldown = 1.5f;

    private Transform playerTransform;
    private Transform currentTargetWaypoint;
    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private float nextAttackTime;
    private float currentHorizontalVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null) playerTransform = player.transform;

        if (patrolPointA != null) currentTargetWaypoint = patrolPointA;
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= chaseRadius)
            {
                currentState = RatState.Run;
            }
            else if (currentState == RatState.Run)
            {
                currentState = RatState.Returning;
            }
        }

        // Determine targets and direction in Update, but move in FixedUpdate
        switch (currentState)
        {
            case RatState.Patrol:
                PatrolBehavior();
                break;
            case RatState.Run:
                RunBehavior();
                break;
            case RatState.Returning:
                ReturningBehavior();
                break;
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // NEW PHYSICS MOVEMENT: Apply horizontal velocity while preserving gravity's vertical pull (rb.linearVelocity.y)
            rb.linearVelocity = new Vector2(currentHorizontalVelocity, rb.linearVelocity.y);
            rb.angularVelocity = 0f;
        }
    }

    void PatrolBehavior()
    {
        if (patrolPointA == null || patrolPointB == null) return;

        // Calculate horizontal direction to the waypoint
        float directionX = Mathf.Sign(currentTargetWaypoint.position.x - transform.position.x);
        currentHorizontalVelocity = directionX * patrolSpeed;

        HandleFlipping(currentTargetWaypoint.position.x);

        // Horizontal-only distance check so Y-axis deviations (like step heights) don't confuse the waypoint system
        float distanceToTargetX = Mathf.Abs(transform.position.x - currentTargetWaypoint.position.x);
        if (distanceToTargetX < waypointThreshold)
        {
            currentTargetWaypoint = (currentTargetWaypoint == patrolPointA) ? patrolPointB : patrolPointA;
        }
    }

    void RunBehavior()
    {
        if (playerTransform == null) return;

        // FIX: Calculate direction based ONLY on the player's horizontal X position
        float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);
        currentHorizontalVelocity = directionX * runSpeed;

        HandleFlipping(playerTransform.position.x);
    }

    void ReturningBehavior()
    {
        Transform closestWaypoint = (Vector2.Distance(transform.position, patrolPointA.position) < Vector2.Distance(transform.position, patrolPointB.position)) ? patrolPointA : patrolPointB;
        
        float directionX = Mathf.Sign(closestWaypoint.position.x - transform.position.x);
        currentHorizontalVelocity = directionX * patrolSpeed;

        HandleFlipping(closestWaypoint.position.x);

        float distanceToTargetX = Mathf.Abs(transform.position.x - closestWaypoint.position.x);
        if (distanceToTargetX < waypointThreshold)
        {
            currentTargetWaypoint = closestWaypoint;
            currentState = RatState.Patrol;
        }
    }

    void HandleFlipping(float targetX)
    {
        if (targetX > transform.position.x && !isFacingRight) Flip();
        else if (targetX < transform.position.x && isFacingRight) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    // DAMAGE HANDLING SYSTEM
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleDamage(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleDamage(collision.gameObject);
    }

    private void HandleDamage(GameObject obj)
    {
        if (obj.CompareTag(playerTag) && Time.time >= nextAttackTime)
        {
            PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                nextAttackTime = Time.time + attackCooldown;
                Debug.Log($"Rat bit the player! Dealt {damageAmount} damage.");
            }
        }
    }
}