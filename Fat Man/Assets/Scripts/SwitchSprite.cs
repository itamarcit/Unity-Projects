using UnityEngine;

public class SwitchSprite : MonoBehaviour
{
	[SerializeField] private Sprite sprite;
	[SerializeField] private SpriteRenderer spriteRendererToChange;
	
	public void SwapSprite()
	{
		spriteRendererToChange.sprite = sprite;
	}
}
