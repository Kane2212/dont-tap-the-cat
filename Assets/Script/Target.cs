using UnityEngine;

public class Target : MonoBehaviour
{
    private Collider2D targetCollider;

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
        }
    }

    public void SetPosition(Vector2 newPosition)
    {
        transform.position = newPosition;
    }
}
