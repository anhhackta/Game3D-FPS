using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBallController : MonoBehaviour
{
    public Transform ballHoldPosition;
    public GameObject ball;
    public float minThrowForce = 10f;
    public float maxThrowForce = 20f;
    public float upwardForce = 2.5f;
    public float chargeTime = 1f;
    public float pickupRadius = 2f;
    public Animator animator;
    public Slider powerSlider;
    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 30;
    public float trajectoryTimeStep = 0.1f;
    public float powerCycleTime = 2f;

    private bool isHoldingBall = false;
    private bool isChargingThrow = false;
    private float chargeStartTime;
    private Rigidbody ballRigidbody;
    private Vector3 throwDirection;

    void Start()
    {
        ballRigidbody = ball.GetComponent<Rigidbody>();
        isHoldingBall = false;
        ball.transform.SetParent(null);
        trajectoryLine.positionCount = trajectoryPoints;
        trajectoryLine.enabled = false;
        SetupLineRenderer();

        if (powerSlider != null)
            powerSlider.gameObject.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCrowdSound("CrowdCheer");
    }

    void Update()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;

        float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);

        // Chuột phải: Cầm hoặc thả bóng
        if (Input.GetMouseButton(1) && !isHoldingBall && distanceToBall <= pickupRadius)
        {
            PickUpBall();
        }
        else if (!Input.GetMouseButton(1) && isHoldingBall)
        {
            DropBall();
            if (isChargingThrow)
                CancelThrow();
        }

        // Chuột trái: Nạp lực và ném bóng
        if (Input.GetMouseButtonDown(0) && isHoldingBall && !isChargingThrow)
        {
            isChargingThrow = true;
            chargeStartTime = Time.time;
            if (powerSlider != null)
                powerSlider.gameObject.SetActive(true);
            trajectoryLine.enabled = true;
        }

        // Cập nhật nạp lực ném
        if (isChargingThrow)
        {
            // Nếu không cầm bóng (thả chuột phải), hủy ném
            if (!isHoldingBall)
            {
                CancelThrow();
                return;
            }

            float holdTime = Time.time - chargeStartTime;
            float chargeFraction;
            if (holdTime <= chargeTime)
            {
                chargeFraction = Mathf.Clamp01(holdTime / chargeTime);
            }
            else
            {
                float cycleTime = (holdTime - chargeTime) % powerCycleTime;
                chargeFraction = Mathf.Sin((cycleTime / powerCycleTime) * 2 * Mathf.PI) * 0.5f + 0.5f;
            }

            if (powerSlider != null)
                powerSlider.value = chargeFraction;

            UpdateTrajectory(chargeFraction);

            // Thả chuột trái để ném
            if (Input.GetMouseButtonUp(0))
            {
                if (isHoldingBall)
                    ThrowBall();
                CancelThrow();
            }
        }
    }

    void PickUpBall()
    {

        isHoldingBall = true;
        ballRigidbody.isKinematic = true;
        ball.transform.position = ballHoldPosition.position;
        ball.transform.SetParent(ballHoldPosition);

    }

    void DropBall()
    {
        isHoldingBall = false;
        ball.transform.SetParent(null);
        ballRigidbody.isKinematic = false;
    }

    void ThrowBall()
    {
        if (!isHoldingBall) return;

        isHoldingBall = false;
        ball.transform.SetParent(null);
        ballRigidbody.isKinematic = false;

        float holdTime = Time.time - chargeStartTime;
        float chargeFraction = holdTime <= chargeTime ? Mathf.Clamp01(holdTime / chargeTime) :
            Mathf.Sin(((holdTime - chargeTime) % powerCycleTime / powerCycleTime) * 2 * Mathf.PI) * 0.5f + 0.5f;
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeFraction);

        throwDirection = (transform.forward + Vector3.up * upwardForce).normalized;
        ballRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("ThrowBall");
    }

    void CancelThrow()
    {
        isChargingThrow = false;
        if (powerSlider != null)
            powerSlider.gameObject.SetActive(false);
        trajectoryLine.enabled = false;
    }

    void SetupLineRenderer()
    {
        trajectoryLine.startWidth = 0.1f;
        trajectoryLine.endWidth = 0.1f;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = Color.yellow;
        trajectoryLine.endColor = Color.yellow;

        Texture2D dashedTexture = new Texture2D(256, 1);
        for (int i = 0; i < 256; i++)
            dashedTexture.SetPixel(i, 0, i % 20 < 10 ? Color.white : Color.clear);
        dashedTexture.Apply();
        trajectoryLine.material.mainTexture = dashedTexture;
        trajectoryLine.material.mainTextureScale = new Vector2(10f, 1f);
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isHoldingBall && collision.gameObject.CompareTag("Ground"))
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("Bounce");
        }
    }
}