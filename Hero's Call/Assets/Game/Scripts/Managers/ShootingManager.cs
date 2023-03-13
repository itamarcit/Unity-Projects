using UnityEngine;

public class ShootingManager : MonoBehaviour
{
    public static ShootingManager Shared { get; private set; }

    [SerializeField] private Rigidbody2D lavaKeyRigidbody;
    [SerializeField] private Rigidbody2D iceKeyRigidbody;

    [SerializeField] private GameObject lavaKeyArrow;
    [SerializeField] private GameObject iceKeyArrow;
    
    [SerializeField] private float keySpeed;

    private const int LAVA = 0;
    private const int ICE = 1;

    private bool _isShootingLava;
    private bool _isShootingIce;

    private int _brokenLocks;

    public void BreakLock()
    {
        _brokenLocks++;
        if (_brokenLocks == 2)
        {
            GameManager.Shared.FinishedGame();
        }
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


    public void Activate(int key)
    {
        switch (key)
        {
            case LAVA:
                _isShootingLava = true;
                break;
            case ICE:
                _isShootingIce = true;
                break;
        }
    }

    private void Update()
    {
        if (_isShootingLava && Input.GetKeyDown(KeyCode.Space))
        {
            ActivatePhysics(LAVA);
            _isShootingLava = false;
            DeactivateArrow(LAVA);
        }
        else if (_isShootingIce && Input.GetKeyDown(KeyCode.Space))
        {
            ActivatePhysics(ICE);
            _isShootingIce = false;
            DeactivateArrow(ICE);
        }
    }

    private void ActivatePhysics(int key)
    {
        Rigidbody2D activeRigidBody;
        GameObject arrow;
        switch (key)
        {
            case LAVA:
                activeRigidBody = lavaKeyRigidbody;
                arrow = lavaKeyArrow;
                break;
            case ICE:
                activeRigidBody = iceKeyRigidbody;
                arrow = iceKeyArrow;
                break;
            default:
                return;
        }

        activeRigidBody.simulated = true;
        activeRigidBody.bodyType = RigidbodyType2D.Dynamic;
        activeRigidBody.gravityScale = 0;
        activeRigidBody.velocity = arrow.transform.up.normalized * keySpeed;
    }

    public void ActivateArrow(int key)
    {
        switch (key)
        {
            case LAVA:
                lavaKeyArrow.SetActive(true);
                break;
            case ICE:
                iceKeyArrow.SetActive(true);
                break;
        }
    }
    
    private void DeactivateArrow(int key)
    {
        switch (key)
        {
            case LAVA:
                lavaKeyArrow.SetActive(false);
                break;
            case ICE:
                iceKeyArrow.SetActive(false);
                break;
        }
    }

    public void DeactivatePhysics(int key)
    {
        switch (key)
        {
            case LAVA:
                lavaKeyRigidbody.simulated = false;
                break;
            case ICE:
                iceKeyRigidbody.simulated = false;
                break;
        }
    } 

    public void StopKeyMovement(int key)
    {
        switch (key)
        {
            case LAVA:
                lavaKeyRigidbody.velocity = Vector2.zero;
                break;
            case ICE:
                iceKeyRigidbody.velocity = Vector2.zero;
                break;
        }
    }
}