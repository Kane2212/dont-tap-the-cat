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
                ProcessTap(touch.position, touch.fingerId);
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            ProcessTap(Input.mousePosition);
        }
    }

    private void ProcessTap(Vector2 screenPosition, int pointerId = -1)
    {
        if (EventSystem.current != null && (pointerId >= 0
            ? EventSystem.current.IsPointerOverGameObject(pointerId)
            : EventSystem.current.IsPointerOverGameObject()))
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
        Cat selectedCat = null;

        foreach (Collider2D collider in hitColliders)
        {
            Cat cat = collider.GetComponentInParent<Cat>();
            if (cat != null)
            {
                selectedCat = cat;
                break;
            }

            Target target = collider.GetComponentInParent<Target>();
            if (target != null && selectedTarget == null)
            {
                selectedTarget = target;
            }
        }

        if (selectedCat != null)
        {
            selectedCat.HandleCatTap();
            return;
        }

        if (selectedTarget != null && !selectedTarget.IsResolved)
        {
            selectedTarget.ResolveTap();
        }
    }
}
