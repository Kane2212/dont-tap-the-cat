using UnityEngine;

public enum TargetType
{
    Safe,
    Cat
}

public class Target : MonoBehaviour
{
    [SerializeField] private TargetDefinition definition;
    private Collider2D targetCollider;
    private TargetBehaviour behaviour;
    private float lifetimeRemaining;
    private bool isResolved;

    public TargetDefinition Definition => definition;
    public bool IsResolved => isResolved;
    public TargetType Type => definition != null && definition.category == TargetCategory.Cat ? TargetType.Cat : TargetType.Safe;

    private void Awake()
    {
        targetCollider = GetComponent<Collider2D>();
        behaviour = GetComponent<TargetBehaviour>();
        if (behaviour == null)
        {
            behaviour = gameObject.AddComponent<TargetBehaviour>();
        }
    }

    private void Update()
    {
        if (GamePlayManager.instance == null || GamePlayManager.instance.GetState() != GameState.Playing)
        {
            return;
        }

        if (definition == null)
        {
            return;
        }

        lifetimeRemaining -= Time.deltaTime;
        behaviour?.Tick(Time.deltaTime);

        if (lifetimeRemaining <= 0f && !isResolved)
        {
            ResolveExpired();
        }
    }

    public void Initialize(TargetDefinition targetDefinition)
    {
        definition = targetDefinition;
        if (definition == null)
        {
            return;
        }

        lifetimeRemaining = definition.lifetime;
        behaviour?.Initialize(this, definition);
        if (targetCollider == null)
        {
            targetCollider = GetComponent<Collider2D>();
        }
    }

    public void ResolveTap()
    {
        if (isResolved || GamePlayManager.instance == null)
        {
            return;
        }

        if (GamePlayManager.instance.GetState() != GameState.Playing)
        {
            return;
        }

        isResolved = true;

        if (definition == null)
        {
            UnregisterFromSpawner();
            Destroy(gameObject);
            return;
        }

        if (definition.category == TargetCategory.Cat)
        {
            GamePlayManager.instance.TriggerGameOver("Cat tapped");
            PlaySound("Cat1");
            UnregisterFromSpawner();
            Destroy(gameObject);
            return;
        }

        if (definition.scoreValue > 0)
        {
            ScoreManager.Instance?.AddScore(definition.scoreValue); 
            PlaySound("Score");
        }

        if (definition.healAmount > 0)
        {
            HealthManager.Instance?.Heal(definition.healAmount);
            PlaySound("Heal");
        }

        if (definition.damageAmount > 0)
        {
            HealthManager.Instance?.TakeDamage(definition.damageAmount);
            PlaySound("Damage");
        }

        if (definition.triggersGameOver)
        {
            GamePlayManager.instance.TriggerGameOver("Fatal target resolved");
        }

        UnregisterFromSpawner();
        Destroy(gameObject);
    }

    public void ResolveExpired()
    {
        if (isResolved)
        {
            return;
        }

        isResolved = true;

        if (definition == null)
        {
            UnregisterFromSpawner();
            Destroy(gameObject);
            return;
        }

        if (definition.category == TargetCategory.Cat)
        {
            UnregisterFromSpawner();
            Destroy(gameObject);
            return;
        }

        if (definition.expireDamage > 0)
        {
            HealthManager.Instance?.TakeDamage(definition.expireDamage);
        }

        UnregisterFromSpawner();
        Destroy(gameObject);
    }

    private void UnregisterFromSpawner()
    {
        TargetSpawner.Instance?.UnregisterTarget(this);
    }

    private void PlaySound(string soundName)
    {
        SoundManager.GetInstance()?.Play(soundName);
    }

    public bool ContainsPoint(Vector2 worldPosition)
    {
        if (targetCollider == null)
        {
            return false;
        }

        return targetCollider.OverlapPoint(worldPosition);
    }

    public void SetPosition(Vector2 newPosition)
    {
        transform.position = newPosition;
    }
}
