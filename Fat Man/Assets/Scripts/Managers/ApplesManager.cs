using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplesManager : MonoBehaviour
{
#region Fields
    [SerializeField] private GameObject lowerBounds;
    [SerializeField] private GameObject upperBounds;
    [SerializeField] private GameObject leftBounds;
    [SerializeField] private GameObject rightBounds;
    [SerializeField] private float minDistanceFromBounds = 2f;
    [SerializeField] private float minDistanceBetweenApples = 2f;
    [SerializeField] private List<PlayerPickUp> applesOnField;
    [SerializeField] private List<GameObject> applesPlaceholders;
    [SerializeField] private List<Sprite> grayPlaceHolders;
    [SerializeField] private List<Sprite> paintedPlaceHolders;
    [SerializeField] private GameObject player1;
    [SerializeField] private GameManager gameManager;
    private readonly List<SpriteRenderer> _placeHoldersRenderers = new();
    private PlayerOneManager _playerOneManager;
    private BoxCollider2D _ceilingBox;
    private BoxCollider2D _floorBox;
    private BoxCollider2D _leftWallBox;
    private BoxCollider2D _rightWallBox;
    private const float TIME_FOR_APPLE_TO_SPAWN_MIN = 1f;
    private const float TIME_FOR_APPLE_TO_SPAWN_MAX = 3f;
    private const int DELAY_BEFORE_APPLES_FALL_OFF_PLAYER1 = 2;
    private readonly List<Vector3> _originalApplePositions = new();
    private readonly List<Vector3> _applesTargetPos = new();
    private const float MIN_DISTANCE_FROM_PLAYER = 2f;
#endregion

#region Events
    private void Awake()
    {
        _playerOneManager = player1.GetComponent<PlayerOneManager>();
        _ceilingBox = upperBounds.GetComponent<BoxCollider2D>();
        _floorBox = lowerBounds.GetComponent<BoxCollider2D>();
        _leftWallBox = leftBounds.GetComponent<BoxCollider2D>();
        _rightWallBox = rightBounds.GetComponent<BoxCollider2D>();
        foreach (GameObject placeholder in applesPlaceholders)
        {
            _placeHoldersRenderers.Add(placeholder.GetComponent<SpriteRenderer>());
        }
    }
    
    private void Start()
    {
        InitializeTargetLocations(); // Inits the target location for each apple.
        // InitializeRightAppleCheckboxes();
        PlaceApplesAtStartSpawnPoint();
        StartCoroutine(SendApplesToEndSpawnPoints(true));
    }

    private void Update()
    {
        bool allApplesInactive = true;
        foreach (PlayerPickUp apple in applesOnField) // checks if all apples are inactive
        {
            if (apple.gameObject.activeSelf)
            {
                allApplesInactive = false;
            }
        }
        if (allApplesInactive) // if yes, put them back
        {
            ResetApples();
        }
    }
#endregion

#region Methods
    private void PlaceApplesAtStartSpawnPoint()
    {
        foreach (PlayerPickUp apple in applesOnField)
        {
            apple.transform.position = player1.transform.position;
        }
    }

    private IEnumerator SendApplesToEndSpawnPoints(bool isStartGame)
    {
        for (int i = 0; i < applesOnField.Count; ++i)
        {
            StartCoroutine(SmoothLerpWithWait(i, isStartGame));
        }
        yield return null;
    }
    
    public void CheckItemCollected(Collider2D col)
    {
        int i = applesOnField.IndexOf(col.gameObject.GetComponent<PlayerPickUp>());
        _placeHoldersRenderers[i].sprite = paintedPlaceHolders[i];
    }

    private IEnumerator SmoothLerpWithWait(int i, bool isStartGame)
    {
        if(isStartGame) yield return new WaitForSecondsRealtime(DELAY_BEFORE_APPLES_FALL_OFF_PLAYER1);
        StartCoroutine 
            (SmoothLerp (applesOnField[i].gameObject, player1.transform.position,
                _originalApplePositions[i],
                Random.Range(TIME_FOR_APPLE_TO_SPAWN_MIN, TIME_FOR_APPLE_TO_SPAWN_MAX), isStartGame));
    }

    
    //https://answers.unity.com/questions/1501234/smooth-forward-movement-with-a-coroutine.html
    private IEnumerator SmoothLerp(GameObject apple, Vector3 startingPos, Vector3 finalPos, float time,
                                   bool isStartGame)
    {
        float elapsedTime = 0;
        apple.transform.position = startingPos;
        while (elapsedTime < time)
        {
            apple.transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (!isStartGame)
        {
            apple.tag = "Apple";
            apple.layer = LayerMask.NameToLayer("Player Triggers");
        }
    }

    /**
     * Gets a random vector inside the bounding box (inside the walls)
     */
    private Vector3 GetRandomVectorInBoundingBox()
    {
        Bounds topBounds = _ceilingBox.bounds, bottomBounds = _floorBox.bounds, 
            leftBox = _leftWallBox.bounds, rightBox = _rightWallBox.bounds;
        float ceilingY = topBounds.center.y - topBounds.extents.y;
        float leftWallX = leftBox.center.x + leftBox.extents.x;
        float rightWallX = rightBox.center.x - rightBox.extents.x;
        float floorY = bottomBounds.center.y + bottomBounds.extents.y;
        return new Vector3(
            Random.Range(leftWallX + minDistanceFromBounds, rightWallX - minDistanceFromBounds),
            Random.Range(floorY + minDistanceFromBounds, ceilingY - minDistanceFromBounds), 0);
    }

    /**
     * Initializes the list of start locations of the apples (_originalApplePositions).
     * Each vector initialized will be in a distance greater than minDistanceBetweenApples
     */
    private void InitializeTargetLocations()
    {
        while (_originalApplePositions.Count < applesOnField.Count)
        {
            Vector3 newLocation = GetRandomVectorInBoundingBox();
            bool tooClose = false;
            foreach (Vector3 pos in _originalApplePositions)
            {
                if (Vector3.Distance(newLocation, pos) < minDistanceBetweenApples)
                {
                    tooClose = true;
                    break;
                }
            }
            if (!tooClose)
            {
                _originalApplePositions.Add(newLocation);
            }
        }
    }
    
    /**
     * Resets the location of the apples to be random again, and collectible again by the player.
     */
    private void ResetApples()
    {
        _originalApplePositions.Clear();
        InitializeTargetLocations();
        StartCoroutine(SendApplesToEndSpawnPoints(false));
        int i = 0;
        foreach (PlayerPickUp apple in applesOnField)
        {
            apple.gameObject.SetActive(true);
            _placeHoldersRenderers[i].sprite = grayPlaceHolders[i];
            ++i;
        }
        gameManager.ResetSentVegetables();
    }
    
    /// <summary>
    /// Gets a random position that is far from the player in the bounding box.
    /// </summary>
    /// <param name="randomVectorFarFromPlayer"></param>
    public void GetVectorForAppleAfterHit(out Vector3 randomVectorFarFromPlayer)
    {
        int i = 0;
        randomVectorFarFromPlayer = GetRandomVectorInBoundingBox();
        float dist = Vector3.Distance(
            randomVectorFarFromPlayer, _playerOneManager.transform.position);
        while (dist < MIN_DISTANCE_FROM_PLAYER || !FarFromOtherTargets(randomVectorFarFromPlayer))
        {
            Vector3 randomVector = GetRandomVectorInBoundingBox();
            float newDist = Vector3.Distance(
                randomVector, _playerOneManager.transform.position);
            randomVectorFarFromPlayer = newDist > dist ? randomVector : randomVectorFarFromPlayer;
            dist = Mathf.Max(newDist, dist);
            i++;
            if (i == 100) break;
        }
        _applesTargetPos.Add(randomVectorFarFromPlayer); // add the apple pos so other apples won't go near
    }

    private bool FarFromOtherTargets(Vector3 randomVectorFarFromPlayer)
    {
        foreach (Vector3 appleTarget in _applesTargetPos)
        {
            if (Vector3.Distance(appleTarget, randomVectorFarFromPlayer) < minDistanceBetweenApples) return false;
        }
        foreach (PlayerPickUp apple in applesOnField)
        {
            if (apple.IsFollowing() || apple.IsBeingSentBackToField()) continue;
            if (Vector3.Distance(apple.transform.position, randomVectorFarFromPlayer) < 1f) return false;
        }
        return true;
    }

    public void RemoveAppleTargetPos(Vector3 toRemove)
    { // Remove all vectors that are equal to toRemove from applesTargetPos
        _applesTargetPos.RemoveAll(vec => vec == toRemove);
    }

#endregion

    /// <summary>
    /// Removes the paint from the indicator that the player has collected the given food (apple object).
    /// </summary>
    public void RemoveColorFromApple(GameObject food)
    {
        int foodIndex = 0;
        for (int i = 0; i < applesOnField.Count; i++)
        {
            if (food == applesOnField[i].gameObject)
            {
                break;
            }
            ++foodIndex;
        }
        _placeHoldersRenderers[foodIndex].sprite = grayPlaceHolders[foodIndex];
    }
}
