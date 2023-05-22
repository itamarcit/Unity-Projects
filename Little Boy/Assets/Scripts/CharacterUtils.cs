using System.Collections;
using UnityEngine;

public static class CharacterUtils
{
	private const float NO_DISSOLVE = 1.5f;
	private static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");

	public static IEnumerator Dissolve(SpriteRenderer renderer)
	{
		float timeElapsed = 0;
		float totalDuration = 1f;
		while (timeElapsed < totalDuration)
		{
			renderer.material.SetFloat(DissolveAmount, NO_DISSOLVE - ((timeElapsed / totalDuration) * NO_DISSOLVE));
			timeElapsed += Time.deltaTime;
			yield return null;
		}
	}
}
