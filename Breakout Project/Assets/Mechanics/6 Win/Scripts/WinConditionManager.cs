using System.Collections.Generic;
using UnityEngine;

public class WinConditionManager : MonoBehaviour
{
    public List<GameObject> winConditionsList;

    private static int _length;

    public SpriteRenderer spriteRenderer;

    void Start()
    {
        _length = winConditionsList.Count;
    }

    public void BrickDestroyed()
    {
        _length--;
        if (_length <= 0)
        {
            spriteRenderer.color = Color.green;
        }
    }
}
