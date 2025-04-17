using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBallController : MonoBehaviour
{
    public Transform ballHoldPosition; // Vị trí trên tay nhân vật để cầm bóng
    public GameObject ball; // Đối tượng bóng
    public GameObject arrow; // Mũi tên ở điểm cuối quỹ đạo
    public float minThrowForce = 5f; // Lực ném tối thiểu
    public float maxThrowForce = 20f; // Lực ném tối đa
    public float upwardForce = 5f; // Lực hướng lên để tạo quỹ đạo parabol
    public float chargeTime = 2f; // Thời gian để thanh lực đạt max hoặc min
    public float pickupRadius = 2f; // Bán kính để nhặt bóng
    public Animator animator; // Animator của nhân vật
    public Slider powerSlider; // Thanh lực UI
    public LineRenderer trajectoryLine; // LineRenderer cho quỹ đạo dự đoán
    public int trajectoryPoints = 30; // Số điểm trên đường quỹ đạo
    public float trajectoryTimeStep = 0.1f; // Khoảng thời gian giữa các điểm
    public GameManager gameManager; // Tham chiếu đến GameManager

    private bool isHoldingBall = false; // Trạng thái cầm bóng
    private bool isChargingThrow = false; // Trạng thái đang canh lực ném
    private float chargeStartTime; // Thời gian bắt đầu giữ chuột trái
    private Rigidbody ballRigidbody;
    private Vector3 throwDirection;
    private float chargeCycleTime; // Thời gian chu kỳ của thanh lực

    void Start()
    {
        ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.isKinematic = true;
        ball.transform.position = ballHoldPosition.position;
        ball.transform.SetParent(ballHoldPosition);
        isHoldingBall = true;
        //animator.SetBool("IsHolding", true);

        // Thiết lập LineRenderer
        trajectoryLine.positionCount = trajectoryPoints;
        trajectoryLine.enabled = false;
        SetupDashedLine();

        // Ẩn thanh lực và mũi tên ban đầu
        if (powerSlider != null)
            powerSlider.gameObject.SetActive(false);
        if (arrow != null)
            arrow.SetActive(false);
    }

    void Update()
    {
        // Kiểm tra khoảng cách đến bóng
        float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);

        // Nhấn giữ chuột phải để cầm bóng (chỉ khi ở gần và không cầm)
        if (Input.GetMouseButton(1) && !isHoldingBall && distanceToBall <= pickupRadius)
        {
            PickUpBall();
        }
        // Thả chuột phải để thả bóng
        else if (Input.GetMouseButtonUp(1) && isHoldingBall)
        {
            DropBall();
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
            if (arrow != null)
                arrow.SetActive(true);
        }

        // Cập nhật chỉ báo khi đang canh lực
        if (isChargingThrow)
        {
            float holdTime = Time.time - chargeStartTime;
            // Tính giá trị lực theo chu kỳ (tăng -> max -> giảm -> min -> lặp lại)
            chargeCycleTime = (holdTime % (chargeTime * 2)) / chargeTime;
            float chargeFraction = chargeCycleTime <= 1f ? chargeCycleTime : 2f - chargeCycleTime;

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
            if (arrow != null)
                arrow.SetActive(false);
        }
    }

    void PickUpBall()
    {
        isHoldingBall = true;
        ballRigidbody.isKinematic = true;
        ball.transform.position = ballHoldPosition.position;
        ball.transform.SetParent(ballHoldPosition);
        //animator.SetBool("IsHolding", true);
    }

    void DropBall()
    {
        isHoldingBall = false;
        ball.transform.SetParent(null);
        ballRigidbody.isKinematic = false;
        //animator.SetBool("IsHolding", false);
        //animator.SetTrigger("Drop");
    }

    void ThrowBall()
    {
        if (!isHoldingBall) return;

        isHoldingBall = false;
        ball.transform.SetParent(null);
        ballRigidbody.isKinematic = false;

        // Tính lực ném dựa trên thanh lực
        float chargeFraction = powerSlider != null ? powerSlider.value : 1f;
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeFraction);

        // Tính hướng ném
        throwDirection = (transform.forward + Vector3.up * upwardForce).normalized;

        // Áp dụng lực ném
        ballRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        //animator.SetBool("IsHolding", false);
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

            // Đặt vị trí và hướng mũi tên ở điểm cuối
            if (i == trajectoryPoints - 1 && arrow != null)
            {
                arrow.transform.position = point;
                Vector3 nextPoint = startPos + velocity * (time + trajectoryTimeStep) + 0.5f * Physics.gravity * (time + trajectoryTimeStep) * (time + trajectoryTimeStep);
                arrow.transform.rotation = Quaternion.LookRotation(nextPoint - point);
            }
        }
    }

    void SetupDashedLine()
    {
        // Thiết lập LineRenderer thành nét đứt
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = Color.yellow;
        trajectoryLine.endColor = Color.yellow;
        trajectoryLine.startWidth = 0.1f;
        trajectoryLine.endWidth = 0.1f;
        trajectoryLine.material.mainTextureScale = new Vector2(10f, 1f); // Tạo hiệu ứng nét đứt
        trajectoryLine.material.mainTexture = CreateDashTexture();
    }

    Texture2D CreateDashTexture()
    {
        Texture2D texture = new Texture2D(16, 1);
        for (int i = 0; i < 16; i++)
        {
            texture.SetPixel(i, 0, i < 8 ? Color.white : Color.clear);
        }
        texture.Apply();
        return texture;
    }

    void OnDrawGizmos()
    {
        // Vẽ bán kính nhặt bóng trong Editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}