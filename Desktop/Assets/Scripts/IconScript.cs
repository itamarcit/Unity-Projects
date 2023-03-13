using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class IconScript : MonoBehaviour
{
    private float _lastClicked;
    private GameObject _openWindow;
    private DragWindowScript _dragWindowScript;
    private float _transparency = 1f;
    private float _deltaX;
    private float _deltaY;
    private Vector3 _destination;
    private static int _sortingOrder = 1;
    private const float HIGHEST_WINDOW_X = 1.737f;
    private const float HIGHEST_WINDOW_Y = 2.5f;
    private const float LOWEST_WINDOW_X = -2.357f;
    private const float LOWEST_WINDOW_Y = -0.269f;

    public SpriteRenderer spriteRenderer;
    public Sprite idle;
    public Sprite hover;
    public GameObject window;
    public SortingGroup group;
    public float maxTimeBetweenClicks = 0.5f;

    private void OnMouseEnter()
    {
        spriteRenderer.sprite = hover;
    }

    private void OnMouseExit()
    {
        spriteRenderer.sprite = idle;
    }

    private void OnMouseUpAsButton()
    {
        if (DoubleClickedIcon())
        {
            OpenWindow();
        }
    }

    private void MoveIconToFront()
    {
        group.sortingOrder = _sortingOrder;
        _sortingOrder++;
    }

    private void Start()
    {
        _destination = transform.position;
        _lastClicked = -maxTimeBetweenClicks - 1;
    }

    private void Update()
    {
        var color = spriteRenderer.color;
        if (!color.a.Equals(_transparency))
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.MoveTowards(color.a, _transparency, Time.deltaTime));
        }

        if (_destination != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, _destination, Time.deltaTime);
        }

    }

    private bool DoubleClickedIcon()
    {
        if (Time.time - _lastClicked <= maxTimeBetweenClicks)
        {
            _lastClicked = Time.time;
            return true;
        }

        _lastClicked = Time.time;
        return false;
    }

    private void OpenWindow()
    {
        if (_openWindow != null)
        {
            if (_dragWindowScript != null)
            {
                _dragWindowScript.MoveWindowToFront();
            }
            return;
        }

        var pos = new Vector3(Random.Range(LOWEST_WINDOW_X, HIGHEST_WINDOW_X), 
            Random.Range(LOWEST_WINDOW_Y, HIGHEST_WINDOW_Y));
        _openWindow = Instantiate(window, pos, Quaternion.identity);
        _dragWindowScript = _openWindow.GetComponentInChildren<DragWindowScript>();
    }

    private void OnMouseUp()
    {
        _transparency = 1f;
        Vector3 pos = transform.position;
        _destination = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), 0);
    }

    private void OnMouseDrag()
    {
        _transparency = 0.2f;
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mouse.x - _deltaX, mouse.y - _deltaY, 0);
    }

    private void OnMouseDown()
    {
        MoveIconToFront();
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse = new Vector3(mouse.x, mouse.y, 0);
        Vector3 objectPos = transform.position;
        _deltaX = mouse.x - objectPos.x;
        _deltaY = mouse.y - objectPos.y;
    }
}