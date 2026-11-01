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

    private readonly List<Target> activeSafeTargets = new List<Target>();
    private readonly List<Target> activeCats = new List<Target>();

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
        RemoveInactiveTargets(activeSafeTargets);

        if (DifficultyManager.Instance != null && activeSafeTargets.Count >= DifficultyManager.Instance.GetMaxActiveSafeTargets())
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

        RemoveInactiveTargets(activeCats);

        if (activeCats.Count >= DifficultyManager.Instance.GetMaxActiveCats())
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

        if (!TryGetRandomSpawnPosition(definition, out Vector3 position))
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
        if (isCat)
        {
            activeCats.Add(target);
        }
        else
        {
            activeSafeTargets.Add(target);
        }
    }

    public Vector3 GetRandomSpawnPosition(TargetDefinition definition)
    {
        TryGetRandomSpawnPosition(definition, out Vector3 position);
        return position;
    }

    public bool TryGetRandomSpawnPosition(TargetDefinition definition, out Vector3 position)
    {
        position = Vector3.zero;

        for (int attempt = 0; attempt < Mathf.RoundToInt(maxSpawnAttempts); attempt++)
        {
            float x = UnityEngine.Random.Range(gameplayArea.xMin, gameplayArea.xMax);
            float y = UnityEngine.Random.Range(gameplayArea.yMin, gameplayArea.yMax);
            Vector3 candidate = new Vector3(x, y, 0f);

            if (IsValidPosition(candidate, definition))
            {
                position = candidate;
                return true;
            }
        }

        return false;
    }

    private bool IsValidPosition(Vector3 candidate, TargetDefinition definition)
    {
        if (!IsInsideGameplayArea(candidate, definition))
        {
            return false;
        }

        if (definition != null && definition.minSpawnDistanceFromCenter > 0f && candidate.magnitude < definition.minSpawnDistanceFromCenter)
        {
            return false;
        }

        RemoveInactiveTargets(activeSafeTargets);
        RemoveInactiveTargets(activeCats);

        float candidateRadius = GetTargetRadius(definition);

        foreach (Target active in activeSafeTargets)
        {
            if (Vector3.Distance(active.transform.position, candidate) < minSpawnSpacing + candidateRadius + GetTargetRadius(active.Definition))
            {
                return false;
            }
        }

        foreach (Target active in activeCats)
        {
            if (Vector3.Distance(active.transform.position, candidate) < minSpawnSpacing + candidateRadius + GetTargetRadius(active.Definition))
            {
                return false;
            }
        }

        return true;
    }

    public bool IsInsideGameplayArea(Vector3 position)
    {
        return IsInsideGameplayArea(position, null);
    }

    public bool IsInsideGameplayArea(Vector3 position, TargetDefinition definition)
    {
        float radius = GetTargetRadius(definition);
        return position.x - radius >= gameplayArea.xMin
            && position.x + radius <= gameplayArea.xMax
            && position.y - radius >= gameplayArea.yMin
            && position.y + radius <= gameplayArea.yMax;
    }

    private float GetTargetRadius(TargetDefinition definition)
    {
        if (definition == null)
        {
            return 0f;
        }

        return definition.colliderSize.magnitude * 0.5f;
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

            if (!definition.eligibleWhenHpFull && HealthManager.Instance != null && HealthManager.Instance.CurrentHp >= HealthManager.Instance.MaxHp)
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

        float randomWeight = Random.value * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < eligible.Count; i++)
        {
            cumulative += Mathf.Max(0.01f, eligible[i].spawnWeight);
            if (randomWeight <= cumulative)
            {
                return eligible[i];
            }
        }

        return eligible[eligible.Count - 1];
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
        DestroyTargets(activeSafeTargets);
        DestroyTargets(activeCats);
        activeSafeTargets.Clear();
        activeCats.Clear();
    }

    public void UnregisterTarget(Target target)
    {
        if (target == null)
        {
            return;
        }

        activeSafeTargets.Remove(target);
        activeCats.Remove(target);
    }

    private void DestroyTargets(List<Target> targets)
    {
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] != null)
            {
                Destroy(targets[i].gameObject);
            }
        }
    }

    private void RemoveInactiveTargets(List<Target> targets)
    {
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] == null)
            {
                targets.RemoveAt(i);
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(gameplayArea.center.x, gameplayArea.center.y, 0f), new Vector3(gameplayArea.width, gameplayArea.height, 0f));
    }
}
