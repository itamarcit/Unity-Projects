using System.Collections;
using UnityEngine;

public class BombDrop : Drop
{
    [SerializeField] private GameObject particles;
    private SpriteRenderer _renderer;
    private Sprite _bombSprite;
    private AudioSource _audio;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _bombSprite = _renderer.sprite;
        _audio = GetComponent<AudioSource>();
    }

    protected override IEnumerator DropLifeCycle()
    {
        yield return new WaitForSeconds(dropLifeCycle);
        PowerUpManager.Shared.bombDropPool.Release(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Paddle"))
        {
            StartCoroutine(ExplodeBomb());
            LifeManager.Shared.LowerLife(true, false);
        }
    }

    private IEnumerator ExplodeBomb()
    {
        _renderer.sprite = null;
        StopCoroutine(fallingEffect);
        particles.SetActive(true);
        _audio.Play();
        yield return new WaitForSeconds(1f);
        _renderer.sprite = _bombSprite;
        particles.SetActive(false);
        PowerUpManager.Shared.bombDropPool.Release(gameObject);
    }
}