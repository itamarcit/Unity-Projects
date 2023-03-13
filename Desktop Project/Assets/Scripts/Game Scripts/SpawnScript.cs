using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnScript : MonoBehaviour
{
    private float _bombTimer;

    private float _winConditionTimer;

    public GameObject bombPrefab;

    public GameObject winnerMessagePrefab;

    public float spawnCooldown = 1;

    public float gameLength = 10f;

    private const float Y_RANGE_TOP = 1.21f;

    private const float Y_RANGE_BOT = 1.32f;

    private const float X_WINNER_MSG = 0.09f;

    private const float Y_WINNER_MSG = 0.27f;

    private bool _isGameLost = false;

    private bool _isGameWon = false;

    private void Awake()
    {
        _winConditionTimer = gameLength;
        _bombTimer = spawnCooldown;
    }

    void Update()
    {
        _winConditionTimer -= Time.deltaTime;
        _bombTimer -= Time.deltaTime;
        if (_bombTimer <= 0 && !_isGameLost && !_isGameWon)
        {
            GameObject newBomb = Instantiate(bombPrefab, GetBombSpawnVector(), Quaternion.identity);
            newBomb.transform.parent = transform;
            BombScript bombScript = newBomb.GetComponent(typeof(BombScript)) as BombScript;
            bombScript.AssignSpawnScript(this);
            _bombTimer = spawnCooldown;
        }

        if (_winConditionTimer <= 0 && !_isGameWon && !_isGameLost)
        {
            _isGameWon = true;
            GameObject msg = Instantiate(winnerMessagePrefab, GetWinnerMessageVector(), Quaternion.identity);
            msg.transform.parent = transform.parent;
            Destroy(gameObject);
        }
    }

    private Vector3 GetWinnerMessageVector()
    {
        Vector3 center = transform.parent.position;
        return new Vector3(center.x - X_WINNER_MSG, center.y - Y_WINNER_MSG, center.z);
    }

    private Vector3 GetBombSpawnVector()
    {
        Vector3 center = transform.position;
        return new Vector3(center.x, Random.Range(center.y - Y_RANGE_BOT, center.y + Y_RANGE_TOP), 0);
    }

    public void BombExploded()
    {
        _isGameLost = true;
    }

    public bool IsGameLost()
    {
        return _isGameLost;
    }

    public bool IsGameWon()
    {
        return _isGameWon;
    }
}