using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayersManager : MonoBehaviour
{
	// [SerializeField] private SpriteRenderer coachTag;
 //    [SerializeField] private SpriteRenderer fatManTag;
 //    [SerializeField] private Sprite p1coachSprite;
 //    [SerializeField] private Sprite p2coachSprite;
 //    [SerializeField] private Sprite p1fatSprite;
 //    [SerializeField] private Sprite p2fatSprite;
 //    
 //    [SerializeField] private GameObject p1Score;
 //    [SerializeField] private GameObject p2Score;

 [SerializeField] private SpriteRenderer rightUILoc;
 [SerializeField] private SpriteRenderer leftUILoc;
 
 [SerializeField] private GameObject bottomAppleRightSpawnLoc;
 [SerializeField] private GameObject bottomAppleLeftSpawnLoc;

 
 
 [SerializeField] private SpriteRenderer p1RoleLoc;
 [SerializeField] private SpriteRenderer p2RoleLoc;
 
 
 // [SerializeField] private GameObject p1ScoreLoc;
 // [SerializeField] private GameObject p2ScoreLoc;
 //
 [SerializeField] private ScoreText p1Score;
 [SerializeField] private ScoreText p2Score;

 
 
 [SerializeField] private Sprite fatManUIimage;
 [SerializeField] private Sprite coachUIimage;
 
 
 [SerializeField] private Sprite p1IsCoach;
 [SerializeField] private Sprite p1IsFatman;
 [SerializeField] private Sprite p2IsCoach;
 [SerializeField] private Sprite p2IsFatman;

 [SerializeField] private GameObject[] applePlaceholders;
 
 private string roleKeyPlayer1 = "Role1";
 private string roleKeyPlayer2 = "Role2";

 
 private const int FATMAN = 0;
 private const int COACH = 1;



 [SerializeField] private FallingBlockSpawner _fallingBlockSpawner;
    [SerializeField] private GameObject p1;
    [SerializeField] private GameObject p2;
    
    


    //private static bool isFirstRound = true;
    
    // Start is called before the first frame update
    void Start()
    {
	    AquireLotteryResult();
   
	  //   if (isFirstRound)
	  //   {
			// AquireLotteryResult();
	  //   }
	  //   else
	  //   {
		 //    AquireInverseLotteryResult();
		 //    
		 //    //AquireLotteryResult();
		 //    p1Score.GetComponent<ScoreText>().SetScore(PlayerPrefs.GetInt("Score1"));
		 //    p2Score.GetComponent<ScoreText>().SetScore(PlayerPrefs.GetInt("Score2"));
	  //   }
   
    }

    public void IncreaseFatmanScore(int playerNum, int scoreToAdd)
    {
	    int p1Index = PlayerPrefs.GetInt("player1Character");
	    // Player 1 is Coach, Player 2 is Fatman
	    if (p1Index == 1)
	    {
		    p2Score.IncreaseScore(playerNum, scoreToAdd);
		    PlayerPrefs.SetInt("FatManScore", p2Score.GetScore());
	    }
	    // Player 2 is Coach, Player 1 is Fatman
	    else
	    {
		    p1Score.IncreaseScore(playerNum, scoreToAdd);
		    PlayerPrefs.SetInt("FatManScore", p1Score.GetScore());
	    }
    }

    public void IncreaseCoachScore(int playerNum, int scoreToAdd)
    {
	    int p1Index = PlayerPrefs.GetInt("player1Character");
	    // Player 1 is Coach, Player 2 is Fatman
	    if (p1Index == 1)
	    {
		    p1Score.IncreaseScore(playerNum, scoreToAdd);
		    PlayerPrefs.SetInt("CoachScore", p1Score.GetScore());

	    }
	    // Player 2 is Coach, Player 1 is Fatman
	    else
	    {
		    p2Score.IncreaseScore(playerNum, scoreToAdd);
		    PlayerPrefs.SetInt("CoachScore", p2Score.GetScore());
	    }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void moveApplePlaceHoldersRight()
    {
	    foreach (GameObject applePlaceholder in applePlaceholders)
	    {
		    Vector3 pos = applePlaceholder.transform.position;
		    pos.x = bottomAppleRightSpawnLoc.transform.position.x;
		    applePlaceholder.transform.position = pos;
	    }
    }
	
    private void moveApplePlaceHoldersLeft()
    {
	    foreach (GameObject applePlaceholder in applePlaceholders)
	    {
		    Vector3 pos = applePlaceholder.transform.position;
		    pos.x = bottomAppleLeftSpawnLoc.transform.position.x;
		    applePlaceholder.transform.position = pos;
	    }
    }
    
    private void AquireLotteryResult()
    {
	    int p1Index = PlayerPrefs.GetInt("player1Character");
	    //int p2Index = PlayerPrefs.GetInt("player2Character");

	    KeyCode[] wasdControls = {KeyCode.W, KeyCode.S, KeyCode.D, KeyCode.A};
	    KeyCode[] arrowsControls = {KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.LeftArrow};

	    //currP1Character == Character.FatMan
	    // Player 1 is Coach, Player 2 is Fatman
	    if (p1Index == COACH)
	    {
		    // fatManTag.sprite = p1fatSprite;
		    // coachTag.sprite = p2coachSprite;


		    rightUILoc.sprite = fatManUIimage;
		    leftUILoc.sprite = coachUIimage;


		    p1RoleLoc.sprite = p1IsCoach;
		    p2RoleLoc.sprite = p2IsFatman;

		    
		    p1.GetComponent<PlayerUnrestrictedMovement>().SetControls(arrowsControls);
		    p2.GetComponent<FallingObject>().SetControls(wasdControls);
		    _fallingBlockSpawner.GetComponent<FallingBlockSpawner>().MoveToLeftSide();

		    moveApplePlaceHoldersRight();
		    
		    
		    PlayerPrefs.SetString(roleKeyPlayer1, "Coach");
		    PlayerPrefs.SetString(roleKeyPlayer2, "Fatman");
	    }
	    else
		    // Player 2 is Coach, Player 1 is Fatman

	    {
		    
		    
		    p1RoleLoc.sprite = p1IsFatman;
		    p2RoleLoc.sprite = p2IsCoach;
		    
		    // fatManTag.sprite = p2fatSprite;
		    // coachTag.sprite = p1coachSprite;
		    
		    rightUILoc.sprite = coachUIimage;
		    leftUILoc.sprite = fatManUIimage;
		    
		    p1.GetComponent<PlayerUnrestrictedMovement>().SetControls(wasdControls);
		    p2.GetComponent<FallingObject>().SetControls(arrowsControls);
		    _fallingBlockSpawner.GetComponent<FallingBlockSpawner>().MoveToRightSide();

		    moveApplePlaceHoldersLeft();

		    PlayerPrefs.SetString(roleKeyPlayer2, "Coach");
		    PlayerPrefs.SetString(roleKeyPlayer1, "Fatman");
	    }
    }
    
    // private void AquireInverseLotteryResult()
    // {
	   //  int p1Index = PlayerPrefs.GetInt("player1Character");
	   //  //int p2Index = PlayerPrefs.GetInt("player2Character");
    //
	   //  //currP1Character == Character.FatMan
	   //  if (p1Index == 0)
	   //  {
		  //   fatManTag.sprite = p2fatSprite;
		  //   coachTag.sprite = p1coachSprite;
	   //  }
	   //  else
	   //  {
		  //   fatManTag.sprite = p1fatSprite;
		  //   coachTag.sprite = p2coachSprite;
	   //  }
    // }
    

    // public void LoadNextScene()
    // {
	   //  SaveScores
	   //  SceneManager.LoadScene("End");
	   //  
	   //  
	   //  // if (isFirstRound)
	   //  // {
		  //  //  isFirstRound = false;
		  //  //  SceneManager.LoadScene("Game");
	   //  // }
	   //  // else
	   //  // {
		  //  //  SceneManager.LoadScene("End");
	   //  // }
    // }
}
