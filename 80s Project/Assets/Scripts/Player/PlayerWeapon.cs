using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
#region Fields
	[SerializeField] private List<GameObject> leftWeaponChildren;
	[SerializeField] private List<GameObject> rightWeaponChildren;
	[SerializeField] private List<GameObject> upWeaponChildren;
	[SerializeField] private List<GameObject> downWeaponChildren;
	[SerializeField] private float firingDelayBetweenAnims = 0.2f;
	[SerializeField] private float weaponDelay = 1f;
	private float _weaponDelayTimer;
	private float _constantWeaponDelayTimer;
	private bool _isFiring;
	private bool _didPressUp;
	private Coroutine _inflateCoroutine;
	private const int CLOSE_STRING = 0;
	private const int MIDDLE_STRING = 1;
	private const int FIRST_WEAPON_TIP = 2;
	private const int SECOND_WEAPON_TIP = 3;
	private const int THIRD_WEAPON_TIP = 4;
	private const string WRONG_DIRECTION_GIVEN = "Wrong direction given.";
	private const float RAYCAST_DISTANCE_PER_RANGE = 2.25f;
	private const float MOVING_HAND_ANIMATION_LENGTH = 1 / 60f;
#endregion

#region Events
	private void Start()
	{
		for (int i = 0; i < 2; i++)
		{
			leftWeaponChildren[2 + i].transform.position = leftWeaponChildren[i].transform.position;
			rightWeaponChildren[2 + i].transform.position = rightWeaponChildren[i].transform.position;
			downWeaponChildren[2 + i].transform.position = downWeaponChildren[i].transform.position;
			upWeaponChildren[2 + i].transform.position = upWeaponChildren[i].transform.position;
		}
		DeactivateLists();
	}

	private void Update()
	{
		UpdateWeaponDelay();
		UpdateFireWeapon();
		if (_inflateCoroutine != null)
		{
			if (Input.GetKeyUp(KeyCode.Space) || !Input.GetKey(KeyCode.Space))
			{
				_didPressUp = true;
			}
		}
	}
#endregion

