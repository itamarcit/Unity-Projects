using System;
using ItamarRamon;
using TMPro;
using UnityEngine;

namespace Flocking
{
	public class TailModeText : MonoBehaviour
	{
		private TextMeshProUGUI tmp;
		private void Awake()
		{
			tmp = GetComponent<TextMeshProUGUI>();
		}

		private void Update()
		{
			tmp.text = "Tail Mode: " + Enum.GetName(typeof(WiggleTail.TailMode), WiggleTail.tailMode);
		}
	}
}