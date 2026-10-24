using UnityEngine;

public class Cat : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private Collider2D catCollider;

    void Awake()
    {
        catCollider = GetComponent<Collider2D>();
    }

    void OnMouseDown()
    {
        Debug.Log("Game Over");
    }
}
