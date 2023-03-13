using UnityEngine;

public class BrickManager : MonoBehaviour
{
    public static BrickManager Shared { get; private set; }

    private int _bricksInStage = 0;
    private const int ICE_STAGE = 1;
    private const int FIRE_STAGE = 2;
    private const int PRINCESS_STAGE = 3;
    private const int LAVA_KEY = 0;

    [SerializeField] private GameObject iceKey;
    [SerializeField] private GameObject lavaKey;
    [SerializeField] private GameObject ball;

    private int _bricksDestroyedThisRound = 0;


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

    public void RegisterBrick()
    {
        _bricksInStage++;
    }

    public void DeregisterBrick(GameObject brick)
    {
        _bricksInStage--;
        _bricksDestroyedThisRound++;
        GameManager.Shared.BrickDestroyed();
        if (_bricksInStage == 0)
        {
            SpawnKey(brick);
            ball.SetActive(false);
        }
        else
        { // generate a drop only if it's not the last brick in the stage
            PowerUpManager.Shared.GenerateDrop(brick.transform.position);
        }
        brick.SetActive(false);
    }
    
    /**
     * Shows a key from given brick position
     */
    private void SpawnKey(GameObject brick)
    {
        switch (GameManager.Shared.GetStage())
        {
            case ICE_STAGE:
                iceKey.transform.position = brick.transform.position;
                iceKey.SetActive(true);
                break;
            case FIRE_STAGE:
                lavaKey.transform.position = brick.transform.position;
                lavaKey.SetActive(true);
                break;
            case PRINCESS_STAGE:
                InventoryManager.Shared.MoveKeyToCenter(LAVA_KEY); 
                break;
        }
    }

    public void ResetRound()
    {
        _bricksDestroyedThisRound = 0;
    }

    public int GetBricksDestroyedThisRound()
    {
        return _bricksDestroyedThisRound;
    }
}