#region Methods
	private void UpdateFireWeapon()
	{
		if (Input.GetKeyDown(KeyCode.Space) && !_isFiring && GameManager.Shared.CanPlayerShoot() &&
		    _weaponDelayTimer <= 0)
		{
			_isFiring = true;
			ShootWeapon(GameManager.Shared.GetPlayerFacingDirection());
			_weaponDelayTimer = weaponDelay;
		}
	}

	private void UpdateWeaponDelay()
	{
		if (_weaponDelayTimer > 0)
		{
			_weaponDelayTimer -= Time.deltaTime;
		}
	}

	private void FinishFiringAllDirections()
	{
		FinishFiring(GridManager.Direction.South);
		FinishFiring(GridManager.Direction.East);
		FinishFiring(GridManager.Direction.West);
		FinishFiring(GridManager.Direction.North);
	}

	private void DeactivateLists()
	{
		foreach (GameObject leftWeaponChild in leftWeaponChildren)
		{
			leftWeaponChild.SetActive(false);
		}
		foreach (GameObject downWeaponChild in downWeaponChildren)
		{
			downWeaponChild.SetActive(false);
		}
		foreach (GameObject rightWeaponChild in rightWeaponChildren)
		{
			rightWeaponChild.SetActive(false);
		}
		foreach (GameObject upWeaponChild in upWeaponChildren)
		{
			upWeaponChild.SetActive(false);
		}
	}

	private void ResetWeaponDirection(GridManager.Direction direction)
	{
		switch (direction)
		{
			case GridManager.Direction.North:
				foreach (GameObject upWeaponChild in upWeaponChildren)
				{
					upWeaponChild.SetActive(false);
				}
				break;
			case GridManager.Direction.South:
				foreach (GameObject downWeaponChild in downWeaponChildren)
				{
					downWeaponChild.SetActive(false);
				}
				break;
			case GridManager.Direction.East:
				foreach (GameObject rightWeaponChild in rightWeaponChildren)
				{
					rightWeaponChild.SetActive(false);
				}
				break;
			default: // west/left
				foreach (GameObject leftWeaponChild in leftWeaponChildren)
				{
					leftWeaponChild.SetActive(false);
				}
				break;
		}
	}

	private void ShootWeapon(GridManager.Direction direction)
	{
		List<GameObject> weaponList;
		switch (direction)
		{
			case GridManager.Direction.North:
				weaponList = upWeaponChildren;
				break;
			case GridManager.Direction.East:
				weaponList = rightWeaponChildren;
				break;
			case GridManager.Direction.South:
				weaponList = downWeaponChildren;
				break;
			default: // West/Left
				weaponList = leftWeaponChildren;
				break;
		}
		if (_inflateCoroutine == null)
		{
			StartCoroutine(FireWeapon(weaponList, direction));
		}
	}

	private IEnumerator FireWeapon(List<GameObject> weaponList, GridManager.Direction direction)
	{
		GameManager.Shared.SetPlayerFiring(true);
		yield return new WaitForSeconds(MOVING_HAND_ANIMATION_LENGTH);
		int range = 1;
		if (!CanShootInDirection(range, direction))
		{
			FinishFiring(direction);
			yield break;
		}
		ShowCorrectWeaponsForRange(weaponList, range);
		if (CheckForHits(range, weaponList[range + 1]))
		{
			yield break;
		}
		yield return new WaitForSeconds(firingDelayBetweenAnims);
		range = 2;
		if (!CanShootInDirection(range, direction))
		{
			FinishFiring(direction);
			yield break;
		}
		ShowCorrectWeaponsForRange(weaponList, range);
		if (CheckForHits(range, weaponList[range + 1]))
		{
			yield break;
		}
		yield return new WaitForSeconds(firingDelayBetweenAnims);
		range = 3;
		if (!CanShootInDirection(range, direction))
		{
			FinishFiring(direction);
			yield break;
		}
		ShowCorrectWeaponsForRange(weaponList, range);
		if (CheckForHits(range, weaponList[range + 1]))
		{
			yield break;
		}
		yield return new WaitForSeconds(firingDelayBetweenAnims);
		FinishFiring(direction);
	}

	// returns whether there was a hit or not
	private bool CheckForHits(int range, GameObject tip)
	{
		RaycastHit2D hit = Physics2D.Raycast(GameManager.Shared.GetPlayerPosition(),
			GameManager.Shared.GetPlayerFacingDirectionVector(), RAYCAST_DISTANCE_PER_RANGE * range,
			1 << LayerMask.NameToLayer("Enemies"));
		if (hit)
		{
			if (range != 1)
			{
				tip.SetActive(false);
			}
			_inflateCoroutine =
				StartCoroutine(InflateUntilPressedUp(hit.collider.gameObject.GetComponent<EnemyController>()));
			return true;
		}
		_inflateCoroutine = null;
		return false;
	}

	private IEnumerator InflateUntilPressedUp(EnemyController controller)
	{
		while (!_didPressUp && GameManager.Shared.GetGamePausedTimer() <= 0)
		{
			controller.Inflate();
			yield return null;
		}
		_didPressUp = false;
		FinishFiringAllDirections();
		_inflateCoroutine = null;
	}

	private void ShowCorrectWeaponsForRange(List<GameObject> weaponList, int range)
	{
		switch (range)
		{
			case 1:
				weaponList[FIRST_WEAPON_TIP].SetActive(true); // set the tip closest to the player.
				break;
			case 2:
				weaponList[FIRST_WEAPON_TIP].SetActive(false);
				weaponList[SECOND_WEAPON_TIP].SetActive(true);
				weaponList[CLOSE_STRING].SetActive(true);
				break;
			case 3:
				weaponList[SECOND_WEAPON_TIP].SetActive(false);
				weaponList[THIRD_WEAPON_TIP].SetActive(true);
				weaponList[MIDDLE_STRING].SetActive(true);
				weaponList[CLOSE_STRING].SetActive(true);
				break;
			default:
				throw new Exception();
		}
	}

	private void FinishFiring(GridManager.Direction direction)
	{
		ResetWeaponDirection(direction);
		GameManager.Shared.SetPlayerFiring(false);
		_isFiring = false;
	}

	private bool CanShootInDirection(int range, GridManager.Direction direction)
	{
		int x, y;
		switch (direction)
		{
			case GridManager.Direction.North:
				x = 0;
				y = range;
				break;
			case GridManager.Direction.South:
				x = 0;
				y = -range;
				break;
			case GridManager.Direction.West:
				x = -range;
				y = 0;
				break;
			case GridManager.Direction.East:
				x = range;
				y = 0;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(direction), direction, WRONG_DIRECTION_GIVEN);
		}
		Vector2Int playerGridPos = GameManager.Shared.GetPlayerGridPosition()[0];
		x += playerGridPos.x;
		y += playerGridPos.y;
		return GridManager.Shared.IsGridSquareDug(x, y, GridManager.Shared.GetOppositeDirection(direction));
	}
#endregion
}