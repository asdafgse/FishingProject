using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFSM gameFSM;
    [SerializeField] private FishController fishPrefab;

    [Header("Population")]
    [SerializeField, Min(1)] private int targetCount = 9;
    [SerializeField, Min(0.1f)] private float respawnInterval = 3f;

    private FishController spawnTemplate;
    private float respawnTimer;
    private bool initialized;

    private void Awake()
    {
        // 연결이 비어 있으면 씬에서 자동으로 찾기
        if (gameFSM == null)
        {
            gameFSM = FindFirstObjectByType<GameFSM>();
        }

        if (fishPrefab == null)
        {
            fishPrefab = FindFirstObjectByType<FishController>();
        }

        if (fishPrefab == null)
        {
            Debug.LogError(
                "FishSpawner: 씬에 FishController가 붙은 물고기가 필요합니다.",
                this
            );

            enabled = false;
            return;
        }

        // 비활성 부모 아래에 복사해 시작 동작이 실행되지 않게 보관
        GameObject storage = new GameObject("FishSpawnTemplate");
        storage.transform.SetParent(transform, false);
        storage.SetActive(false);

        spawnTemplate = Instantiate(
            fishPrefab,
            storage.transform
        );

        spawnTemplate.name = "FishTemplate";
        spawnTemplate.gameObject.SetActive(false);
        spawnTemplate.enabled = true;

        if (gameFSM == null)
        {
            Debug.LogError(
                "FishSpawner: GameFSM을 찾을 수 없습니다.",
                this
            );
        }
    }

    private void Update()
    {
        if (gameFSM == null || spawnTemplate == null)
            return;

        if (gameFSM.CurrentState != gameFSM.PlayState)
            return;

        if (!initialized)
        {
            initialized = true;

            int missing = targetCount - CountFish();

            for (int i = 0; i < missing; i++)
            {
                SpawnFish();
            }

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
            {
                count++;
            }
        }

        return count;
    }

    private void SpawnFish()
    {
        FishController fish = Instantiate(
            spawnTemplate,
            Vector3.zero,
            Quaternion.identity
        );

        fish.name = "Fish";
        fish.SetGameFSM(gameFSM);
        fish.enabled = true;
        fish.gameObject.SetActive(true);
    }
}