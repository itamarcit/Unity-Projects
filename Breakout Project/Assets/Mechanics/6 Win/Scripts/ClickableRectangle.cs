using UnityEngine;

public class ClickableRectangle : MonoBehaviour
{
    public WinConditionManager manager;
    private void OnMouseUp()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        manager.BrickDestroyed();
    }
}
