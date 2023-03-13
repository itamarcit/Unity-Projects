using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Shared { get; private set; }

    [SerializeField] private float lifeDropChance = 0.2f;
    [SerializeField] private GameObject lifeDropPrefab;
    
    [SerializeField] private float bombDropChance = 0.2f;
    [SerializeField] private GameObject bombDropPrefab;

    [SerializeField] private GameObject portalParent;
    [SerializeField] private float portalDropChance = 0.2f;
    [SerializeField] private float portalCooldown = 5f;
    [SerializeField] public float portalActiveTime = 10f;
    
    [SerializeField] public AudioSource audioSource;
    
    public ObjectPool<GameObject> lifeDropPool;
    public ObjectPool<GameObject> bombDropPool;


    private bool _canSpawnPortal = true;

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

    private void Start()
    {
        
        lifeDropPool = new ObjectPool<GameObject>(CreateLifeDrop, OnGetDrop, OnReleaseDrop);
        bombDropPool = new ObjectPool<GameObject>(CreateBombDrop, OnGetDrop, OnReleaseDrop);
    }

    private GameObject CreateLifeDrop()
    {
        GameObject item = Instantiate(lifeDropPrefab);
        item.gameObject.SetActive(false);
        return item;
    }

    private GameObject CreateBombDrop()
    {
        GameObject item = Instantiate(bombDropPrefab);
        item.gameObject.SetActive(false);
        return item;
    }

    private void OnGetDrop(GameObject item)
    {
        if(item == null) return;
        item.gameObject.SetActive(true);
    }

    private void OnReleaseDrop(GameObject item)
    {
        item.gameObject.SetActive(false);
    }

    public void GenerateDrop(Vector3 position)
    {
        GameObject item = null;
        var random = Random.Range(0, 100);
        var bombChance = bombDropChance * 100 + lifeDropChance * 100;
        var portalChance = bombChance + portalDropChance * 100;
        if (random < lifeDropChance * 100)
        {
            item = lifeDropPool.Get();
        }
        else if (random < bombChance && random >= lifeDropChance * 100)
        {
            item = bombDropPool.Get();
        }
        else if (_canSpawnPortal && random < portalChance * 100 && random >= bombChance)
        {
            _canSpawnPortal = false;
            portalParent.SetActive(true);
            StartCoroutine(WaitCooldown());
        }
        
        if (item != null)
        {
            item.transform.position = position;
        }
    }

    private IEnumerator WaitCooldown()
    {
        yield return new WaitForSeconds(portalCooldown + portalActiveTime);
        _canSpawnPortal = true;
    }
}
