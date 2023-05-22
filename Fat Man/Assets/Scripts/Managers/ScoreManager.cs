using UnityEngine;

public class ScoreManager : MonoBehaviour
{
	[SerializeField] private int scoreForHittingPlayer;
	[SerializeField] private int scoreForKillingPlayer;
	[Tooltip("Score the player receives for collecting one junk food item.")]
	[SerializeField] private int scoreForOneJunkFood;

	[Tooltip("Score the player receives for collecting two junk food items.")]
	[SerializeField] private int scoreForTwoJunkFood;

	[Tooltip("Score the player receives for collecting three junk food items.")]
	[SerializeField] private int scoreForThreeJunkFood;

	[Tooltip("Score the player receives for collecting four junk food items.")]
	[SerializeField] private int scoreForFourJunkFood;
	
	public int ScoreForHittingPlayer => scoreForHittingPlayer;
	public int ScoreForKillingPlayer => scoreForKillingPlayer;
	public int ScoreForOneJunkFood => scoreForOneJunkFood;
	public int ScoreForTwoJunkFood => scoreForTwoJunkFood;
	public int ScoreForThreeJunkFood => scoreForThreeJunkFood;
	public int ScoreForFourJunkFood => scoreForFourJunkFood;
}
