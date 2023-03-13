using UnityEngine;

public class NextStageWall : MonoBehaviour
{
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            GameManager.Shared.AdvanceStage();
        }
    }
}
