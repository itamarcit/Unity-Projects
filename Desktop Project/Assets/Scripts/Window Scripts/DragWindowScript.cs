using UnityEngine;
using UnityEngine.Rendering;

public class DragWindowScript : MonoBehaviour
{
    private static int _sortingOrder = 1;
    private float _deltaX;
    private float _deltaY;

    public SortingGroup group;
    
    void Start()
    {
        MoveWindowToFront();
    }

    private void OnMouseDown()
    {
        MoveWindowToFront();
        Vector3 mouse = GetMousePosition();
        Vector3 objectPos = transform.parent.position;
        _deltaX = mouse.x - objectPos.x;
        _deltaY = mouse.y - objectPos.y;
    }

    public void MoveWindowToFront()
    {
        group.sortingOrder = _sortingOrder;
        _sortingOrder++;
    }

    private void OnMouseDrag()
    {
        transform.parent.position = GetMousePositionWithDelta();
    }

    public Vector3 GetMousePosition()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector3(mouse.x, mouse.y, 0);
    }

    public Vector3 GetMousePositionWithDelta()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector3(mouse.x - _deltaX, mouse.y - _deltaY, 0);
    }
}
