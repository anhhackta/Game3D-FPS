using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AdvancedBasketballAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float crouchSpeed = 1f;
    [SerializeField] private float idealDistance = 3f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 5f;

    [Header("Defense Settings")]
    [SerializeField] private float blockRange = 2.5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float waveDuration = 1f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform ball;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    private Vector3 initialPosition;
    private bool isBallInAir = false;
    private float currentDistance;
    private AIState currentState = AIState.Idle;

    private enum AIState
    {
        Idle,
        Walking,
        CrouchedWalking,
        IdleBlock,
        Waving,
        PickingBall
    }

    private void Start()
    {
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        currentDistance = Vector3.Distance(transform.position, player.position);
        
        // Xác định trạng thái dựa trên vị trí bóng và khoảng cách với người chơi
        DetermineState();
        
        // Xử lý hành vi cho từng trạng thái
        HandleStateBehavior();
        
        // Luôn hướng về phía người chơi
        FacePlayer();
    }

    private void DetermineState()
    {
        // Kiểm tra nếu bóng đang trên không
        isBallInAir = ball.position.y > 1f && !IsBallHeld();

        if (IsBallOnGround())
        {
            currentState = AIState.PickingBall;
            return;
        }

        if (isBallInAir && IsInBlockRange())
        {
            currentState = AIState.Waving;
            return;
        }

        if (currentDistance < minDistance)
        {
            currentState = AIState.CrouchedWalking;
        }
        else if (currentDistance > maxDistance)
        {
            currentState = AIState.Walking;
        }
        else
        {
            currentState = AIState.IdleBlock;
        }
    }

    private void HandleStateBehavior()
    {
        switch (currentState)
        {
            case AIState.Walking:
                MoveAwayFromPlayer(walkSpeed);
                animator.SetBool("IsWalking", true);
                animator.SetBool("IsCrouched", false);
                break;
                
            case AIState.CrouchedWalking:
                MoveAwayFromPlayer(crouchSpeed);
                animator.SetBool("IsWalking", true);
                animator.SetBool("IsCrouched", true);
                break;
                
            case AIState.IdleBlock:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsCrouched", true);
                break;
                
            case AIState.Waving:
                StartCoroutine(PerformWave());
                break;
                
            case AIState.PickingBall:
                PickUpBall();
                break;
        }
    }

    private void MoveAwayFromPlayer(float speed)
    {
        Vector3 direction = (transform.position - player.position).normalized;
        direction.y = 0;
        
        Vector3 targetPosition = player.position + direction * idealDistance;
        targetPosition.y = transform.position.y;
        
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            speed * Time.deltaTime
        );
    }

    private System.Collections.IEnumerator PerformWave()
    {
        animator.SetTrigger("Wave");
        
        // Nhảy lên cản bóng
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        
        yield return new WaitForSeconds(waveDuration);
        
        // Trở về trạng thái bình thường
        if (currentState != AIState.PickingBall)
        {
            currentState = AIState.IdleBlock;
        }
    }

    private void PickUpBall()
    {
        animator.SetTrigger("PickUp");
        
        // Di chuyển đến bóng
        transform.position = Vector3.MoveTowards(
            transform.position, 
            ball.position, 
            walkSpeed * Time.deltaTime
        );
        
        // Khi đã đến đủ gần
        if (Vector3.Distance(transform.position, ball.position) < 1f)
        {
            //GameManager.Instance.PlayerLost();
        }
    }

    private void FacePlayer()
    {
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f * Time.deltaTime);
    }

    private bool IsInBlockRange()
    {
        return Vector3.Distance(transform.position, ball.position) < blockRange;
    }

    private bool IsBallHeld()
    {
        return ball.parent != null && ball.parent.CompareTag("Player");
    }

    private bool IsBallOnGround()
    {
        return ball.position.y < 0.5f && !IsBallHeld();
    }
}