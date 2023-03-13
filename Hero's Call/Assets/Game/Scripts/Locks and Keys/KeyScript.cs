using UnityEngine;

public class KeyScript : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 1;
    private SpriteRenderer _renderer;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Paddle"))
        {
            if (gameObject.CompareTag("Ice Key"))
            {
                InventoryManager.Shared.AddItem(InventoryManager.ICE_KEY);
            }
            if (gameObject.CompareTag("Lava Key"))
            {
                InventoryManager.Shared.AddItem(InventoryManager.LAVA_KEY);
            }
            gameObject.SetActive(false);
            GameManager.Shared.CompleteStage();
        }
    }

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (transform.position.y >= GameManager.Shared.GetPaddle().transform.position.y)
        {
            transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
        }
        gameObject.transform.RotateAround(_renderer.bounds.center, Vector3.forward, -Time.deltaTime * 100);
    }
}
