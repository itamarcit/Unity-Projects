using System.Collections;
using UnityEngine;

public class LockScript : MonoBehaviour
{
    private const int LAVA = 0;
    private const int ICE = 1;
    [SerializeField] private Sprite openLock;
    [SerializeField] private Vector3 keyPosition;
    private SpriteRenderer _renderer;
    private AudioSource _audio;
    private GameObject _keyToMove = null;
    private bool _moveToCenter;
    private bool _drop;


    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _audio = GetComponent<AudioSource>();
    }

    private void OpenLock(GameObject key)
    {
        _renderer.sprite = openLock;
        _audio.Play();
        var lockObject = gameObject;
        lockObject.transform.eulerAngles -= new Vector3(0, 0, 3.62f);
        lockObject.transform.localScale = new Vector3(0.1645263f, 0.1645263f, 0.1645263f);
        StartCoroutine(DropKey(key));
    }

    private IEnumerator DropKey(GameObject key)
    {
        yield return new WaitForSeconds(1f);
        _drop = true;
        yield return new WaitForSeconds(8f);
        Destroy(key);
        Destroy(gameObject);
        ShootingManager.Shared.BreakLock();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!InventoryManager.Shared.IsKeyMoving(LAVA) && gameObject.CompareTag("Lava Lock") &&
            col.gameObject.CompareTag("Lava Key"))
        {
            ShootingManager.Shared.StopKeyMovement(LAVA);
            _keyToMove = col.gameObject;
            _moveToCenter = true;
            ShootingManager.Shared.DeactivatePhysics(LAVA);
            InventoryManager.Shared.MoveKeyToCenter(ICE);
        }
        else if (!InventoryManager.Shared.IsKeyMoving(ICE) && gameObject.CompareTag("Ice Lock") &&
                 col.gameObject.CompareTag("Ice Key"))
        {
            ShootingManager.Shared.StopKeyMovement(ICE);
            _keyToMove = col.gameObject;
            _moveToCenter = true;
            ShootingManager.Shared.DeactivatePhysics(ICE);
        }
    }

    private void Update()
    {
        if (_moveToCenter)
        {
            _keyToMove.transform.localPosition =
                Vector3.MoveTowards(_keyToMove.transform.localPosition, keyPosition, Time.deltaTime * 5f);
            if (Vector3.Distance(_keyToMove.transform.localPosition, keyPosition) < 0.01f)
            {
                OpenLock(_keyToMove);
                _moveToCenter = false;
            }
        }
        else if (_drop)
        {
            gameObject.transform.position += Vector3.down * Time.deltaTime;
            _keyToMove.transform.position += Vector3.down * Time.deltaTime;
        }
    }
}