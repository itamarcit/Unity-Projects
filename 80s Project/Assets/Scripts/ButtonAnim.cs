using System.Collections;
using UnityEngine;

public class ButtonAnim : MonoBehaviour
{
#region Fields
	[SerializeField] private Sprite pressedButton;
	private Sprite _unpressedButton;
	private SpriteRenderer _renderer;
	private bool _isAnimated;
#endregion

#region Events
	private void Awake()
	{
		_renderer = GetComponent<SpriteRenderer>();
		_unpressedButton = _renderer.sprite;
	}
#endregion
	
#region Methods
	IEnumerator AnimateButton()
	{
		_isAnimated = true;
		while (true)
		{
			_renderer.sprite = pressedButton;
			yield return new WaitForSeconds(0.5f);
			_renderer.sprite = _unpressedButton;
			yield return new WaitForSeconds(0.5f);
		}
	}

	public void Animate()
	{
		if (!_isAnimated)
		{
			StartCoroutine(AnimateButton());
		}
	}
	
	public void StopAnimating()
	{
		_renderer.sprite = _unpressedButton;
		_isAnimated = false;
		StopAllCoroutines();
	}
#endregion
}