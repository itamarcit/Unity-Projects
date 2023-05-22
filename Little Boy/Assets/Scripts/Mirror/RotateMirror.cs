using UnityEngine;

public class RotateMirror : MonoBehaviour
{
	private void OnMouseUp()
	{
		transform.Rotate(new Vector3(0, 0, 45));
	}
}
