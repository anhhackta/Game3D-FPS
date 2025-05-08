using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    public Transform player; // Người chơi
    public GameObject ball; // Quả bóng
    public Transform basket; // Vị trí rổ
    public Animator animator; // Animator của AI
    public float crouchRange = 3f; // Phạm vi sử dụng Crouch Walk
    public float zoomRange = 10f; // Phạm vi sử dụng Run
    public float catchHeight = 2f; // Độ cao tối đa để bắt bóng
    public float reactionDelay = 0.3f; // Độ trễ phản ứng
    public float randomMoveRadius = 0.5f; // Bán kính di chuyển ngẫu nhiên trong crouchRange
    public float catchDistance = 1.5f; // Khoảng cách để kích hoạt Catch
    public float resetDelayCatch = 3f; // Thời gian reset sau khi bắt trúng
    public float resetDelayFail = 5f; // Thời gian reset sau khi thua

    private NavMeshAgent agent;
    private Vector3 targetPosition;
    private float lastReactionTime;
    private bool isCatching = false;
    private bool isFailed = false;
    private bool hasCaughtBall = false;
    private float catchTimer = 0f;
    private float failTimer = 0f;

    // Animator parameters
    private readonly int crouchForwardHash = Animator.StringToHash("CrouchWalkForward");
    private readonly int crouchBackHash = Animator.StringToHash("CrouchWalkBack");
    private readonly int crouchLeftHash = Animator.StringToHash("CrouchWalkLeft");
    private readonly int crouchRightHash = Animator.StringToHash("CrouchWalkRight");
    private readonly int runHash = Animator.StringToHash("Run");
    private readonly int catchHash = Animator.StringToHash("Catch");
    private readonly int failHash = Animator.StringToHash("Failed");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
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

        // Thiết lập NavMeshAgent
        agent.stoppingDistance = crouchRange * 0.8f;
        targetPosition = transform.position;
        lastReactionTime = Time.time;
    }

    void Update()
    {
        // Dừng logic nếu game bị pause hoặc đang reset
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused || isCatching || isFailed || hasCaughtBall)
        {
            agent.isStopped = true;
            animator.SetBool(runHash, false);
            animator.SetBool(crouchForwardHash, false);
            animator.SetBool(crouchBackHash, false);
            animator.SetBool(crouchLeftHash, false);
            animator.SetBool(crouchRightHash, false);
            return;
        }

        // Xoay AI hướng về người chơi
        Vector3 lookDirection = (player.position - transform.position).normalized;
        lookDirection.y = 0; // Giữ xoay trên mặt phẳng XZ
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
        }

        // Kiểm tra bóng trên không để bắt
        if (CanCatchBall())
        {
            StartCatch();
            return;
        }

        // Kiểm tra người chơi ghi điểm
        /*if (GameManager.Instance != null && GameManager.Instance.Score > 0 && !isFailed)
        {
            StartFail();
            return;
        }*/

        // Xử lý di chuyển sau độ trễ phản ứng
        if (Time.time - lastReactionTime < reactionDelay)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > zoomRange)
        {
            // Chạy đến người chơi bằng animation Run
            targetPosition = player.position;
            agent.SetDestination(targetPosition);
            agent.speed = 5f; // Tốc độ chạy
            animator.SetBool(runHash, true);
            animator.SetBool(crouchForwardHash, false);
            animator.SetBool(crouchBackHash, false);
            animator.SetBool(crouchLeftHash, false);
            animator.SetBool(crouchRightHash, false);
        }
        else if (distanceToPlayer > crouchRange)
        {
            // Di chuyển đến người chơi bằng animation Crouch Walk
            targetPosition = player.position;
            agent.SetDestination(targetPosition);
            agent.speed = 2f; // Tốc độ di chuyển thấp
            UpdateCrouchAnimation();
            animator.SetBool(runHash, false);
        }
        else
        {
            // Di chuyển ngẫu nhiên trong crouchRange
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f || Time.time - lastReactionTime > 2f)
            {
                targetPosition = transform.position + new Vector3(
                    Random.Range(-randomMoveRadius, randomMoveRadius),
                    0,
                    Random.Range(-randomMoveRadius, randomMoveRadius)
                );
                lastReactionTime = Time.time;
            }
            agent.SetDestination(targetPosition);
            agent.speed = 2f;
            UpdateCrouchAnimation();
            animator.SetBool(runHash, false);
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
            }
        }
    }

    bool CanCatchBall()
    {
        if (hasCaughtBall || isCatching || isFailed)
            return false;

        Vector3 ballPos = ball.transform.position;
        Vector3 aiHeadPos = transform.position + Vector3.up * 1.5f; // Giả sử đầu AI cao 1.5m
        float distanceToBall = Vector3.Distance(aiHeadPos, ballPos);

        // Kiểm tra bóng trên không và trong phạm vi bắt
        return ballPos.y > 0.5f && // Bóng không trên sàn
               distanceToBall <= catchDistance && // Trong khoảng cách bắt
               ballPos.y <= aiHeadPos.y + catchHeight; // Trong phạm vi chiều cao
    }

    void StartCatch()
    {
        isCatching = true;
        agent.isStopped = true;
        animator.SetTrigger(catchHash);
        animator.SetBool(runHash, false);
        animator.SetBool(crouchForwardHash, false);
        animator.SetBool(crouchBackHash, false);
        animator.SetBool(crouchLeftHash, false);
        animator.SetBool(crouchRightHash, false);

        // Kiểm tra bắt trúng qua collision (xử lý trong OnTriggerEnter)
    }

    void StartFail()
    {
        isFailed = true;
        agent.isStopped = true;
        animator.SetTrigger(failHash);
        animator.SetBool(runHash, false);
        animator.SetBool(crouchForwardHash, false);
        animator.SetBool(crouchBackHash, false);
        animator.SetBool(crouchLeftHash, false);
        animator.SetBool(crouchRightHash, false);
    }

    void UpdateCrouchAnimation()
    {
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        Vector3 forward = transform.forward;
        float dotForward = Vector3.Dot(moveDirection, forward);
        float dotRight = Vector3.Dot(moveDirection, transform.right);

        // Xác định hướng di chuyển tương đối
        bool isForward = dotForward > 0.7f;
        bool isBack = dotForward < -0.7f;
        bool isRight = dotRight > 0.7f;
        bool isLeft = dotRight < -0.7f;

        animator.SetBool(crouchForwardHash, isForward);
        animator.SetBool(crouchBackHash, isBack);
        animator.SetBool(crouchLeftHash, isLeft);
        animator.SetBool(crouchRightHash, isRight);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCatching && other.gameObject == ball)
        {
            // Bắt trúng bóng
            hasCaughtBall = true;
            isCatching = false;
            ball.transform.SetParent(transform); // Gắn bóng vào AI
            ball.transform.localPosition = new Vector3(0, 1.5f, 0.5f); // Dơ bóng lên
            ball.GetComponent<Rigidbody>().isKinematic = true; // Tắt vật lý
            Debug.Log("AI caught the ball!");
        }
    }

    void OnDrawGizmos()
    {
        // Vẽ phạm vi crouchRange và zoomRange
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, crouchRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, zoomRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.5f, catchDistance);
    }
}