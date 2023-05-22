using System.Collections.Generic;
using UnityEngine;

public class ChooseType : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private PolygonCollider2D _polygonCollider;
    private VegetableWrapper _vegetableWrapper;
    [SerializeField] private List<GameObject> forms;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _polygonCollider = GetComponent<PolygonCollider2D>();
        int randomIndex = Random.Range(0, forms.Count); // Set the initial form to be random
        SetFormToBeRandom(randomIndex);
        UpdatePolygonCollider();
    }

    private void UpdatePolygonCollider()
    {
        Sprite sprite = _spriteRenderer.sprite;
        for (int i = 0; i < _polygonCollider.pathCount; i++) _polygonCollider.SetPath(i, new List<Vector2>());
        _polygonCollider.pathCount = sprite.GetPhysicsShapeCount();
        List<Vector2> path = new List<Vector2>();
        for (int i = 0; i < _polygonCollider.pathCount; i++) {
            path.Clear();
            sprite.GetPhysicsShape(i, path);
            _polygonCollider.SetPath(i, path.ToArray());
        }
    }

    private void SetFormToBeRandom(int formIndex)
    {
        VegetableWrapper vegetableWrapper = forms[formIndex].GetComponent<VegetableWrapper>();
        vegetableWrapper.SwitchSprite(_spriteRenderer, transform, true);
        transform.localScale = vegetableWrapper.gameObject.transform.localScale;
        transform.rotation = vegetableWrapper.transform.rotation;
        _vegetableWrapper = vegetableWrapper;
    }

    public void SetForm(bool isPainted)
    {
        _vegetableWrapper.SwitchSprite(_spriteRenderer, transform, isPainted);
    }
}
