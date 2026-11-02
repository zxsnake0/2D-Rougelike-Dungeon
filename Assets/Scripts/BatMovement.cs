using UnityEngine;

public class BatMovement : MonoBehaviour
{
    public enum BatState { Patrol, Swoop, Returning }

    [Header("State Machine")]
    public BatState currentState = BatState.Patrol;

    [Header("Flying Attributes (Bobbing Effect)")]
    public float patrolSpeed = 3f;
    public float swoopSpeed = 6f;
    public float bobSpeed = 5f;
    public float bobAmount = 0.2f;

    [Header("Detection Settings")]
    public float chaseRadius = 8f;
    public string playerTag = "Player";

    [Header("Patrol Settings")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    public float waypointThreshold = 0.3f;

    [Header("Combat Settings")]
    public int damageAmount = 1;
    public float attackCooldown = 1.5f;

    private Transform playerTransform;
    private Transform currentTargetWaypoint;
    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private float bobTimer;
    private float nextAttackTime;

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
        bobTimer += Time.deltaTime * bobSpeed;

        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= chaseRadius)
            {
                currentState = BatState.Swoop;
            }
            else if (currentState == BatState.Swoop)
            {
                currentState = BatState.Returning;
            }
        }

        switch (currentState)
        {
            case BatState.Patrol:
                PatrolBehavior();
                break;
            case BatState.Swoop:
                SwoopBehavior();
                break;
            case BatState.Returning:
                ReturningBehavior();
                break;
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void PatrolBehavior()
    {
        if (patrolPointA == null || patrolPointB == null) return;

        Vector2 targetPos = currentTargetWaypoint.position;
        Vector2 nextPosition = Vector2.MoveTowards(transform.position, targetPos, patrolSpeed * Time.deltaTime);
        
        nextPosition.y += Mathf.Sin(bobTimer) * bobAmount * Time.deltaTime;
        transform.position = nextPosition;

        HandleFlipping(currentTargetWaypoint.position.x);

        if (Vector2.Distance(transform.position, currentTargetWaypoint.position) < waypointThreshold)
        {
            currentTargetWaypoint = (currentTargetWaypoint == patrolPointA) ? patrolPointB : patrolPointA;
        }
    }

    void SwoopBehavior()
    {
        if (playerTransform == null) return;

        Vector2 targetPos = playerTransform.position;
        Vector2 nextPosition = Vector2.MoveTowards(transform.position, targetPos, swoopSpeed * Time.deltaTime);

        nextPosition.y += Mathf.Sin(bobTimer) * (bobAmount * 0.5f) * Time.deltaTime;
        transform.position = nextPosition;

        HandleFlipping(playerTransform.position.x);
    }

    void ReturningBehavior()
    {
        Transform closestWaypoint = (Vector2.Distance(transform.position, patrolPointA.position) < Vector2.Distance(transform.position, patrolPointB.position)) ? patrolPointA : patrolPointB;
        
        Vector2 targetPos = closestWaypoint.position;
        Vector2 nextPosition = Vector2.MoveTowards(transform.position, targetPos, patrolSpeed * Time.deltaTime);
        
        nextPosition.y += Mathf.Sin(bobTimer) * bobAmount * Time.deltaTime;
        transform.position = nextPosition;

        HandleFlipping(closestWaypoint.position.x);

        if (Vector2.Distance(transform.position, closestWaypoint.position) < waypointThreshold)
        {
            currentTargetWaypoint = closestWaypoint;
            currentState = BatState.Patrol;
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
                Debug.Log($"Bat bit the player! Dealt {damageAmount} damage.");
            }
        }
    }
}
