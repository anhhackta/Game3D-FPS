using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public Transform player; // Nhân vật người chơi
    public GameObject ball; // Quả bóng
    public float minDistance = 3f; // Khoảng cách tối thiểu đến người chơi
    public float maxDistance = 5f; // Khoảng cách tối đa đến người chơi
    public float interceptRadius = 4f; // Phạm vi phát hiện bóng để cản
    public float pickupRadius = 1.5f; // Phạm vi nhặt bóng dưới sàn
    public Animator animator; // Animator của AI
    public float crouchDistance = 4f; // Khoảng cách để chuyển sang Crouched Walking

    private NavMeshAgent agent;
    private Rigidbody ballRigidbody;
    private bool isBallInAir = false;
    private bool isGameOver = false;
    private float idleLookAroundTimer = 0f; // Bộ đếm thời gian cho hành vi nhìn xung quanh
    private float idleLookAroundInterval = 3f; // Khoảng thời gian giữa các lần nhìn xung quanh
    private Vector3 lookAroundTarget; // Điểm mục tiêu để nhìn xung quanh

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (ball != null)
            ballRigidbody = ball.GetComponent<Rigidbody>();
        else
            Debug.LogError("Ball is not assigned in AIController!");
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator is not assigned in AIController!");
    }

    void Update()
    {
        // Dừng logic nếu game bị pause hoặc game over
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused || isGameOver)
            return;

        // Tính khoảng cách đến người chơi
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Kiểm tra trạng thái bóng
        CheckBallStatus();

        // Xử lý hành vi AI
        if (isBallInAir && IsBallInInterceptRange())
        {
            // Nhảy cản bóng
            animator.SetBool("IsWaving", true);
            agent.isStopped = true; // Dừng di chuyển khi nhảy
            SoundManager.Instance.PlaySFX(SoundManager.Instance.scoreClip); // Âm thanh cản bóng
        }
        else if (IsBallOnGround() && Vector3.Distance(transform.position, ball.transform.position) <= pickupRadius)
        {
            // Nhặt bóng và thua cuộc
            PickUpBall();
        }
        else
        {
            animator.SetBool("IsWaving", false);
            MoveToPlayer(distanceToPlayer);
        }

        // Thêm hành vi nhìn xung quanh khi bot không di chuyển
        if (agent.isStopped && !animator.GetBool("IsWalking") && !animator.GetBool("IsCrouchedWalking"))
        {
            HandleIdleLookAround();
        }
    }

    void CheckBallStatus()
    {
        // Bóng ở trên không (được ném) nếu có vận tốc và không bị cầm
        isBallInAir = !ballRigidbody.isKinematic && ballRigidbody.velocity.magnitude > 0.1f;
    }

    bool IsBallInInterceptRange()
    {
        return Vector3.Distance(transform.position, ball.transform.position) <= interceptRadius;
    }

    bool IsBallOnGround()
    {
        return !isBallInAir && !ballRigidbody.isKinematic && ballRigidbody.velocity.magnitude < 0.1f;
    }

    void MoveToPlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > maxDistance)
        {
            // Đi bộ để đến gần người chơi
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsCrouchedWalking", false);
            animator.SetBool("IsIdleBlock", false);
        }
        else if (distanceToPlayer < minDistance)
        {
            // Lùi lại để giữ khoảng cách
            Vector3 retreatDirection = (transform.position - player.position).normalized;
            Vector3 retreatPosition = transform.position + retreatDirection * (minDistance - distanceToPlayer);
            agent.isStopped = false;
            agent.SetDestination(retreatPosition);
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsCrouchedWalking", false);
            animator.SetBool("IsIdleBlock", false);
        }
        else if (distanceToPlayer <= crouchDistance)
        {
            // Di chuyển ngồi trong phạm vi gần
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsCrouchedWalking", true);
            animator.SetBool("IsIdleBlock", false);
        }
        else
        {
            // Đứng chờ (IdleBlock) ở khoảng cách lý tưởng
            agent.isStopped = true;
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsCrouchedWalking", false);
            animator.SetBool("IsIdleBlock", true);
        }
    }

    void PickUpBall()
    {
        if (ball == null || ballRigidbody == null || animator == null)
        {
            Debug.LogError("Ball, BallRigidbody, or Animator is not assigned in AIController!");
            return;
        }

        isGameOver = true;
        ballRigidbody.isKinematic = true;
        ball.transform.position = transform.position + Vector3.up * 1f; // Nâng bóng lên tay AI
        ball.transform.SetParent(transform); // Gắn bóng vào AI
        animator.SetBool("IsHolding", true); // Animation cầm bóng
        if (GameManager.Instance != null)
            GameManager.Instance.ShowNotification("Game Over! AI nhặt bóng!");
        else
            Debug.LogError("GameManager.Instance is null!");
        if (SoundManager.Instance != null && SoundManager.Instance.scoreClip != null)
            SoundManager.Instance.PlaySFX(SoundManager.Instance.scoreClip);
        else
            Debug.LogWarning("SoundManager or scoreClip is not assigned!");
    }

    void HandleIdleLookAround()
    {
        idleLookAroundTimer += Time.deltaTime;

        if (idleLookAroundTimer >= idleLookAroundInterval)
        {
            // Tạo một hướng nhìn ngẫu nhiên trong phạm vi 360 độ
            lookAroundTarget = transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            idleLookAroundTimer = 0f; // Reset bộ đếm thời gian
        }

        // Xoay bot về hướng mục tiêu nhìn xung quanh
        Vector3 direction = (lookAroundTarget - transform.position).normalized;
        if (direction.magnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f); // Làm mượt xoay
        }
    }

    void OnDrawGizmos()
    {
        // Vẽ phạm vi cản bóng và nhặt bóng
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interceptRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}