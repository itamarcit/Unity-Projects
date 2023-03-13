using UnityEngine;

public class BrickScript : MonoBehaviour
{
    private void Start()
    {
        BrickManager.Shared.RegisterBrick();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            BrickManager.Shared.DeregisterBrick(gameObject);
        }
    }
}
