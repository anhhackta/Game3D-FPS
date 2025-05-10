using UnityEngine;

public class SlowSpin : MonoBehaviour
{
    public float rotationSpeed = 30f;

    void Update()
    {
        // Quay đều bất kể game bị pause (Time.timeScale = 0)
        transform.Rotate(0, 0, -rotationSpeed * Time.unscaledDeltaTime);
    }
}
