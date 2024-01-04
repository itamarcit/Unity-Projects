using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class FadeObjectParent : MonoBehaviour, IEquatable<FadeObjectParent>
{
	public readonly List<Material> Materials = new();
	[SerializeField] private Renderer[] renderers;

	private void Awake()
	{
		if(renderers.Length == 0) renderers = gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer1 in renderers)
		{
			Materials.AddRange(renderer1.materials);
		}
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((FadeObjectParent)obj);
	}

	public bool Equals(FadeObjectParent other)
	{
		return other != null && transform.position.Equals(other.transform.position);
	}
	
	public override int GetHashCode()
	{
		return transform.position.GetHashCode();
	}
}