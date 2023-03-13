using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FygarFire : MonoBehaviour
{
#region Fields
	[SerializeField] private List<GameObject> fires; // stage 0-2, in the correct order
	[SerializeField] private float fygarDelay = 1f;
	private const int MAX_PLAYER_DISTANCE_FROM_FYGAR = 2;
	private static readonly int IsPreparingToFire = Animator.StringToHash("IsPreparingToFire");
#endregion
#region Methods
	public void AttemptFire(EnemyController controller, Vector2Int fygarGridPos)
	{
		List<Vector2Int> playerGridPos = GameManager.Shared.GetPlayerGridPosition();
		// Check if the player is within 2 grid squares from fygar. (Checks both movement's target and origin	
		if (Mathf.Abs(playerGridPos[0].x - fygarGridPos.x) > MAX_PLAYER_DISTANCE_FROM_FYGAR && 
		    Mathf.Abs(playerGridPos[1].x - fygarGridPos.x) > MAX_PLAYER_DISTANCE_FROM_FYGAR)
		{
			return;
		}
		if (fygarGridPos.y != playerGridPos[0].y && fygarGridPos.y != playerGridPos[1].y)
		{
			return;
		}
		controller.GetAnimator().SetBool(IsPreparingToFire, true);
		controller.SetIsWaitingForFire(true);
		StartCoroutine(Fire(fygarGridPos, controller));
	}

	private List<Vector2Int> GetShootingPoints(Vector2Int fygarGridPos, Vector2Int playerGridPos)
	{
		List<Vector2Int> result = new();
		if (fygarGridPos.x > playerGridPos.x) // The player is on fygar's left side.
		{
			for (int i = 1; i < 4; i++)
			{
				if (GridManager.Shared.IsGridSquareDug(fygarGridPos.x - i, fygarGridPos.y, GridManager.Direction.East))
				{
					result.Add(new Vector2Int(fygarGridPos.x - i, fygarGridPos.y));
				}
				if (!GridManager.Shared.IsGridSquareDug(fygarGridPos.x - i, fygarGridPos.y, GridManager.Direction.West))
				{
					break;
				}
			}
			if (result.Count == 0)
			{
				if (GridManager.Shared.IsGridSquareDug(fygarGridPos.x - 1, fygarGridPos.y, new[]
				    {
					    GridManager.Direction.East, GridManager.Direction.West, GridManager.Direction.South
				    }))
				{
					result.Add(new Vector2Int(fygarGridPos.x - 1, fygarGridPos.y));
				}
			}
		}
		else // The player is on fygar's right side.
		{
			for (int i = 1; i < 4; i++)
			{
				if (GridManager.Shared.IsGridSquareDug(fygarGridPos.x + i, fygarGridPos.y, GridManager.Direction.West))
				{
					result.Add(new Vector2Int(fygarGridPos.x + i, fygarGridPos.y));
				}
				if (!GridManager.Shared.IsGridSquareDug(fygarGridPos.x + i, fygarGridPos.y, GridManager.Direction.East))
				{
					break;
				}
			}
			if (result.Count == 0)
			{
				if (GridManager.Shared.IsGridSquareDug(fygarGridPos.x + 1, fygarGridPos.y, new[]
				    {
					    GridManager.Direction.East, GridManager.Direction.West, GridManager.Direction.South
				    }))
				{
					result.Add(new Vector2Int(fygarGridPos.x + 1, fygarGridPos.y));
				}
			}
		}
		return result;
	}

	private IEnumerator Fire(Vector2Int fygarGridPos, EnemyController controller)
	{
		yield return new WaitForSeconds(fygarDelay); // give the player time to run away
		List<Vector2Int> playerPositions = GameManager.Shared.GetPlayerGridPosition();
		int fireSize = Mathf.Max(GetShootingPoints(fygarGridPos, playerPositions[0]).Count, 
			GetShootingPoints(fygarGridPos, playerPositions[1]).Count);
		if (fireSize == 0)
		{
			controller.SetIsWaitingForFire(false);
			controller.GetAnimator().SetBool(IsPreparingToFire, false);
			yield break; // nothing's dug around fygar.
		}
		bool isShootingDirectionLeft = (fygarGridPos.x - GameManager.Shared.GetPlayerGridPosition()[0].x) > 0;
		controller.SetFygarFacingDirection(isShootingDirectionLeft
			? GridManager.Direction.West
			: GridManager.Direction.East);
		if (!controller.CanFireNow())
		{
			controller.SetIsWaitingForFire(false);
			controller.GetAnimator().SetBool(IsPreparingToFire, false);
			yield break;
		}
		for (int i = 0; i < fireSize; i++)
		{
			fires[i].SetActive(true);
			yield return new WaitForSeconds(0.2f);
			fires[i].SetActive(false);
		}
		controller.GetAnimator().SetBool(IsPreparingToFire, false);
		controller.SetIsWaitingForFire(false);
	}
#endregion
}