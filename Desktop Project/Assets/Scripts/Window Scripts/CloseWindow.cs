using UnityEngine;

public class CloseWindow : MonoBehaviour
{
    private void OnMouseUpAsButton()
    {
        Destroy(transform.parent.gameObject);
    }
}
