using UnityEngine;

public class AI : MonoBehaviour
{
    public Transform player; // Người chơi
    public GameObject ball;  // Quả bóng
    public Animator animator; // Animator của AI
    public float crouchRange = 2f; // Phạm vi Crouch Walk
    public float pickupRadius = 2f; // Phạm vi nhặt bóng
    public float moveSpeed = 3f; // Tốc độ di chuyển
    public float runSpeed = 6f; // Tốc độ chạy
    public float rotationSpeed = 5f; // Tốc độ xoay

    [SerializeField] Rigidbody rb; // Rigidbody của AI
    private Vector3 targetPosition;
    private bool isCatching = false;
    private bool hasCaughtBall = false;

    private enum AIState { Idle, ChaseBall, Defend, Catch, Failed }
    private AIState currentState = AIState.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            // Nếu Rigidbody không tồn tại, thêm mới một component
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component not found on this GameObject!");
            }
        }
            if (animator != null)
        {
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError("Animator Controller chưa được gán! Vui lòng gán Animator Controller trong Inspector.");
            }
            else
            {
                Debug.Log("Animator Controller đã được gán: " + animator.runtimeAnimatorController.name);
                
                // Kiểm tra các parameters
                bool hasIsMoving = false, hasRun = false, hasMoveX = false, hasMoveZ = false, hasCatch = false, hasFailed = false;
                
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.name == "IsMoving") hasIsMoving = true;
                    if (param.name == "Run") hasRun = true;
                    if (param.name == "MoveX") hasMoveX = true;
                    if (param.name == "MoveZ") hasMoveZ = true;
                    if (param.name == "Catch") hasCatch = true;
                    if (param.name == "Failed") hasFailed = true;
                }
                
                if (!hasIsMoving || !hasRun || !hasMoveX || !hasMoveZ || !hasCatch || !hasFailed)
                {
                    Debug.LogError("Thiếu parameters trong Animator Controller! Parameters hiện tại: " +
                                "IsMoving: " + hasIsMoving +
                                ", Run: " + hasRun +
                                ", MoveX: " + hasMoveX +
                                ", MoveZ: " + hasMoveZ +
                                ", Catch: " + hasCatch +
                                ", Failed: " + hasFailed);
                }
            }
        }
    }

    void Update()
    {
        RotateTowardsBall();
        UpdateState();
        ExecuteStateActions();
    }

    void RotateTowardsBall()
    {
        Vector3 ballDirection = (ball.transform.position - transform.position).normalized;
        ballDirection.y = 0; // Giữ AI chỉ xoay trên trục Y
        if (ballDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(ballDirection), Time.deltaTime * rotationSpeed);
        }
    }

    void UpdateState()
    {
        bool isBallHeld = ball.transform.parent != null;
        float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);

        if (isCatching || hasCaughtBall)
        {
            return;
        }

        if (distanceToBall <= pickupRadius)
        {
            currentState = AIState.Catch;
        }
        else if (!isBallHeld)
        {
            currentState = AIState.ChaseBall;
        }
        else
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > crouchRange)
            {
                currentState = AIState.Defend;
            }
            else
            {
                currentState = AIState.Idle;
            }
        }
    }

    void ExecuteStateActions()
    {
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

    void HandleIdle()
    {
        if (rb == null || ball == null) return;
    
    Vector3 direction = (ball.transform.position - transform.position).normalized;
    rb.velocity = direction * runSpeed;

    // Cập nhật Animator
    if (animator != null)
    {
        animator.SetBool("IsMoving", true);
        animator.SetBool("Run", true);
        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveZ", direction.z);
    }
    }

    void HandleChaseBall()
    {
        Vector3 direction = (ball.transform.position - transform.position).normalized;
        rb.velocity = direction * runSpeed;

        // Cập nhật Animator
        animator.SetBool("IsMoving", true);
        animator.SetBool("Run", true);
        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveZ", direction.z);
    }

    void HandleDefend()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 targetOffset = direction * crouchRange;
        Vector3 targetPosition = player.position - targetOffset;
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        rb.velocity = moveDirection * moveSpeed;

        // Cập nhật Animator
        animator.SetBool("IsMoving", true);
        animator.SetBool("Run", false); // Di chuyển nhưng không chạy
        animator.SetFloat("MoveX", moveDirection.x);
        animator.SetFloat("MoveZ", moveDirection.z);
    }

    void HandleCatch()
    {
        isCatching = true;
        rb.velocity = Vector3.zero;
        animator.SetTrigger("Catch");

        // Bắt bóng
        ball.transform.SetParent(transform);
        ball.transform.localPosition = new Vector3(0, 1.5f, 0.5f);
        hasCaughtBall = true;
    }

    void HandleFailed()
    {
        rb.velocity = Vector3.zero;
        animator.SetTrigger("Failed");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, crouchRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
