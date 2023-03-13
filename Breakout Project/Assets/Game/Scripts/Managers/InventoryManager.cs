using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public const string ICE_KEY = "Ice Stage";
    public const string LAVA_KEY = "Lava Stage";
    private const int LAVA = 0;
    private const int ICE = 1;
    private const float KEY_MOVE_SPEED = 3f;

    public static InventoryManager Shared { get; private set; }

    [SerializeField] private GameObject iceKey;
    [SerializeField] private GameObject lavaKey;

    private KeyShooting _iceKeyShooting;
    private KeyShooting _lavaKeyShooting;
    private bool _moveIceKey;
    private bool _moveLavaKey;
    private readonly Vector3 _middleVector = new Vector3(0.79f, -3.91f, 0);


    private void Start()
    {
        _iceKeyShooting = iceKey.GetComponent<KeyShooting>();
        _lavaKeyShooting = lavaKey.GetComponent<KeyShooting>();
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

    public void AddItem(string item)
    {
        switch (item)
        {
            case ICE_KEY:
                iceKey.SetActive(true);
                break;
            case LAVA_KEY:
                lavaKey.SetActive(true);
                break;
        }
    }


    /*
     * Activates a given key once the final level is over.
     * Key is 0 for lava, 1 for ice
     */
    public void MoveKeyToCenter(int key)
    {
        switch (key)
        {
            case LAVA:
                _moveLavaKey = true;
                break;
            case ICE:
                _moveIceKey = true;
                break;
        }
    }

    private void Update()
    {
        if (_moveLavaKey)
        {
            lavaKey.transform.localPosition = Vector3.MoveTowards(lavaKey.transform.localPosition,
                _middleVector, Time.deltaTime * KEY_MOVE_SPEED);
            if (Vector3.Distance(lavaKey.transform.localPosition, _middleVector) < 0.01f)
            {
                _moveLavaKey = false;
                ShootingManager.Shared.ActivateArrow(LAVA);
                ShootingManager.Shared.Activate(LAVA);
            }
        }

        if (_moveIceKey)
        {
            iceKey.transform.localPosition = Vector3.MoveTowards(iceKey.transform.localPosition,
                _middleVector, Time.deltaTime * KEY_MOVE_SPEED);
            if (Vector3.Distance(iceKey.transform.localPosition, _middleVector) < 0.01f)
            {
                _moveIceKey = false;
                ShootingManager.Shared.ActivateArrow(ICE);
                ShootingManager.Shared.Activate(ICE);
            }
        }
    }

    public void RegisterLocks()
    {
        _iceKeyShooting.RegisterLock();
        _lavaKeyShooting.RegisterLock();
    }

    public bool IsKeyMoving(int key)
    {
        switch (key)
        {
            case LAVA:
                return _moveLavaKey;
            case ICE:
                return _moveIceKey;
            default:
                return false;
        }
    }
}