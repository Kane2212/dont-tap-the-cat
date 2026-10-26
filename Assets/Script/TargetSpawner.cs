using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public static TargetSpawner Instance { get; private set; }

    [Header("Spawn Area")]
    [SerializeField] private Rect gameplayArea = new Rect(-3f, -4f, 6f, 8f);
    [SerializeField] private float minSpawnSpacing = 0.5f;
    [SerializeField] private float maxSpawnAttempts = 15f;

    [Header("Target Prefabs")]
    [SerializeField] private GameObject safeTargetPrefab;
    [SerializeField] private GameObject catPrefab;

    [Header("Target Definitions")]
    [SerializeField] private List<TargetDefinition> safeTargetDefinitions = new List<TargetDefinition>();
    [SerializeField] private List<TargetDefinition> catDefinitions = new List<TargetDefinition>();

    [SerializeField] private float spawnTimer;
    [SerializeField] private float catSpawnTimer;

    private readonly List<Target> activeTargets = new List<Target>();

    public Rect GameplayArea => gameplayArea;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (GamePlayManager.instance == null || GamePlayManager.instance.GetState() != GameState.Playing)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        catSpawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnSafeTarget();
            spawnTimer = GetSafeSpawnInterval();
        }

        if (catSpawnTimer <= 0f)
        {
            SpawnCat();
            catSpawnTimer = GetCatSpawnInterval();
        }
    }

    public void InitializeSession()
    {
        ClearActiveTargets();
        spawnTimer = 0f;
        catSpawnTimer = 0f;
    }

    public void SpawnSafeTarget()
    {
        if (DifficultyManager.Instance != null && activeTargets.Count >= DifficultyManager.Instance.GetMaxActiveSafeTargets())
        {
            return;
        }

        TargetDefinition definition = GetWeightedSafeDefinition();
        if (definition == null)
        {
            return;
        }

        SpawnTarget(definition, false);
    }

    public void SpawnCat()
    {
        if (DifficultyManager.Instance == null)
        {
            return;
        }

        if (activeTargets.Count >= DifficultyManager.Instance.GetMaxActiveCats())
        {
            return;
        }

        TargetDefinition definition = GetCatDefinition();
        if (definition == null)
        {
            return;
        }

        SpawnTarget(definition, true);
    }

    private void SpawnTarget(TargetDefinition definition, bool isCat)
    {
        if (definition == null)
        {
            return;
        }

        GameObject prefab = isCat ? catPrefab : safeTargetPrefab;
        if (prefab == null)
        {
            return;
        }

        Vector3 position = GetRandomSpawnPosition(definition);
        if (!IsInsideGameplayArea(position))
        {
            return;
        }

        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        Target target = obj.GetComponent<Target>();
        if (target == null)
        {
            target = obj.AddComponent<Target>();
        }

        target.Initialize(definition);
        activeTargets.Add(target);
    }

    public Vector3 GetRandomSpawnPosition(TargetDefinition definition)
    {
        for (int attempt = 0; attempt < Mathf.RoundToInt(maxSpawnAttempts); attempt++)
        {
            float x = UnityEngine.Random.Range(gameplayArea.xMin, gameplayArea.xMax);
            float y = UnityEngine.Random.Range(gameplayArea.yMin, gameplayArea.yMax);
            Vector3 candidate = new Vector3(x, y, 0f);

            if (IsValidPosition(candidate))
            {
                return candidate;
            }
        }

        return Vector3.zero;
    }

    private bool IsValidPosition(Vector3 candidate)
    {
        if (!IsInsideGameplayArea(candidate))
        {
            return false;
        }

        foreach (Target active in activeTargets)
        {
            if (active == null)
            {
                continue;
            }

            if (Vector3.Distance(active.transform.position, candidate) < minSpawnSpacing)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsInsideGameplayArea(Vector3 position)
    {
        return position.x >= gameplayArea.xMin && position.x <= gameplayArea.xMax && position.y >= gameplayArea.yMin && position.y <= gameplayArea.yMax;
    }

    private float GetSafeSpawnInterval()
    {
        if (DifficultyManager.Instance == null)
        {
            return 2f;
        }

        return DifficultyManager.Instance.GetSafeSpawnInterval();
    }

    private float GetCatSpawnInterval()
    {
        if (DifficultyManager.Instance == null)
        {
            return 4f;
        }

        return DifficultyManager.Instance.GetCatSpawnInterval();
    }

    private TargetDefinition GetWeightedSafeDefinition()
    {
        if (safeTargetDefinitions == null || safeTargetDefinitions.Count == 0)
        {
            return null;
        }

        List<TargetDefinition> eligible = new List<TargetDefinition>();
        float totalWeight = 0f;

        foreach (TargetDefinition definition in safeTargetDefinitions)
        {
            if (definition == null || definition.category != TargetCategory.Safe)
            {
                continue;
            }

            if (definition.targetId.Contains("Healing") && HealthManager.Instance != null && HealthManager.Instance.CurrentHp >= HealthManager.Instance.MaxHp)
            {
                continue;
            }

            eligible.Add(definition);
            totalWeight += Mathf.Max(0.01f, definition.spawnWeight);
        }

        if (eligible.Count == 0)
        {
            return null;
        }

        float randomWeight = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;

        foreach (TargetDefinition definition in eligible)
        {
            cumulative += Mathf.Max(0.01f, definition.spawnWeight);
            if (randomWeight <= cumulative)
            {
                return definition;
            }
        }

        return eligible[0];
    }

    private TargetDefinition GetCatDefinition()
    {
        if (catDefinitions == null || catDefinitions.Count == 0)
        {
            return null;
        }

        return catDefinitions[UnityEngine.Random.Range(0, catDefinitions.Count)];
    }

    public void ClearActiveTargets()
    {
        for (int i = activeTargets.Count - 1; i >= 0; i--)
        {
            if (activeTargets[i] != null)
            {
                Destroy(activeTargets[i].gameObject);
            }
        }

        activeTargets.Clear();
    }
}
