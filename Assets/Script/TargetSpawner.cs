using System.Collections.Generic;
using UnityEngine;

public enum SpawnPatternType
{
    SingleAppearance,
    DoubleTrouble,
    MixedTrio,
    BonusRush,
    CatAmbush,
    RecoveryWave
}

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
    private SpawnPatternType lastSelectedPattern;
    private bool hasSelectedPattern;

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

        if (spawnTimer <= 0f)
        {
            TryExecutePattern();
            spawnTimer = GetSafeSpawnInterval();
        }
    }

    public void InitializeSession()
    {
        ClearActiveTargets();
        spawnTimer = 0f;
        catSpawnTimer = 0f;
        hasSelectedPattern = false;
    }

    public bool TryExecutePattern()
    {
        if (GamePlayManager.instance == null || GamePlayManager.instance.GetState() != GameState.Playing)
        {
            return false;
        }

        GamePlayManager.instance.RecordPatternSelectionAttempt();
        if (!TrySelectPattern(out SpawnPatternType selectedPattern))
        {
            return false;
        }

        if (!TryBuildPatternDefinitions(selectedPattern, out List<TargetDefinition> definitions))
        {
            GamePlayManager.instance.RecordPatternMappingResult(selectedPattern, false);
            GamePlayManager.instance.RecordPatternOutcome(selectedPattern, false);
            return false;
        }

        GamePlayManager.instance.RecordPatternMappingResult(selectedPattern, true);
        bool spawned = TrySpawnGroup(definitions);
        GamePlayManager.instance.RecordAtomicSpawnResult(selectedPattern, spawned);
        GamePlayManager.instance.RecordPatternOutcome(selectedPattern, spawned);
        if (spawned)
        {
            RemoveInactiveTargets(activeSafeTargets);
            RemoveInactiveTargets(activeCats);
            RecordActiveTargetMetrics();
            GamePlayManager.instance.RecordPatternContext(
                selectedPattern,
                DifficultyManager.Instance.GetCurrentPhase(),
                DifficultyManager.Instance.GetCurrentLevel(),
                HealthManager.Instance != null ? HealthManager.Instance.CurrentHp : 0,
                activeSafeTargets.Count,
                activeCats.Count);
        }

        return spawned;
    }

    public bool TryBuildPatternDefinitions(SpawnPatternType pattern, out List<TargetDefinition> definitions)
    {
        definitions = new List<TargetDefinition>();

        List<TargetDefinition> eligibleSafeDefinitions = GetEligiblePatternDefinitions(TargetCategory.Safe);
        List<TargetDefinition> eligibleCatDefinitions = GetEligiblePatternDefinitions(TargetCategory.Cat);
        List<TargetDefinition> candidates = new List<TargetDefinition>();

        switch (pattern)
        {
            case SpawnPatternType.SingleAppearance:
                AddNormalSafeCandidates(eligibleSafeDefinitions, candidates);
                return TryAddRandomDefinitions(candidates, 1, definitions);

            case SpawnPatternType.DoubleTrouble:
                AddNormalSafeCandidates(eligibleSafeDefinitions, candidates);
                if (!TryAddRandomDefinitions(candidates, 1, definitions)
                    || !TryAddRandomDefinitions(eligibleCatDefinitions, 1, definitions))
                {
                    definitions.Clear();
                    return false;
                }

                return true;

            case SpawnPatternType.MixedTrio:
                AddMixedSafeCandidates(eligibleSafeDefinitions, candidates);
                if (!TryAddRandomDefinitions(candidates, 2, definitions)
                    || !TryAddRandomDefinitions(eligibleCatDefinitions, 1, definitions))
                {
                    definitions.Clear();
                    return false;
                }

                return true;

            case SpawnPatternType.BonusRush:
                AddBonusSafeCandidates(eligibleSafeDefinitions, candidates);
                int bonusCount = candidates.Count >= 3 ? UnityEngine.Random.Range(2, 4) : 2;
                if (!TryAddRandomDefinitions(candidates, bonusCount, definitions))
                {
                    definitions.Clear();
                    return false;
                }

                return true;

            case SpawnPatternType.CatAmbush:
                AddNormalSafeCandidates(eligibleSafeDefinitions, candidates);
                if (!TryAddRandomDefinitions(eligibleCatDefinitions, 2, definitions)
                    || !TryAddRandomDefinitions(candidates, 1, definitions))
                {
                    definitions.Clear();
                    return false;
                }

                return true;

            case SpawnPatternType.RecoveryWave:
                AddHealingSafeCandidates(eligibleSafeDefinitions, candidates);
                if (!TryAddRandomDefinitions(candidates, 1, definitions))
                {
                    definitions.Clear();
                    return false;
                }

                candidates.Clear();
                AddNormalSafeCandidates(eligibleSafeDefinitions, candidates);
                if (!TryAddRandomDefinitions(candidates, 1, definitions))
                {
                    definitions.Clear();
                    return false;
                }

                return true;

            default:
                definitions.Clear();
                return false;
        }
    }

    public bool TrySelectPattern(out SpawnPatternType selectedPattern)
    {
        selectedPattern = SpawnPatternType.SingleAppearance;

        if (DifficultyManager.Instance == null)
        {
            return false;
        }

        UnlockPhase currentPhase = DifficultyManager.Instance.GetCurrentPhase();
        bool recoveryWaveEligible = IsRecoveryWaveEligible();
        bool catAmbushEligible = !hasSelectedPattern || lastSelectedPattern != SpawnPatternType.CatAmbush;

        int singleAppearanceWeight = GetPatternWeight(currentPhase, SpawnPatternType.SingleAppearance);
        int doubleTroubleWeight = GetPatternWeight(currentPhase, SpawnPatternType.DoubleTrouble);
        int mixedTrioWeight = GetPatternWeight(currentPhase, SpawnPatternType.MixedTrio);
        int bonusRushWeight = GetPatternWeight(currentPhase, SpawnPatternType.BonusRush);
        int catAmbushWeight = catAmbushEligible
            ? GetPatternWeight(currentPhase, SpawnPatternType.CatAmbush)
            : 0;
        int recoveryWaveWeight = recoveryWaveEligible
            ? GetPatternWeight(currentPhase, SpawnPatternType.RecoveryWave)
            : 0;

        int totalWeight = singleAppearanceWeight
            + doubleTroubleWeight
            + mixedTrioWeight
            + bonusRushWeight
            + catAmbushWeight
            + recoveryWaveWeight;

        if (totalWeight <= 0)
        {
            return false;
        }

        int randomWeight = UnityEngine.Random.Range(0, totalWeight);
        if (randomWeight < singleAppearanceWeight)
        {
            selectedPattern = SpawnPatternType.SingleAppearance;
        }
        else
        {
            randomWeight -= singleAppearanceWeight;
            if (randomWeight < doubleTroubleWeight)
            {
                selectedPattern = SpawnPatternType.DoubleTrouble;
            }
            else
            {
                randomWeight -= doubleTroubleWeight;
                if (randomWeight < mixedTrioWeight)
                {
                    selectedPattern = SpawnPatternType.MixedTrio;
                }
                else
                {
                    randomWeight -= mixedTrioWeight;
                    if (randomWeight < bonusRushWeight)
                    {
                        selectedPattern = SpawnPatternType.BonusRush;
                    }
                    else
                    {
                        randomWeight -= bonusRushWeight;
                        if (randomWeight < catAmbushWeight)
                        {
                            selectedPattern = SpawnPatternType.CatAmbush;
                        }
                        else
                        {
                            selectedPattern = SpawnPatternType.RecoveryWave;
                        }
                    }
                }
            }
        }

        lastSelectedPattern = selectedPattern;
        hasSelectedPattern = true;
        return true;
    }

    private List<TargetDefinition> GetEligiblePatternDefinitions(TargetCategory category)
    {
        List<TargetDefinition> eligibleDefinitions = new List<TargetDefinition>();
        List<TargetDefinition> sourceDefinitions = category == TargetCategory.Cat
            ? catDefinitions
            : safeTargetDefinitions;

        if (sourceDefinitions == null)
        {
            return eligibleDefinitions;
        }

        foreach (TargetDefinition definition in sourceDefinitions)
        {
            if (definition != null
                && definition.category == category
                && IsDefinitionEligibleForSpawn(definition))
            {
                eligibleDefinitions.Add(definition);
            }
        }

        return eligibleDefinitions;
    }

    private void AddNormalSafeCandidates(List<TargetDefinition> source, List<TargetDefinition> candidates)
    {
        foreach (TargetDefinition definition in source)
        {
            if (definition.movementMode == MovementMode.Static
                && definition.scoreValue == 1
                && definition.healAmount == 0
                && definition.damageAmount == 0
                && definition.expireDamage > 0
                && !definition.triggersGameOver)
            {
                candidates.Add(definition);
            }
        }
    }

    private void AddMixedSafeCandidates(List<TargetDefinition> source, List<TargetDefinition> candidates)
    {
        foreach (TargetDefinition definition in source)
        {
            if (!IsBonusSafeDefinition(definition)
                && !IsHealingSafeDefinition(definition)
                && !IsRareSafeDefinition(definition))
            {
                candidates.Add(definition);
            }
        }
    }

    private void AddBonusSafeCandidates(List<TargetDefinition> source, List<TargetDefinition> candidates)
    {
        foreach (TargetDefinition definition in source)
        {
            if (IsBonusSafeDefinition(definition))
            {
                candidates.Add(definition);
            }
        }
    }

    private void AddHealingSafeCandidates(List<TargetDefinition> source, List<TargetDefinition> candidates)
    {
        foreach (TargetDefinition definition in source)
        {
            if (IsHealingSafeDefinition(definition))
            {
                candidates.Add(definition);
            }
        }
    }

    private bool IsBonusSafeDefinition(TargetDefinition definition)
    {
        return definition.category == TargetCategory.Safe
            && definition.movementMode == MovementMode.Dynamic
            && definition.scoreValue > 1
            && definition.healAmount == 0
            && definition.damageAmount == 0
            && definition.expireDamage == 0
            && !definition.triggersGameOver;
    }

    private bool IsHealingSafeDefinition(TargetDefinition definition)
    {
        return definition.category == TargetCategory.Safe && definition.healAmount > 0;
    }

    private bool IsRareSafeDefinition(TargetDefinition definition)
    {
        return definition.category == TargetCategory.Safe && definition.movementMode == MovementMode.Blink;
    }

    private bool TryAddRandomDefinitions(List<TargetDefinition> candidates, int count, List<TargetDefinition> definitions)
    {
        if (candidates == null || definitions == null || count <= 0 || candidates.Count < count)
        {
            return false;
        }

        List<TargetDefinition> remaining = new List<TargetDefinition>(candidates);
        for (int i = 0; i < count; i++)
        {
            int selectedIndex = UnityEngine.Random.Range(0, remaining.Count);
            definitions.Add(remaining[selectedIndex]);
            remaining.RemoveAt(selectedIndex);
        }

        return true;
    }

    private int GetPatternWeight(UnlockPhase phase, SpawnPatternType patternType)
    {
        switch (phase)
        {
            case UnlockPhase.Early:
                if (patternType == SpawnPatternType.SingleAppearance)
                {
                    return 60;
                }

                if (patternType == SpawnPatternType.DoubleTrouble)
                {
                    return 40;
                }

                return 0;

            case UnlockPhase.Middle:
                switch (patternType)
                {
                    case SpawnPatternType.SingleAppearance:
                        return 25;
                    case SpawnPatternType.DoubleTrouble:
                        return 30;
                    case SpawnPatternType.MixedTrio:
                        return 20;
                    case SpawnPatternType.BonusRush:
                        return 10;
                    case SpawnPatternType.CatAmbush:
                        return 5;
                    case SpawnPatternType.RecoveryWave:
                        return 10;
                    default:
                        return 0;
                }

            case UnlockPhase.Late:
                switch (patternType)
                {
                    case SpawnPatternType.SingleAppearance:
                        return 10;
                    case SpawnPatternType.DoubleTrouble:
                        return 20;
                    case SpawnPatternType.MixedTrio:
                        return 25;
                    case SpawnPatternType.BonusRush:
                        return 15;
                    case SpawnPatternType.CatAmbush:
                        return 20;
                    case SpawnPatternType.RecoveryWave:
                        return 10;
                    default:
                        return 0;
                }

            default:
                return 0;
        }
    }

    private bool IsRecoveryWaveEligible()
    {
        if (HealthManager.Instance == null
            || HealthManager.Instance.CurrentHp >= HealthManager.Instance.MaxHp)
        {
            return false;
        }

        if (safeTargetDefinitions == null)
        {
            return false;
        }

        foreach (TargetDefinition definition in safeTargetDefinitions)
        {
            if (definition != null
                && definition.category == TargetCategory.Safe
                && definition.healAmount > 0
                && IsDefinitionEligibleForSpawn(definition))
            {
                return true;
            }
        }

        return false;
    }

    public void SpawnSafeTarget()
    {
        if (!CanSpawnTarget(false))
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
        if (!CanSpawnTarget(true))
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

    public bool TrySpawnGroup(IList<TargetDefinition> definitions)
    {
        if (definitions == null || definitions.Count == 0)
        {
            return false;
        }

        if (GamePlayManager.instance == null || GamePlayManager.instance.GetState() != GameState.Playing)
        {
            return false;
        }

        if (DifficultyManager.Instance == null)
        {
            return false;
        }

        RemoveInactiveTargets(activeSafeTargets);
        RemoveInactiveTargets(activeCats);

        int groupSafeCount = 0;
        int groupCatCount = 0;

        for (int i = 0; i < definitions.Count; i++)
        {
            TargetDefinition definition = definitions[i];
            if (!IsDefinitionEligibleForSpawn(definition))
            {
                return false;
            }

            bool isCat = definition.category == TargetCategory.Cat;
            if (isCat)
            {
                if (catPrefab == null)
                {
                    return false;
                }

                groupCatCount++;
            }
            else
            {
                if (safeTargetPrefab == null)
                {
                    return false;
                }

                groupSafeCount++;
            }
        }

        int activeSafeCount = activeSafeTargets.Count;
        int activeCatCount = activeCats.Count;
        int activeTargetCount = activeSafeCount + activeCatCount;
        DifficultyManager difficultyManager = DifficultyManager.Instance;

        if (activeTargetCount + definitions.Count > difficultyManager.GetMaxActiveTargets()
            || activeSafeCount + groupSafeCount > difficultyManager.GetMaxActiveSafeTargets()
            || activeCatCount + groupCatCount > difficultyManager.GetMaxActiveCats())
        {
            return false;
        }

        List<Vector3> positions = new List<Vector3>(definitions.Count);
        List<TargetDefinition> reservedDefinitions = new List<TargetDefinition>(definitions.Count);
        for (int i = 0; i < definitions.Count; i++)
        {
            if (!TryGetRandomSpawnPosition(definitions[i], positions, reservedDefinitions, out Vector3 position))
            {
                return false;
            }

            positions.Add(position);
            reservedDefinitions.Add(definitions[i]);
        }

        List<Target> spawnedTargets = new List<Target>(definitions.Count);
        for (int i = 0; i < definitions.Count; i++)
        {
            bool isCat = definitions[i].category == TargetCategory.Cat;
            Target target = InstantiateTarget(definitions[i], isCat, positions[i]);
            if (target == null)
            {
                for (int spawnedIndex = spawnedTargets.Count - 1; spawnedIndex >= 0; spawnedIndex--)
                {
                    UnregisterTarget(spawnedTargets[spawnedIndex]);
                    Destroy(spawnedTargets[spawnedIndex].gameObject);
                }

                return false;
            }

            spawnedTargets.Add(target);
        }

        return true;
    }

    private bool CanSpawnTarget(bool isCat)
    {
        if (DifficultyManager.Instance == null)
        {
            return false;
        }

        RemoveInactiveTargets(activeSafeTargets);
        RemoveInactiveTargets(activeCats);

        int activeSafeCount = activeSafeTargets.Count;
        int activeCatCount = activeCats.Count;
        int activeTargetCount = activeSafeCount + activeCatCount;

        if (activeTargetCount >= DifficultyManager.Instance.GetMaxActiveTargets())
        {
            return false;
        }

        if (isCat)
        {
            return activeCatCount < DifficultyManager.Instance.GetMaxActiveCats();
        }

        return activeSafeCount < DifficultyManager.Instance.GetMaxActiveSafeTargets();
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

        InstantiateTarget(definition, isCat, position);
    }

    private Target InstantiateTarget(TargetDefinition definition, bool isCat, Vector3 position)
    {
        GameObject prefab = isCat ? catPrefab : safeTargetPrefab;
        if (prefab == null)
        {
            return null;
        }

        GameObject obj = null;
        Target target = null;
        bool isRegistered = false;

        try
        {
            obj = Instantiate(prefab, position, Quaternion.identity);
            if (obj == null)
            {
                return null;
            }

            target = obj.GetComponent<Target>();
            if (target == null)
            {
                target = obj.AddComponent<Target>();
            }

            if (target == null)
            {
                throw new System.InvalidOperationException("Target component could not be created.");
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

            RecordActiveTargetMetrics();

            isRegistered = true;
            return target;
        }
        catch (System.Exception exception)
        {
            if (isRegistered && target != null)
            {
                UnregisterTarget(target);
            }

            if (obj != null)
            {
                Destroy(obj);
            }

            Debug.LogException(exception);
            return null;
        }
    }

    public Vector3 GetRandomSpawnPosition(TargetDefinition definition)
    {
        TryGetRandomSpawnPosition(definition, out Vector3 position);
        return position;
    }

    public bool TryGetRandomSpawnPosition(TargetDefinition definition, out Vector3 position)
    {
        return TryGetRandomSpawnPosition(definition, null, null, out position);
    }

    private bool TryGetRandomSpawnPosition(TargetDefinition definition, IList<Vector3> reservedPositions, IList<TargetDefinition> reservedDefinitions, out Vector3 position)
    {
        position = Vector3.zero;

        for (int attempt = 0; attempt < Mathf.RoundToInt(maxSpawnAttempts); attempt++)
        {
            float x = UnityEngine.Random.Range(gameplayArea.xMin, gameplayArea.xMax);
            float y = UnityEngine.Random.Range(gameplayArea.yMin, gameplayArea.yMax);
            Vector3 candidate = new Vector3(x, y, 0f);

            if (IsValidPosition(candidate, definition, reservedPositions, reservedDefinitions))
            {
                position = candidate;
                return true;
            }
        }

        return false;
    }

    private bool IsValidPosition(Vector3 candidate, TargetDefinition definition)
    {
        return IsValidPosition(candidate, definition, null, null);
    }

    private bool IsValidPosition(Vector3 candidate, TargetDefinition definition, IList<Vector3> reservedPositions, IList<TargetDefinition> reservedDefinitions)
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

        if (reservedPositions != null)
        {
            for (int i = 0; i < reservedPositions.Count; i++)
            {
                float reservedRadius = GetTargetRadius(reservedDefinitions[i]);
                if (Vector3.Distance(reservedPositions[i], candidate) < minSpawnSpacing + candidateRadius + reservedRadius)
                {
                    return false;
                }
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

            if (!IsDefinitionEligibleForSpawn(definition))
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

    private bool IsDefinitionEligibleForSpawn(TargetDefinition definition)
    {
        if (definition == null || DifficultyManager.Instance == null)
        {
            return false;
        }

        if (definition.unlockPhase > DifficultyManager.Instance.GetCurrentPhase())
        {
            return false;
        }

        if (definition.category == TargetCategory.Safe
            && !definition.eligibleWhenHpFull
            && HealthManager.Instance != null
            && HealthManager.Instance.CurrentHp >= HealthManager.Instance.MaxHp)
        {
            return false;
        }

        return true;
    }

    private TargetDefinition GetCatDefinition()
    {
        if (catDefinitions == null || catDefinitions.Count == 0)
        {
            return null;
        }

        if (DifficultyManager.Instance == null)
        {
            return null;
        }

        UnlockPhase currentPhase = DifficultyManager.Instance.GetCurrentPhase();
        int eligibleCount = 0;

        foreach (TargetDefinition definition in catDefinitions)
        {
            if (definition != null && definition.unlockPhase <= currentPhase)
            {
                eligibleCount++;
            }
        }

        if (eligibleCount == 0)
        {
            return null;
        }

        int selectedIndex = UnityEngine.Random.Range(0, eligibleCount);
        foreach (TargetDefinition definition in catDefinitions)
        {
            if (definition == null || definition.unlockPhase > currentPhase)
            {
                continue;
            }

            if (selectedIndex == 0)
            {
                return definition;
            }

            selectedIndex--;
        }

        return null;
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
        RecordActiveTargetMetrics();
    }

    private void RecordActiveTargetMetrics()
    {
        GamePlayManager.instance?.RecordActiveTargetCounts(activeSafeTargets.Count, activeCats.Count);
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
            if (targets[i] == null || targets[i].IsResolved)
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
