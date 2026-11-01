using UnityEngine;

public class TargetBehaviour : MonoBehaviour
{
    private Target target;
    private TargetDefinition definition;
    private Vector3 startPosition;
    private Vector2 movementDirection = Vector2.right;
    private float patrolTravel;
    private float timer;
    private float blinkWarningTimer;
    private bool blinkWarningShown;
    private Vector3 pendingBlinkPosition;
    private SpriteRenderer spriteRenderer;
    private Color baseColor = Color.white;

    public void Initialize(Target owner, TargetDefinition def)
    {
        target = owner;
        definition = def;
        startPosition = owner.transform.position;
        timer = 0f;
        blinkWarningTimer = 0f;
        blinkWarningShown = false;

        if (definition == null)
        {
            return;
        }

        if (definition.movementMode == MovementMode.Patrol)
        {
            movementDirection = definition.patrolDirection == Vector2.zero ? Vector2.right : definition.patrolDirection.normalized;
            patrolTravel = definition.patrolDistance;
        }
        else if (definition.movementMode == MovementMode.Dynamic)
        {
            movementDirection = UnityEngine.Random.insideUnitCircle.normalized;
        }

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseColor = definition.spriteColor;
            spriteRenderer.color = baseColor;
        }
    }

    public void Tick(float deltaTime)
    {
        if (target == null || target.IsResolved || definition == null)
        {
            return;
        }

        if (GamePlayManager.instance == null || GamePlayManager.instance.GetState() != GameState.Playing)
        {
            return;
        }

        timer += deltaTime;

        switch (definition.movementMode)
        {
            case MovementMode.Patrol:
                UpdatePatrol(deltaTime);
                break;
            case MovementMode.Dynamic:
                UpdateDynamic(deltaTime);
                break;
            case MovementMode.Blink:
                UpdateBlink(deltaTime);
                break;
            default:
                break;
        }

        UpdateVisualEffect();
    }

    private void UpdateVisualEffect()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color color = baseColor;

        if (definition.healAmount > 0)
        {
            color.a = Mathf.Lerp(0.65f, 1f, Mathf.PingPong(timer * 3f, 1f));
        }
        else if (definition.movementMode == MovementMode.Blink)
        {
            color.a = Mathf.Lerp(0.25f, 1f, Mathf.PingPong(timer * 6f, 1f));
        }

        spriteRenderer.color = color;
    }

    private void UpdatePatrol(float deltaTime)
    {
        float offset = Mathf.PingPong(timer * definition.movementSpeed, definition.patrolDistance) - (definition.patrolDistance * 0.5f);
        Vector3 newPosition = startPosition + (Vector3)(movementDirection * offset);
        transform.position = newPosition;
    }

    private void UpdateDynamic(float deltaTime)
    {
        Vector3 pos = transform.position;
        Vector3 nextPosition = pos + (Vector3)(movementDirection * definition.dynamicMaxSpeed * deltaTime);

        if (TargetSpawner.Instance != null && !TargetSpawner.Instance.IsInsideGameplayArea(nextPosition, definition))
        {
            movementDirection = -movementDirection;
            nextPosition = pos + (Vector3)(movementDirection * definition.dynamicMaxSpeed * deltaTime);
        }

        if (Time.frameCount % 120 == 0)
        {
            movementDirection = UnityEngine.Random.insideUnitCircle.normalized;
        }

        transform.position = nextPosition;
    }

    private void UpdateBlink(float deltaTime)
    {
        if (blinkWarningTimer <= 0f && !blinkWarningShown)
        {
            blinkWarningShown = true;
            blinkWarningTimer = definition.blinkWarningTime;
            return;
        }

        if (blinkWarningShown)
        {
            blinkWarningTimer -= deltaTime;
            if (blinkWarningTimer <= 0f)
            {
                MoveToNewBlinkPosition();
                blinkWarningShown = false;
                blinkWarningTimer = Mathf.Max(definition.blinkWarningTime, 0.2f);
            }
        }
        else
        {
            blinkWarningTimer -= deltaTime;
        }
    }

    private void MoveToNewBlinkPosition()
    {
        if (TargetSpawner.Instance != null && TargetSpawner.Instance.TryGetRandomSpawnPosition(definition, out Vector3 newPosition))
        {
            transform.position = newPosition;
        }
    }
}
