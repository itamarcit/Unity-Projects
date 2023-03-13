using UnityEngine;

public class RemoveLifeOnCollision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Ball"))
        {
            LifeManager.Shared.LowerLife(false);
        }
    }
}
