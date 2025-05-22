using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FootstepSound : MonoBehaviour
{
    public float minMoveSpeed = 0.1f;
    public float soundCooldown = 0.4f;

    private CharacterController controller;
    private float lastStepTime = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Kiểm tra đang trên mặt đất và đang di chuyển
        if (controller.isGrounded && controller.velocity.magnitude > minMoveSpeed)
        {
            if (Time.time - lastStepTime > soundCooldown)
            {
                AudioManager.Instance?.PlaySFX("Move");
                lastStepTime = Time.time;
            }
        }
    }
}
