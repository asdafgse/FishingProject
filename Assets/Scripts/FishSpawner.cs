using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFSM gameFSM;
    [SerializeField] private FishController fishPrefab;

    [Header("Population")]
    [SerializeField, Min(1)] private int targetCount = 9;
    [SerializeField, Min(0.1f)] private float respawnInterval = 3f;

    private float respawnTimer;
    private bool initialized;

    private void Update()
    {
        if (gameFSM == null || fishPrefab == null)
            return;

        if (gameFSM.CurrentState != gameFSM.PlayState)
            return;

        // 게임 시작 시 부족한 수만큼 채우기
        if (!initialized)
        {
            initialized = true;

            int missing = targetCount - CountFish();

            for (int i = 0; i < missing; i++)
                SpawnFish();

            respawnTimer = 0f;
            return;
        }

        if (CountFish() >= targetCount)
        {
            respawnTimer = 0f;
            return;
        }

        respawnTimer += Time.deltaTime;

        if (respawnTimer >= respawnInterval)
        {
            respawnTimer = 0f;
            SpawnFish();
        }
    }

    private int CountFish()
    {
        FishController[] fishes =
            FindObjectsByType<FishController>(
                FindObjectsSortMode.None
            );

        int count = 0;

        foreach (FishController fish in fishes)
        {
            if (fish != null && fish.isActiveAndEnabled)
                count++;
        }

        return count;
    }

    private void SpawnFish()
    {
        FishController fish = Instantiate(
            fishPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        // 프리팹에는 씬의 GameFSM을 저장할 수 없으므로
        // 생성 직후 연결
        fish.SetGameFSM(gameFSM);

        fish.gameObject.SetActive(true);
    }
}