using UnityEngine;

public class Cat : MonoBehaviour
{
    [SerializeField] private Target target;

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<Target>();
        }
    }

    public void HandleCatTap()
    {
        if (target != null)
        {
            target.ResolveTap();
            return;
        }

        GamePlayManager.instance?.TriggerGameOver("Cat tapped");
    }
}
