using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    private int _selection;
    private const int START = 0;
    private const int QUIT = 1;
    private const int NONE = 2;
    private bool _afterVideo;
    private bool _videoStarted;
    private bool _isMapShown;
    private bool _didSwordMove;
    [SerializeField] private GameObject startHand;
    [SerializeField] private GameObject quitHand;
    [SerializeField] private GameObject openingCanvas;
    [SerializeField] private GameObject afterVideo;
    private const int FIRST_STAGE = 1;
    
    public static StartMenu Shared { get; private set; } 

    private void Start()
    {
        UpperSelection();
        _selection = START;
        afterVideo.SetActive(false);
        SceneManager.sceneLoaded += this.OnSceneLoad;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= this.OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode sceneMode)
    {
        SceneManager.SetActiveScene(scene);
    }

    private void Awake()
    {
        if (Shared == null)
        {
            Shared = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            UpperSelection();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            BottomSelection();
        }
        if (!_videoStarted && Input.GetKeyDown(KeyCode.Space))
        {
            if (_selection == START) 
            {
                _selection = NONE; // start video
                _videoStarted = true;
                CameraManager.Shared.ShowVideo();
                openingCanvas.SetActive(false);
            }
            else
            {
                Application.Quit();
            }
        }
        else if (_afterVideo && !_isMapShown && Input.GetKeyDown(KeyCode.Space)) // show map after video
        {
            afterVideo.SetActive(false);
            StartCoroutine(MapManager.Shared.ShowMap(MapManager.MoveName.BeginningToIce));
            _isMapShown = true;
        }
        else if (_didSwordMove && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(MapManager.Shared.HideMap());
            SceneManager.LoadScene(FIRST_STAGE);
        } 
    }

    private void UpperSelection()
    {
        _selection = START;
        startHand.SetActive(true);
        quitHand.SetActive(false);
    }

    private void BottomSelection()
    {
        _selection = QUIT;
        startHand.SetActive(false);
        quitHand.SetActive(true);
    }

    public void AfterVideoPlayed()
    {
        afterVideo.SetActive(true);
        _afterVideo = true;
    }

    public void SwordMoved()
    {
        _didSwordMove = true;
    }
}
