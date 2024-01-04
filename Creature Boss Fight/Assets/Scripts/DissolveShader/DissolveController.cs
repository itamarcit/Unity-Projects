using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Avrahamy;

namespace DissolveShader
{
	public class DissolveController : MonoBehaviour
	{
		private static readonly int DISSOLVE_AMOUNT_PROPERTY_ID = Shader.PropertyToID("_DissolveAmount");

		[SerializeField] Renderer[] meshRenderers;
		[SerializeField] PassiveTimer dissolveDuration = new(1f);
		[SerializeField] PassiveTimer appearDuration = new(1f);

		private bool isVisible;
		private MaterialPropertyBlock propertyBlock;

		protected void Awake()
		{
			propertyBlock = new MaterialPropertyBlock();
			isVisible = false;
			SetDissolve(0f);
			if (meshRenderers is { Length: > 0 }) return;
			meshRenderers = GetComponentsInChildren<Renderer>();
		}

		protected void Update()
		{
			if (dissolveDuration.IsSet)
			{
				if (dissolveDuration.IsActive)
				{
					SetDissolve(dissolveDuration.Progress);
				}
				else
				{
					dissolveDuration.Clear();
					SetDissolve(1f);
					isVisible = false;
				}
			}
			else if (appearDuration.IsSet)
			{
				if (appearDuration.IsActive)
				{
					SetDissolve(1f - appearDuration.Progress);
				}
				else
				{
					appearDuration.Clear();
					SetDissolve(0f);
					isVisible = true;
				}
			}
		}

		private void ToggleDissolve()
		{
			if (dissolveDuration.IsActive)
			{
				var progress = dissolveDuration.Progress;
				dissolveDuration.Clear();
				appearDuration.Start();
				appearDuration.Progress = 1f - progress;
				return;
			}
			if (appearDuration.IsActive)
			{
				var progress = appearDuration.Progress;
				appearDuration.Clear();
				dissolveDuration.Start();
				dissolveDuration.Progress = 1f - progress;
				return;
			}
			if (isVisible)
			{
				dissolveDuration.Start();
				return;
			}
			appearDuration.Start();
		}

		private void SetDissolve(float amount)
		{
			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.GetPropertyBlock(propertyBlock);
				propertyBlock.SetFloat(DISSOLVE_AMOUNT_PROPERTY_ID, amount);
				meshRenderer.SetPropertyBlock(propertyBlock);
			}
		}

		public void SetVisibility(bool visible)
		{
			dissolveDuration.Clear();
			appearDuration.Clear();
			isVisible = !visible;
			SetDissolve(visible ? 1 : 0);
			ToggleDissolve();
		}
	}
}