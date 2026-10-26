using UnityEngine;

public enum TargetCategory
{
    Safe,
    Cat
}

public enum MovementMode
{
    Static,
    Patrol,
    Dynamic,
    Blink
}

public enum UnlockPhase
{
    Early,
    Middle,
    Late
}

[System.Serializable]
public class TargetEffect
{
    public enum EffectType
    {
        Score,
        Heal,
        Damage,
        GameOver
    }

    public EffectType type;
    public int amount;
}

[CreateAssetMenu(fileName = "TargetDefinition", menuName = "DontTapTheCat/Target Definition")]
public class TargetDefinition : ScriptableObject
{
    [Header("Identity")]
    public string targetId = "Target";
    public TargetCategory category = TargetCategory.Safe;
    public UnlockPhase unlockPhase = UnlockPhase.Early;
    public MovementMode movementMode = MovementMode.Static;

    [Header("Core Gameplay")]
    public float lifetime = 3f;
    public int scoreValue = 1;
    public int healAmount = 0;
    public int damageAmount = 0;
    public int expireDamage = 0;
    public bool triggersGameOver = false;

    [Header("Spawn")]
    public float spawnWeight = 1f;
    public bool eligibleWhenHpFull = true;
    public float minSpawnDistanceFromCenter = 0.25f;

    [Header("Movement")]
    public float movementSpeed = 1f;
    public float patrolDistance = 1.2f;
    public Vector2 patrolDirection = Vector2.right;
    public float dynamicMaxSpeed = 2.5f;
    public float dynamicAcceleration = 1.5f;
    public float blinkWarningTime = 0.75f;
    public float blinkMovementDuration = 0.2f;

    [Header("Visual")]
    public Sprite sprite;
    public Color spriteColor = Color.white;
    public Vector2 colliderSize = new Vector2(0.7f, 0.7f);

    [Header("Effects")]
    public TargetEffect[] effects = new TargetEffect[0];
}
