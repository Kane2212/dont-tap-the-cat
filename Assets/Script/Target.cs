using UnityEngine;
public enum TargetType
{
    Safe,
    Cat
}
public class Target : MonoBehaviour
{
    private Collider2D targetCollider;
    public TargetType targetType;

    void Awake()
    {
        targetCollider = GetComponent<Collider2D>();
    }

    void OnMouseDown()
    {
        if (GamePlayManager.instance.GetState() != GameState.Playing) return;

        Vector2 mousePos = Input.mousePosition;
        Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        if (targetCollider.OverlapPoint(worldMousePos))
        {
            Debug.Log("Point++");
            Destroy(gameObject);
            Debug.Log("Target destroyed, destroyed effect pop up.");
        }
    }

    public void SetPosition(Vector2 newPosition)
    {
        transform.position = newPosition;
    }
}
