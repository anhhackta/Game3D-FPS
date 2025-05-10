using UnityEngine;

public class BasketballAI : MonoBehaviour
{
    [Header("References")]
    public Transform player; 
    public GameObject ball; 
    public Transform basket; 
    public Animator animator; 
    public Collider rightHandCollider; 
    public Collider leftHandCollider; 

    [Header("Movement Settings")]
    public float runSpeed = 5f; 
    public float crouchSpeed = 1.5f; 
    public float zoomRange = 2f; 
    public float catchRange = 2f; 

    [Header("Defense Settings")]
    public float randomMoveRadius = 0.5f; 
    public float wallDistanceThreshold = 1f; 
    public float idleTimeout = 5f; 

    [Header("Reset Settings")]
    public float resetDelayCatch = 3f; 
    public float resetDelayFail = 5f; 

    private Rigidbody ballRigidbody;
    private Vector3 targetPosition;
    private bool isCatching = false;
    private bool isFailed = false;
    private bool hasCaughtBall = false;
    private float catchTimer = 0f;
    private float failTimer = 0f;
    private Vector3 lastPlayerPosition;
    private float playerIdleTimer = 0f;
    private bool isPlayerIdle = false;
    private float lastDecisionTime = 0f;
    private float decisionInterval = 0.5f;
    private bool isInitialized = false;

    private enum AIState { Idle, ChaseBall, Defend, Catch, Failed }
    private AIState currentState = AIState.Idle;

    private readonly int moveXHash = Animator.StringToHash("MoveX");
    private readonly int moveZHash = Animator.StringToHash("MoveZ");
    private readonly int isRunningHash = Animator.StringToHash("IsRunning");
    private readonly int catchHash = Animator.StringToHash("Catch");
    private readonly int failedHash = Animator.StringToHash("Failed");

    void Start()
    {

        if (player == null || ball == null || basket == null)
        {
            Debug.LogError("Player, Ball, or Basket is not assigned on BasketballAI! Please assign in the Inspector.");
            isInitialized = false;
            return;
        }
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned on BasketballAI! Please assign in the Inspector.");
            isInitialized = false;
            return;
        }
        ballRigidbody = ball.GetComponent<Rigidbody>();
        if (ballRigidbody == null)
        {
            Debug.LogError("Ball does not have a Rigidbody component!");
            isInitialized = false;
            return;
        }
        if (rightHandCollider == null || leftHandCollider == null)
        {
            Debug.LogWarning("Hand colliders not assigned! Please assign in the Inspector.");
        }

        targetPosition = transform.position;
        lastDecisionTime = Time.time;
        lastPlayerPosition = player.position;
        isInitialized = true;

