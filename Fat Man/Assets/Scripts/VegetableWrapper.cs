using System;
using UnityEngine;

[Serializable]
public class VegetableWrapper : MonoBehaviour
{
	[SerializeField] private GameObject coloredVegetable;
	[SerializeField] private GameObject notColoredVegetable;
	private SpriteRenderer _paintedRenderer;
	private SpriteRenderer _notPaintedRenderer;

	private void Awake()
	{
		_paintedRenderer = coloredVegetable.GetComponent<SpriteRenderer>();
		_notPaintedRenderer = notColoredVegetable.GetComponent<SpriteRenderer>();
	}

	/**
	 * Changes the given sprite renderer to the requested sprite,
	 * and sets the transform to be like the prefab
	 */
	public void SwitchSprite(SpriteRenderer spriteRenderer, Transform prefabTransform, bool isColored)
	{
		if(_paintedRenderer == null) _paintedRenderer = coloredVegetable.GetComponent<SpriteRenderer>();
		if(_notPaintedRenderer == null) _notPaintedRenderer = notColoredVegetable.GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = isColored ? _paintedRenderer.sprite : _notPaintedRenderer.sprite;
	}
}