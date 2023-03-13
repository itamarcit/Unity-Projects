using System.Collections;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Shared { get; private set; }

    [SerializeField] private GameObject sword;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject mapText;
    [SerializeField] private float speed = 2;

    private readonly Vector3[] _beginningToIce = new[]
    {
        new Vector3(7.369754f, -4.233933f, 0),
        new Vector3(4.879094f, -4.263223f, 0),
        new Vector3(3.707494f, -3.794583f, 0),
        new Vector3(2.345004f, -3.691563f, 0),
    };

    private readonly Vector3[] _iceToLava = new[]
    {
        new Vector3(1.034024f, -3.032033f, 0),
        new Vector3(0.02301407f, -2.973454f, 0),
        new Vector3(-0.3648258f, -2.468453f, 0),
        new Vector3(-0.2254459f, -1.216053f, 0),
        new Vector3(-0.9506259f, -0.9372933f, 0),
        new Vector3(-0.7597358f, -0.7615533f, 0),
        new Vector3(0.3098541f, -1.002943f, 0),
        new Vector3(0.6542641f, -0.8928533f, 0),
        new Vector3(0.5371041f, -0.4171433f, 0),
        new Vector3(1.196634f, -0.3363433f, 0),
        new Vector3(0.6401241f, 0.05149668f, 0),
        new Vector3(1.036044f, 0.4322667f, 0),
        new Vector3(1.226934f, 1.567507f, 0)
    };

    private readonly Vector3[] _lavaToPrincess = new[]
    {
        new Vector3(0.1331041f, 2.296727f, 0f),
        new Vector3(-0.2729158f, 2.275517f, 0f),
        new Vector3(-0.8981059f, 1.754357f, 0f),
        new Vector3(-1.054656f, 1.931107f, 0f),
        new Vector3(-0.4920859f, 2.848187f, 0f),
        new Vector3(-1.481886f, 4.014736f, 0f),
        new Vector3(-1.732366f, 2.493677f, 0f),
        new Vector3(-2.961536f, 1.201887f, 0f),
        new Vector3(-3.524106f, 1.003927f, 0f),
        new Vector3(-5.784486f, 1.513977f, 0f),
        new Vector3(-6.357156f, 2.659317f, 0f),
    };

    private int _currentVector;
    private int _currentStage;

    public enum MoveName
    {
        BeginningToIce = 0,
        IceToLava = 1,
        LavaToPrincess = 2
    }
    private const int BEGINNING_TO_ICE = 0;
    private const int ICE_TO_LAVA = 1;
    private const int LAVA_TO_PRINCESS = 2;
    private bool _startMoving = false;

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

    private void MoveSword()
    {
        _startMoving = true;
    }

    private void Update()
    {
        if (_startMoving && _currentStage <= LAVA_TO_PRINCESS)
        {
            Vector3[] positionVectors = GetCurrentPositionVector();
            sword.transform.localPosition = Vector3.MoveTowards(sword.transform.localPosition, positionVectors[_currentVector], Time.deltaTime * speed);
            if (Vector3.Distance(sword.transform.localPosition, positionVectors[_currentVector]) < 0.01f)
            {
                if (_currentVector >= positionVectors.Length - 1) // if arrived at a landmark
                {
                    _startMoving = false;
                    _currentVector = 0;
                    _currentStage++;
                }
                _currentVector++;
            }
        }
    }

    private Vector3[] GetCurrentPositionVector()
    {
        switch (_currentStage)
        {
            case BEGINNING_TO_ICE:
                return _beginningToIce;
            case ICE_TO_LAVA:
                return _iceToLava;
            case LAVA_TO_PRINCESS:
                return _lavaToPrincess;
            default:
                return null;
        }
    }

    public IEnumerator ShowMap(MoveName stage)
    {
        background.SetActive(true);
        sword.SetActive(true);
        yield return StartCoroutine(HandleShowMap(stage));
    }

    private IEnumerator HandleShowMap(MoveName stage)
    {
        yield return StartCoroutine(DelayMoveSword(stage)); // wait until the coroutine is finished before continuing
        mapText.SetActive(true);
        if(stage == MoveName.BeginningToIce) StartMenu.Shared.SwordMoved();
    }
    
    private IEnumerator DelayMoveSword(MoveName stage)
    {
        yield return new WaitForSeconds(1);
        MoveSword();
        while (!DidFinishMove(stage))
        {
            yield return null;
        }

        yield return new WaitForSeconds(2);
    }

    public IEnumerator HideMap()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        yield break;
    }

    /**
     * Checks if the move is finished. move indicates which move it means,
     * Use with class's enum from outside
     */
    private bool DidFinishMove(MoveName move)
    {
        return _currentStage == (int)move+1;
    }
}