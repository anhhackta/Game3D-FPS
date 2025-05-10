using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    public Transform player; // Người chơi
    public GameObject ball; // Quả bóng
    public Transform basket; // Vị trí rổ
    public Animator animator; // Animator của AI
    public float crouchRange = 2f; // Phạm vi dùng Crouch Walk
    public float pickupRadius = 2f; // Bán kính nhặt bóng
    
    [Header("Movement Settings")]
    public float normalSpeed = 2f; // Tốc độ đi bộ bình thường
    public float runSpeed = 5f; // Tốc độ chạy
    public float crouchSpeed = 1.5f; // Tốc độ khi cúi
    
    [Header("Defense Settings")]
    public float randomMoveRadius = 0.5f; // Bán kính di chuyển ngẫu nhiên
    public float resetDelayCatch = 3f; // Thời gian reset sau khi bắt
    public float resetDelayFail = 5f; // Thời gian reset sau khi thua
    public float idleTimeout = 5f; // Thời gian người chơi đứng yên trước khi AI cướp bóng
    public float wallDistanceThreshold = 1f; // Khoảng cách đến tường để lấn gần người chơi
    
    [Header("Hand Colliders")]
    public Collider rightHandCollider; // Collider cho tay phải
    public Collider leftHandCollider; // Collider cho tay trái

    private NavMeshAgent agent;
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

    // Trạng thái AI
    private enum AIState { Idle, ChaseBall, Defend, Catch, Failed }
    private AIState currentState = AIState.Idle;

    // Animator parameters
    private readonly int moveXHash = Animator.StringToHash("MoveX");
    private readonly int moveZHash = Animator.StringToHash("MoveZ");
    private readonly int runHash = Animator.StringToHash("Run");
    private readonly int catchHash = Animator.StringToHash("Catch");
    private readonly int failHash = Animator.StringToHash("Failed");
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ballRigidbody = ball.GetComponent<Rigidbody>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent is not assigned on AI!");
            return;
        }
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned on AI!");
            return;
        }
        if (player == null || ball == null || basket == null)
        {
            Debug.LogError("Player, Ball, or Basket is not assigned on AI!");
            return;
        }
        if (rightHandCollider == null || leftHandCollider == null)
        {
            Debug.LogWarning("Hand colliders not assigned! Please assign in the Inspector.");
        }

        // Thiết lập NavMeshAgent
        agent.stoppingDistance = crouchRange * 0.5f;
        agent.speed = normalSpeed;
        targetPosition = transform.position;
        lastDecisionTime = Time.time;
        lastPlayerPosition = player.position;
    }

    void Update()
    {
        // Dừng logic nếu game bị pause hoặc đang reset
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused || isCatching || isFailed || hasCaughtBall)
        {
            agent.isStopped = true;
            animator.SetBool(runHash, false);
            animator.SetFloat(moveXHash, 0f);
            animator.SetFloat(moveZHash, 0f);
            animator.SetBool(isMovingHash, false);
            return;
        }

        // Xoay AI hướng về quả bóng
        Vector3 ballDirection = (ball.transform.position - transform.position).normalized;
        ballDirection.y = 0;
        if (ballDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(ballDirection), Time.deltaTime * 5f);
        }

        // Kiểm tra người chơi đứng yên
        UpdatePlayerIdleStatus();

        // Cập nhật trạng thái
        UpdateState();

        // Thực thi hành vi theo trạng thái
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
        // Xử lý reset sau khi bắt trúng
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

        // Xử lý reset sau khi thua
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
        // Kiểm tra người chơi có đứng yên
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
        // Kiểm tra người chơi ghi điểm
        if (GameManager.Instance != null &&  !isFailed && currentState != AIState.Failed)
        {
            currentState = AIState.Failed;
            return;
        }

        // Kiểm tra bóng trên không để bắt
        if (IsBallInAir())
        {
            currentState = AIState.Catch;
            return;
        }

        // Kiểm tra trạng thái bóng
        bool isPlayerHoldingBall = ball.transform.parent != null && ball.transform.parent.GetComponent<PlayerBallController>() != null;

        if (isPlayerIdle)
        {
            // Người chơi đứng yên quá lâu, cướp bóng
            currentState = AIState.ChaseBall;
        }
        else if (!isPlayerHoldingBall)
        {
            // Bóng tự do, đuổi bóng
            currentState = AIState.ChaseBall;
        }
        else
        {
            // Người chơi cầm bóng, phòng thủ
            currentState = AIState.Defend;
        }
    }

    void HandleIdle()
    {
        agent.isStopped = true;
        animator.SetBool(runHash, false);
        animator.SetFloat(moveXHash, 0f);
        animator.SetFloat(moveZHash, 0f);
        animator.SetBool(isMovingHash, false);
    }

    void HandleChaseBall()
    {
        // Chạy đến bóng
        targetPosition = ball.transform.position;
        if (!agent.SetDestination(targetPosition))
        {
            Debug.LogWarning("Failed to set destination for NavMeshAgent!");
        }
        agent.isStopped = false;

        // Điều chỉnh tốc độ dựa vào khoảng cách
        float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);

        if (distanceToBall > crouchRange)
        {
            // Chạy nhanh khi ở xa
            agent.speed = runSpeed;
            animator.SetBool(runHash, true);
            animator.SetFloat(moveXHash, 0f);
            animator.SetFloat(moveZHash, 0f);
            animator.SetBool(isMovingHash, true);
        }
        else
        {
            // Đi chậm khi gần bóng
            agent.speed = normalSpeed;
            animator.SetBool(runHash, false);
            UpdateCrouchAnimation();
            animator.SetBool(isMovingHash, true);
        }

        // Kiểm tra nhặt bóng
        if (distanceToBall <= pickupRadius && !hasCaughtBall)
        {
            PickUpBall();
        }
    }

    void HandleDefend()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float adjustedCrouchRange = crouchRange;

        // Kiểm tra nếu AI gần tường
        NavMeshHit hit;
        if (NavMesh.Raycast(transform.position, transform.position - transform.forward * wallDistanceThreshold, out hit, NavMesh.AllAreas) ||
            NavMesh.Raycast(transform.position, transform.position - transform.right * wallDistanceThreshold, out hit, NavMesh.AllAreas) ||
            NavMesh.Raycast(transform.position, transform.position + transform.right * wallDistanceThreshold, out hit, NavMesh.AllAreas))
        {
            adjustedCrouchRange = crouchRange * 0.5f; // Giảm khoảng cách khi gần tường
        }

        if (distanceToPlayer > crouchRange && !isPlayerIdle)
        {
            // Chạy đến người chơi khi ngoài tầm crouchRange
            targetPosition = player.position;
            if (!agent.SetDestination(targetPosition))
            {
                Debug.LogWarning("Failed to set destination for NavMeshAgent in Defend!");
            }
            agent.isStopped = false;
            agent.speed = runSpeed;
            animator.SetBool(runHash, true);
            animator.SetFloat(moveXHash, 0f);
            animator.SetFloat(moveZHash, 0f);
            animator.SetBool(isMovingHash, true);
        }
        else if (distanceToPlayer > adjustedCrouchRange)
        {
            // Di chuyển đến người chơi bằng Crouch Walk
            targetPosition = player.position;
            if (!agent.SetDestination(targetPosition))
            {
                Debug.LogWarning("Failed to set destination for NavMeshAgent in Defend!");
            }
            agent.isStopped = false;
            agent.speed = crouchSpeed;
            UpdateCrouchAnimation();
            animator.SetBool(runHash, false);
            animator.SetBool(isMovingHash, true);
        }
        else
        {
            // Di chuyển ngẫu nhiên trong crouchRange để phòng thủ
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f || Time.time - lastDecisionTime > decisionInterval * 4)
            {
                targetPosition = transform.position + new Vector3(
                    Random.Range(-randomMoveRadius, randomMoveRadius),
                    0,
                    Random.Range(-randomMoveRadius, randomMoveRadius)
                );
                lastDecisionTime = Time.time;
            }
            if (!agent.SetDestination(targetPosition))
            {
                Debug.LogWarning("Failed to set destination for NavMeshAgent in Defend random move!");
            }
            agent.isStopped = false;
            agent.speed = crouchSpeed;
            UpdateCrouchAnimation();
            animator.SetBool(runHash, false);
            animator.SetBool(isMovingHash, true);
        }
    }

    void HandleCatch()
    {
        isCatching = true;
        agent.isStopped = true;
        animator.SetTrigger(catchHash);
        animator.SetBool(runHash, false);
        animator.SetFloat(moveXHash, 0f);
        animator.SetFloat(moveZHash, 0f);
        animator.SetBool(isMovingHash, false);

        // Kích hoạt colliders tay khi bắt
        if (rightHandCollider != null) rightHandCollider.enabled = true;
        if (leftHandCollider != null) leftHandCollider.enabled = true;
    }

    void HandleFailed()
    {
        isFailed = true;
        agent.isStopped = true;
        animator.SetTrigger(failHash);
        animator.SetBool(runHash, false);
        animator.SetFloat(moveXHash, 0f);
        animator.SetFloat(moveZHash, 0f);
        animator.SetBool(isMovingHash, false);
    }

    bool IsBallInAir()
    {
        if (hasCaughtBall || isCatching || isFailed)
            return false;

        Vector3 ballPos = ball.transform.position;
        Vector3 aiPos = transform.position;

        // Kiểm tra bóng trong không trung và thuộc phạm vi có thể bắt được
        bool isBallFree = ball.transform.parent == null;
        bool isBallMoving = ballRigidbody != null && ballRigidbody.velocity.magnitude > 1.0f;
        float horizontalDistance = Vector3.Distance(
            new Vector3(aiPos.x, 0, aiPos.z),
            new Vector3(ballPos.x, 0, ballPos.z)
        );

        return isBallFree && isBallMoving && ballPos.y > 0.5f && horizontalDistance < pickupRadius;
    }

    void PickUpBall()
    {
        hasCaughtBall = true;
        ballRigidbody.isKinematic = true;
        ball.transform.SetParent(transform);
        ball.transform.localPosition = new Vector3(0, 1.5f, 0.5f); // Dơ bóng lên
        animator.SetBool(runHash, false);
        animator.SetFloat(moveXHash, 0f);
        animator.SetFloat(moveZHash, 0f);
        animator.SetBool(isMovingHash, false);

        // Thông báo khi AI bắt được bóng
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BotCaughtBall();
        }

        // Phát âm thanh nếu có
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("GameOver");
        }
    }

    void UpdateCrouchAnimation()
    {
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);
        float moveX = localMoveDirection.x;
        float moveZ = localMoveDirection.z;

        animator.SetFloat(moveXHash, moveX);
        animator.SetFloat(moveZHash, moveZ);
        animator.SetBool(isMovingHash, Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f);
    }

    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra bóng va chạm với tay khi đang bắt
        if (other.gameObject == ball && isCatching)
        {
            PickUpBall();
            Debug.Log("AI caught the ball!");
        }
    }

    public void OnCatchAnimationEnd()
    {
        isCatching = false;

        // Tắt colliders tay khi kết thúc animation bắt
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
        Gizmos.DrawWireSphere(transform.position, crouchRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}