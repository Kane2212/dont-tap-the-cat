using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

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

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                ProcessTap(touch.position);
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            ProcessTap(Input.mousePosition);
        }
    }

    private void ProcessTap(Vector2 screenPosition)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector3 worldPosition = Camera.main != null ? Camera.main.ScreenToWorldPoint(screenPosition) : Vector3.zero;
        worldPosition.z = 0f;

        Collider2D[] hitColliders = Physics2D.OverlapPointAll(worldPosition);
        if (hitColliders == null || hitColliders.Length == 0)
        {
            return;
        }

        Target selectedTarget = null;

        foreach (Collider2D collider in hitColliders)
        {
            Target target = collider.GetComponent<Target>();
            if (target != null)
            {
                selectedTarget = target;
                break;
            }

            Cat cat = collider.GetComponent<Cat>();
            if (cat != null)
            {
                cat.HandleCatTap();
                return;
            }
        }

        if (selectedTarget != null && !selectedTarget.IsResolved)
        {
            selectedTarget.ResolveTap();
        }
    }
}
