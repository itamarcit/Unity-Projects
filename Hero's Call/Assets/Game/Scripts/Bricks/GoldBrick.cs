using UnityEngine;

public class GoldBrick : MonoBehaviour
{
    [SerializeField] private Sprite brokenBrick;
    private SpriteRenderer _renderer;
    private bool _isHit;
    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        BrickManager.Shared.RegisterBrick();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            if (_isHit)
            {
                BrickManager.Shared.DeregisterBrick(gameObject);
            }
            else
            {
                _isHit = true;
                _renderer.sprite = brokenBrick;
            }
        }
    }
}
