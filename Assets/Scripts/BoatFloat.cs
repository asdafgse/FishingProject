using UnityEngine;

public class BoatFloat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFSM gameFSM;

    [Header("Floating")]
    [SerializeField] private float floatHeight = 0.05f;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float tiltAngle = 1f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float waveTime;

    private void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        // 게임 종료 시 현재 위치에서 멈추기
        if (gameFSM == null ||
            gameFSM.CurrentState != gameFSM.PlayState)
        {
            return;
        }

        waveTime += Time.deltaTime * floatSpeed;

        float height = Mathf.Sin(waveTime) * floatHeight;
        float tilt = Mathf.Sin(waveTime * 0.8f) * tiltAngle;

        transform.localPosition =
            startPosition + Vector3.up * height;

        transform.localRotation =
            startRotation * Quaternion.Euler(0f, 0f, tilt);
    }
}