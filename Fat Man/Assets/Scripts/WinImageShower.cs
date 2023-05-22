using TMPro;
using UnityEngine;

public class WinImageShower : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundSpriteLoc;
    [SerializeField] private SpriteRenderer winnerMessageSpriteLoc;
    [SerializeField] private SpriteRenderer winnerIconSpriteLoc;

    private const string SCORE_KEY_PLAYER1 = "Score1";
    private const string SCORE_KEY_PLAYER2 = "Score2";

    private const string ROLE_KEY_PLAYER1 = "Role1";
    private const string ROLE_KEY_PLAYER2 = "Role2";

    [SerializeField] private Sprite iconFatmanSprite;
    [SerializeField] private Sprite iconCoachSprite;

    [SerializeField] private Sprite backgroundFatmanWin;
    [SerializeField] private Sprite backgroundCoachWin;
    [SerializeField] private Sprite messageP1FatmanWin;
    [SerializeField] private Sprite messageP2FatmanWin;
    [SerializeField] private Sprite messageP1CoachWin;
    [SerializeField] private Sprite messageP2CoachWin;
    [SerializeField] private GameObject buttonPlayAgainFatman;
    [SerializeField] private GameObject buttonPlayAgainCoach;
    [SerializeField] private TextMeshProUGUI p1Score;
    [SerializeField] private TextMeshProUGUI p2Score;

    void Start()
    {
        //PlayerPrefs key scoreKeyPlayer+i is not null because this scene comes after initializing them
        int player1Score = PlayerPrefs.GetInt(SCORE_KEY_PLAYER1);
        int player2Score = PlayerPrefs.GetInt(SCORE_KEY_PLAYER2);

        string player1Role = PlayerPrefs.GetString(ROLE_KEY_PLAYER1);
        string player2Role = PlayerPrefs.GetString(ROLE_KEY_PLAYER2);

        if (player2Role == "Fatman" && (player2Score < player1Score) ||
            player1Role == "Coach" && (player2Score > player1Score))
        {
            int tmp = player2Score;
            player2Score = player1Score;
            player1Score = tmp;
        }
        
        p1Score.text = "" + player1Score;
        p2Score.text = "" + player2Score;
        

        if (player1Score > player2Score)
        {
            if (player1Role == "Fatman")
            {
                //player1 is Fatman and won
                backgroundSpriteLoc.sprite = backgroundFatmanWin;
                winnerMessageSpriteLoc.sprite = messageP1FatmanWin;
                winnerIconSpriteLoc.sprite = iconFatmanSprite;
                buttonPlayAgainFatman.SetActive(true);
                buttonPlayAgainCoach.SetActive(false);
            }
            else
            {
                //player1 is Coach and won
                backgroundSpriteLoc.sprite = backgroundCoachWin;
                winnerMessageSpriteLoc.sprite = messageP1CoachWin;
                winnerIconSpriteLoc.sprite = iconCoachSprite;

                buttonPlayAgainCoach.SetActive(true);
                buttonPlayAgainFatman.SetActive(false);
            }
            
            
            // backgroundSpriteLoc.sprite = backgroundSprites[(int)SpriteIndex.p1Win];
            // winnerSpriteLoc.sprite = winnerSprites[(int)SpriteIndex.p1Win];
        }
        else if (player2Score > player1Score)
        {
            //player2 won
            if (player2Role == "Fatman")
            {
                //player2 is Fatman and won
                backgroundSpriteLoc.sprite = backgroundFatmanWin;
                winnerMessageSpriteLoc.sprite = messageP2FatmanWin;
                winnerIconSpriteLoc.sprite = iconFatmanSprite;

                buttonPlayAgainFatman.SetActive(true);
                buttonPlayAgainCoach.SetActive(false);
            }
            else
            {
                //player2 is Coach and won
                backgroundSpriteLoc.sprite = backgroundCoachWin;
                winnerMessageSpriteLoc.sprite = messageP2CoachWin;
                winnerIconSpriteLoc.sprite = iconCoachSprite;

                buttonPlayAgainCoach.SetActive(true);
                buttonPlayAgainFatman.SetActive(false);
            }
        }
    }
}