        if (rightHandCollider != null) rightHandCollider.enabled = false;
        if (leftHandCollider != null) leftHandCollider.enabled = false;
    }

    void Update()
    {

        if (!isInitialized || (PauseManager.Instance != null && PauseManager.Instance.IsPaused) || isCatching || isFailed || hasCaughtBall)
        {
            animator.SetBool(isRunningHash, false);
            animator.SetFloat(moveXHash, 0f);
            animator.SetFloat(moveZHash, 0f);
            return;
        }

        Vector3 ballDirection = (ball.transform.position - transform.position).normalized;
        ballDirection.y = 0;
        if (ballDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(ballDirection), Time.deltaTime * 5f);
        }

        UpdatePlayerIdleStatus();

        UpdateState();

        switch (currentState)
        {
            case AIState.Idle:
                HandleIdle();
                break;
            case AIState.ChaseBall:
                HandleChaseBall();
                break;
            case AIState.Defend:
                HandleDefend();
                break;
            case AIState.Catch:
                HandleCatch();
                break;
            case AIState.Failed:
                HandleFailed();
                break;
        }
    }

    void LateUpdate()
    {
        if (!isInitialized)
            return;

        if (hasCaughtBall)
        {
            catchTimer += Time.deltaTime;
            if (catchTimer >= resetDelayCatch)
            {
                GameManager.Instance?.ResetBall();
                hasCaughtBall = false;
                catchTimer = 0f;
                currentState = AIState.Idle;
            }
        }

        if (isFailed)
        {
            failTimer += Time.deltaTime;
            if (failTimer >= resetDelayFail)
            {
                GameManager.Instance?.ResetBall();
                isFailed = false;
                failTimer = 0f;
                currentState = AIState.Idle;
            }
        }
    }

    void UpdatePlayerIdleStatus()
    {

        float distanceMoved = Vector3.Distance(player.position, lastPlayerPosition);
        if (distanceMoved < 0.1f)
        {
            playerIdleTimer += Time.deltaTime;
            isPlayerIdle = playerIdleTimer >= idleTimeout;
        }
        else
        {
            playerIdleTimer = 0f;
            isPlayerIdle = false;
        }
        lastPlayerPosition = player.position;
    }

    void UpdateState()
    {

        if (GameManager.Instance != null && !isFailed && currentState != AIState.Failed)
        {
            currentState = AIState.Failed;
            return;
        }

        if (IsBallInAir())
        {
            currentState = AIState.Catch;
            return;
        }

        bool isPlayerHoldingBall = ball.transform.parent != null && ball.transform.parent.GetComponent<PlayerBallController>() != null;

        if (isPlayerIdle)
        {

            currentState = AIState.ChaseBall;
        }
        else if (!isPlayerHoldingBall)
        {

            currentState = AIState.ChaseBall;
        }
        else
        {

            currentState = AIState.Defend;
        }
    }

    void HandleIdle()
    {
        animator.SetBool(isRunningHash, false);
        animator.SetFloat(moveXHash, 0f);
        animator.SetFloat(moveZHash, 0f);
    }

    void HandleChaseBall()
    {

        targetPosition = ball.transform.position;
        float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);

        if (distanceToBall > zoomRange)
        {

            MoveToTarget(targetPosition, runSpeed);
            animator.SetBool(isRunningHash, true);
            animator.SetFloat(moveXHash, 0f);
            animator.SetFloat(moveZHash, 0f);
        }
        else
        {

            MoveToTarget(targetPosition, crouchSpeed);
            animator.SetBool(isRunningHash, false);
            UpdateCrouchAnimation();
        }

        if (distanceToBall <= catchRange && !hasCaughtBall)
        {
            PickUpBall();
        }
    }

    void HandleDefend()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float adjustedZoomRange = zoomRange;

        if (IsNearWall())
        {
            adjustedZoomRange = zoomRange * 0.5f; 
        }

        if (distanceToPlayer > zoomRange && !isPlayerIdle)
        {

            targetPosition = player.position;
            MoveToTarget(targetPosition, runSpeed);
            animator.SetBool(isRunningHash, true);
            animator.SetFloat(moveXHash, 0f);
            animator.SetFloat(moveZHash, 0f);
        }
        else if (distanceToPlayer > adjustedZoomRange)
        {

            targetPosition = player.position;
            MoveToTarget(targetPosition, crouchSpeed);
            animator.SetBool(isRunningHash, false);
            UpdateCrouchAnimation();
        }
        else
        {

            if (Vector3.Distance(transform.position, targetPosition) < 0.5f || Time.time - lastDecisionTime > decisionInterval * 4)
            {
                targetPosition = transform.position + new Vector3(
                    Random.Range(-randomMoveRadius, randomMoveRadius),
                    0,
                    Random.Range(-randomMoveRadius, randomMoveRadius)
                );
                lastDecisionTime = Time.time;
            }
            MoveToTarget(targetPosition, crouchSpeed);
            animator.SetBool(isRunningHash, false);
            UpdateCrouchAnimation();
        }
    }

    void HandleCatch()
    {
        isCatching = true;
        animator.SetTrigger(catchHash);
        animator.SetBool(isRunningHash, false);
        animator.SetFloat(moveXHash, 0f);
        animator.SetFloat(moveZHash, 0f);

        if (rightHandCollider != null) rightHandCollider.enabled = true;
        if (leftHandCollider != null) leftHandCollider.enabled = true;
    }

    void HandleFailed()
    {
        isFailed = true;
        animator.SetTrigger(failedHash);
        animator.SetBool(isRunningHash, false);
        animator.SetFloat(moveXHash, 0f);
        animator.SetFloat(moveZHash, 0f);
    }

    bool IsBallInAir()
    {
        if (!isInitialized || hasCaughtBall || isCatching || isFailed)
            return false;

        Vector3 ballPos = ball.transform.position;
        Vector3 aiPos = transform.position;

        bool isBallFree = ball.transform.parent == null;
        bool isBallMoving = ballRigidbody != null && ballRigidbody.velocity.magnitude > 1.0f;
        float horizontalDistance = Vector3.Distance(
            new Vector3(aiPos.x, 0, aiPos.z),
            new Vector3(ballPos.x, 0, ballPos.z)
        );

        return isBallFree && isBallMoving && ballPos.y > 0.5f && horizontalDistance < catchRange;
    }

    void PickUpBall()
    {
        hasCaughtBall = true;
        ballRigidbody.isKinematic = true;
        ball.transform.SetParent(transform);
        ball.transform.localPosition = new Vector3(0, 1.5f, 0.5f); 
        animator.SetBool(isRunningHash, false);
        animator.SetFloat(moveXHash, 0f);
        animator.SetFloat(moveZHash, 0f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.BotCaughtBall();
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("Lose");
        }
    }

    void MoveToTarget(Vector3 target, float speed)
    {
        target.y = transform.position.y; 
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    bool IsNearWall()
    {

        return Physics.Raycast(transform.position, -transform.forward, wallDistanceThreshold, ~LayerMask.GetMask("Player", "Ball")) ||
               Physics.Raycast(transform.position, -transform.right, wallDistanceThreshold, ~LayerMask.GetMask("Player", "Ball")) ||
               Physics.Raycast(transform.position, transform.right, wallDistanceThreshold, ~LayerMask.GetMask("Player", "Ball"));
    }

    void UpdateCrouchAnimation()
    {
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);
        float moveX = localMoveDirection.x;
        float moveZ = localMoveDirection.z;

        animator.SetFloat(moveXHash, moveX);
        animator.SetFloat(moveZHash, moveZ);
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject == ball && isCatching)
        {
            PickUpBall();
            Debug.Log("AI caught the ball!");
        }
    }

    public void OnCatchAnimationEnd()
    {
        isCatching = false;

        if (rightHandCollider != null) rightHandCollider.enabled = false;
        if (leftHandCollider != null) leftHandCollider.enabled = false;
    }

    void OnHandColliderHitBall(Collider handCollider)
    {
        Collider ballCollider = ball.GetComponent<Collider>();
        if (ballCollider != null)
        {
            Vector3 direction;
            float distance;

            if (Physics.ComputePenetration(
                handCollider, handCollider.transform.position, handCollider.transform.rotation,
                ballCollider, ballCollider.transform.position, ballCollider.transform.rotation,
                out direction, out distance))
            {
                if (!hasCaughtBall && isCatching)
                {
                    PickUpBall();
                    Debug.Log($"AI caught the ball with {handCollider.name}!");
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, zoomRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, catchRange);
    }
}