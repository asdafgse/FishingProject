using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public SaveData Data { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        Load();
    }

    public void Save()
    {
        if (Data == null)
        {
            Data = new SaveData();
        }

        SaveSystem.Save(Data);
    }

    public void Load()
    {
        Data = SaveSystem.Load();
    }

    public void ResetSave()
    {
        Data = new SaveData();
        Save();

        Debug.Log("최고 기록을 초기화했어.");
    }

    public void RecordCatch(float weight)
    {
        if (float.IsNaN(weight) ||
            float.IsInfinity(weight) ||
            weight < 0f)
        {
            return;
        }

        if (Data == null)
        {
            Load();
        }

        if (weight <= Data.bestCatchWeight)
        {
            return;
        }

        Data.bestCatchWeight = weight;
        Save();

        Debug.Log($"새 최고 기록: {weight:0.#} kg");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}