using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBallController : MonoBehaviour
{
    public Transform ballHoldPosition; // Vị trí trên tay nhân vật để cầm bóng
    public GameObject ball; // Đối tượng bóng
    public float minThrowForce = 5f; // Lực ném tối thiểu
    public float maxThrowForce = 20f; // Lực ném tối đa
    public float upwardForce = 5f; // Lực hướng lên để tạo quỹ đạo parabol
    public float chargeTime = 2f; // Thời gian giữ chuột để đạt lực tối đa
    public float pickupRadius = 2f; // Bán kính để nhặt bóng
    public Animator animator; // Animator của nhân vật
    public Slider powerSlider; // Thanh lực UI
    public LineRenderer trajectoryLine; // LineRenderer cho quỹ đạo dự đoán
    public int trajectoryPoints = 30; // Số điểm trên đường quỹ đạo
    public float trajectoryTimeStep = 0.1f; // Khoảng thời gian giữa các điểm

    private bool isHoldingBall = false; // Trạng thái cầm bóng
    private bool isChargingThrow = false; // Trạng thái đang canh lực ném
    private float chargeStartTime; // Thời gian bắt đầu giữ chuột trái
    private Rigidbody ballRigidbody;
    private Vector3 throwDirection;

    void Start()
    {
        ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.isKinematic = true; // Bóng không chịu vật lý khi đang cầm
        ball.transform.position = ballHoldPosition.position; // Đặt bóng ở vị trí tay
        ball.transform.SetParent(ballHoldPosition); // Gắn bóng vào tay nhân vật
        isHoldingBall = true;

        // Thiết lập LineRenderer
        trajectoryLine.positionCount = trajectoryPoints;
        trajectoryLine.enabled = false;

        // Ẩn thanh lực ban đầu
        if (powerSlider != null)
            powerSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        // Kiểm tra khoảng cách đến bóng
        float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);

        // Nhấn giữ chuột phải để nhặt bóng (chỉ khi ở gần và không cầm bóng)
        if (Input.GetMouseButtonDown(1) && !isHoldingBall && distanceToBall <= pickupRadius)
        {
            PickUpBall();
        }

        // Nhấn giữ chuột trái để canh lực ném
        if (Input.GetMouseButtonDown(0) && isHoldingBall)
        {
            isChargingThrow = true;
            chargeStartTime = Time.time;
            //animator.SetBool("IsCharging", true);
            if (powerSlider != null)
                powerSlider.gameObject.SetActive(true);
            trajectoryLine.enabled = true;
        }

        // Cập nhật chỉ báo khi đang canh lực
        if (isChargingThrow)
        {
            float holdTime = Time.time - chargeStartTime;
            float chargeFraction = Mathf.Clamp01(holdTime / chargeTime);

            // Cập nhật thanh lực UI
            if (powerSlider != null)
                powerSlider.value = chargeFraction;

            // Cập nhật đường quỹ đạo dự đoán
            UpdateTrajectory(chargeFraction);
        }

        // Thả chuột trái để ném bóng
        if (Input.GetMouseButtonUp(0) && isChargingThrow)
        {
            ThrowBall();
            isChargingThrow = false;
            //animator.SetBool("IsCharging", false);
            //animator.SetTrigger("Throw");
            if (powerSlider != null)
                powerSlider.gameObject.SetActive(false);
            trajectoryLine.enabled = false;
        }
    }

    void PickUpBall()
    {
        isHoldingBall = true;
        ballRigidbody.isKinematic = true; // Tắt vật lý
        ball.transform.position = ballHoldPosition.position;
        ball.transform.SetParent(ballHoldPosition); // Gắn bóng vào tay
        //animator.SetBool("IsHolding", true); // Kích hoạt animation cầm bóng
    }

    void ThrowBall()
    {
        if (!isHoldingBall) return;

        isHoldingBall = false;
        ball.transform.SetParent(null); // Tách bóng khỏi tay
        ballRigidbody.isKinematic = false; // Bật vật lý

        // Tính lực ném dựa trên thời gian giữ chuột
        float holdTime = Time.time - chargeStartTime;
        float chargeFraction = Mathf.Clamp01(holdTime / chargeTime);
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeFraction);

        // Tính hướng ném (phía trước nhân vật + lực hướng lên)
        throwDirection = transform.forward + Vector3.up * upwardForce;
        throwDirection = throwDirection.normalized;

        // Áp dụng lực ném
        ballRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        //animator.SetBool("IsHolding", false); // Tắt animation cầm bóng
    }

    void UpdateTrajectory(float chargeFraction)
    {
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeFraction);
        throwDirection = (transform.forward + Vector3.up * upwardForce).normalized;
        Vector3 startPos = ball.transform.position;
        Vector3 velocity = throwDirection * throwForce / ballRigidbody.mass;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * trajectoryTimeStep;
            Vector3 point = startPos + velocity * time + 0.5f * Physics.gravity * time * time;
            trajectoryLine.SetPosition(i, point);
        }
    }
}