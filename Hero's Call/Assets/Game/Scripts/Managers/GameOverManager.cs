using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> elements;
    [SerializeField] private GameObject yes;
    [SerializeField] private GameObject no;
    [SerializeField] private Sprite highlightedYes;
    [SerializeField] private Sprite highlightedNo;
    [SerializeField] private Sprite normalYes;
    [SerializeField] private Sprite normalNo;
    private int _currentElement = 1;
    private bool _finishedShowElements;
    private SpriteRenderer _yesRenderer;
    private SpriteRenderer _noRenderer;
    private const int TOTAL_ELEMENTS = 3;
    private const int TOTAL_SPRITES = 5;

    private void Start()
    {
        _yesRenderer = yes.GetComponent<SpriteRenderer>();
        _noRenderer = no.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!_finishedShowElements && Input.GetKeyDown(KeyCode.Space))
        {
            elements[_currentElement].SetActive(true);
            _currentElement++;
            if (_currentElement >= TOTAL_ELEMENTS)
            {
                for (int i = TOTAL_ELEMENTS; i < TOTAL_SPRITES; ++i)
                {
                    elements[i].SetActive(true);
                }

                _finishedShowElements = true;
            }
        }
        else if (_finishedShowElements)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _yesRenderer.sprite = highlightedYes;
                _noRenderer.sprite = normalNo;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _yesRenderer.sprite = normalYes;
                _noRenderer.sprite = highlightedNo;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_yesRenderer.sprite == highlightedYes)
                {
                    LifeManager.Shared.ResetLives();
                    GameManager.Shared.ResetGame();
                    for (int i = 1; i < TOTAL_SPRITES; ++i)
                    {
                        elements[i].SetActive(false);
                    }

                    ResetForNextDeath();
                    gameObject.SetActive(false);
                }
                else
                {
                    Application.Quit();
                }
            }
        }
    }

    private void ResetForNextDeath()
    {
        _currentElement = 1;
        _finishedShowElements = false;
    }
}