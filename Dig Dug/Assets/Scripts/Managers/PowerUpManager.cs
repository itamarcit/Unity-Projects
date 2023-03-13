using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
#region Fields
	[SerializeField] private List<GameObject> energyObjects;
	[SerializeField] private Sprite emptyEnergy;
	[SerializeField] private Sprite quarterEnergy;
	[SerializeField] private Sprite halfEnergy;
	[SerializeField] private Sprite threeQuartersEnergy;
	[SerializeField] private Sprite fullEnergy;
	[SerializeField] private ButtonAnim button;
	public int energyForRockKill = 2;
	public int energyForDrop = 2;
	public int energyForInflateKill = 1;
	public static PowerUpManager Shared { get; private set; }
	private List<SpriteRenderer> _renderers = new();
	private int _energy;
	private const int TWO_MAX_ENERGY = 8;
	private const int TWO_THREE_ENERGY = 7;
	private const int TWO_HALF_ENERGY = 6;
	private const int TWO_ONE_ENERGY = 5;
	private const int ONE_MAX_ENERGY = 4;
	private const int ONE_THREE_ENERGY = 3;
	private const int ONE_ONE_ENERGY = 1;
	private const int ONE_HALF_ENERGY = 2;
	private const int ONE_EMPTY_ENERGY = 0;
	private const int FIRST_ENERGY_RENDERER = 0;
	private const int SECOND_ENERGY_RENDERER = 1;
	private const float RAYCAST_DISTANCE = 1.5f;
#endregion
#region Events
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

	private void Start()
	{
		foreach (GameObject energyObject in energyObjects)
		{
			_renderers.Add(energyObject.GetComponent<SpriteRenderer>());
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.B))
		{
			if (_energy < ONE_MAX_ENERGY) return;
			RaycastHit2D hit = Physics2D.Raycast(GameManager.Shared.GetPlayerPosition(),
				GameManager.Shared.GetPlayerFacingDirectionVector(), RAYCAST_DISTANCE, 1 << LayerMask.NameToLayer("Rocks"));
			if (hit)
			{
				StartCoroutine(PowerUp(hit));
			}
		}
		if (Input.GetKeyDown(KeyCode.BackQuote))
		{
			AddEnergy(1);
		}
		if (_energy >= ONE_MAX_ENERGY)
		{
			button.Animate();
		}
		else if (_energy < ONE_MAX_ENERGY)
		{
			button.StopAnimating();
		}
	}
#endregion
#region Methods
	private IEnumerator PowerUp(RaycastHit2D hit)
	{
		RockScript script = hit.collider.gameObject.GetComponent<RockScript>();
		script.StartPowerUpMovement(GameManager.Shared.GetPlayerFacingDirectionVector());
		RemoveEnergy();
		yield break;
	}

	public void AddEnergy(int amount)
	{
		if (_energy + amount < 0) return;
		if (amount + _energy >= TWO_MAX_ENERGY)
		{
			FillEnergyBar(TWO_MAX_ENERGY);
			_energy = TWO_MAX_ENERGY;
		}
		else
		{
			FillEnergyBar(amount + _energy);
			_energy = amount + _energy;
		}
	}

	private void FillEnergyBar(int amount)
	{
		switch (amount)
		{
			case ONE_ONE_ENERGY:
				_renderers[FIRST_ENERGY_RENDERER].sprite = quarterEnergy;
				_renderers[SECOND_ENERGY_RENDERER].sprite = emptyEnergy;
				break;
			case ONE_THREE_ENERGY:
				_renderers[FIRST_ENERGY_RENDERER].sprite = threeQuartersEnergy;
				_renderers[SECOND_ENERGY_RENDERER].sprite = emptyEnergy;
				break;
			case TWO_ONE_ENERGY:
				_renderers[FIRST_ENERGY_RENDERER].sprite = fullEnergy;
				_renderers[SECOND_ENERGY_RENDERER].sprite = quarterEnergy;
				break;
			case TWO_THREE_ENERGY:
				_renderers[FIRST_ENERGY_RENDERER].sprite = fullEnergy;
				_renderers[SECOND_ENERGY_RENDERER].sprite = threeQuartersEnergy;
				break;
			case TWO_MAX_ENERGY:
				foreach (SpriteRenderer spriteRenderer in _renderers)
				{
					spriteRenderer.sprite = fullEnergy;
				}
				break;
			case ONE_EMPTY_ENERGY:
				foreach (SpriteRenderer spriteRenderer in _renderers)
				{
					spriteRenderer.sprite = emptyEnergy;
				}
				break;
			case ONE_MAX_ENERGY:
				_renderers[FIRST_ENERGY_RENDERER].sprite = fullEnergy;
				_renderers[SECOND_ENERGY_RENDERER].sprite = emptyEnergy;
				break;
			case ONE_HALF_ENERGY:
				_renderers[FIRST_ENERGY_RENDERER].sprite = halfEnergy;
				_renderers[SECOND_ENERGY_RENDERER].sprite = emptyEnergy;
				break;
			case TWO_HALF_ENERGY:
				_renderers[FIRST_ENERGY_RENDERER].sprite = fullEnergy;
				_renderers[SECOND_ENERGY_RENDERER].sprite = halfEnergy;
				break;
		}
	}

	private void RemoveEnergy() // this function shouldn't be called when _energy < 4, it will do nothing.
	{
		switch (_energy)
		{
			case > ONE_MAX_ENERGY:
				_energy -= 4;
				_renderers[FIRST_ENERGY_RENDERER].sprite = _renderers[SECOND_ENERGY_RENDERER].sprite;
				_renderers[SECOND_ENERGY_RENDERER].sprite = emptyEnergy;
				break;
			case ONE_MAX_ENERGY:
			{
				_energy = 0;
				foreach (SpriteRenderer spriteRenderer in _renderers)
				{
					spriteRenderer.sprite = emptyEnergy;
				}
				break;
			}
		}
	}
#endregion
}