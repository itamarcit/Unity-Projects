using UnityEngine;

public class BombScript : MonoBehaviour
{
    public int speed = 1;

    private const float WINDOW_X_SIZE = 5.67f;
    private const float BALL_ROTATION_SPEED = 100f;

    public SpriteRenderer spriteRenderer;

    public Sprite bomb;

    public Sprite boomSprite;

    private SpawnScript _spawnScript;

    private void Start()
    {
        spriteRenderer.sprite = bomb;
    }

    void Update()
    {
        if (_spawnScript.IsGameLost())
        {
            transform.eulerAngles = Vector3.zero;
            spriteRenderer.sprite = boomSprite;
        }
        else if (_spawnScript.IsGameWon())
        {
            Destroy(gameObject);
        }
        else
        { // move the bomb, game isn't over
            transform.position -= new Vector3(Time.deltaTime * speed, 0, 0);
            transform.eulerAngles += Vector3.back * (Time.deltaTime * BALL_ROTATION_SPEED);
        }

        if (Mathf.Abs(transform.position.x - transform.parent.parent.position.x) >= WINDOW_X_SIZE)
        {
            _spawnScript.BombExploded();
        }
    }

    private void OnMouseDown()
    {
        if (!_spawnScript.IsGameLost())
        {
            Destroy(gameObject);
        }
    }

    public void AssignSpawnScript(SpawnScript script)
    {
        _spawnScript = script;
    }
}