using UnityEngine;

public class SeaweedSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float swayAngle = 5f;
    [SerializeField] private float swaySpeed = 1.5f;

    private Quaternion startRotation;
    private float elapsedTime;
    private float phase;

    private void Awake()
    {
        startRotation = transform.localRotation;

        // 복제한 해초들이 서로 다른 타이밍으로 흔들리기
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        float angle = Mathf.Sin(
            elapsedTime * swaySpeed + phase
        ) * swayAngle;

        transform.localRotation =
            startRotation * Quaternion.Euler(0f, 0f, angle);
    }

    private void OnDisable()
    {
        transform.localRotation = startRotation;
    }
}