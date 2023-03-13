using System.Collections.Generic;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Shared { get; private set; }

    [SerializeField] private List<GameObject> lifeObjects;

    [SerializeField] private float lowerLifeCooldown = 2;

    private float _timer;

    private int _lives;

    private void Start()
    {
        _lives = lifeObjects.Count;
        _timer = lowerLifeCooldown;
    }

    private void Update()
    {
        if (_timer >= 0) _timer -= Time.deltaTime;
    }

    private void Awake()
    {
        if (Shared == null)
        {
            Shared = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void ResetLives()
    {
        foreach (var life in lifeObjects)
        {
            life.SetActive(true);
        }

        _lives = lifeObjects.Count;
    }

    /**
     * There is an invulnerability timer for getting hit by a bomb after the player's life is reduced
     */
    public void LowerLife(bool isBomb, bool resetGame = true)
    {
        if (_timer > 0 && isBomb)
        {
            return;
        }
        _timer = lowerLifeCooldown;
        _lives--;
        lifeObjects[_lives].SetActive(false);
        if (_lives <= 0)
        {
            GameManager.Shared.GameOver();
        }
        else if (resetGame)
        {
            GameManager.Shared.ResetGame();
        }
    }

    public void GiveLife()
    {
        if (_lives >= 5) return;
        _lives++;
        lifeObjects[_lives - 1].SetActive(true);
    }
}