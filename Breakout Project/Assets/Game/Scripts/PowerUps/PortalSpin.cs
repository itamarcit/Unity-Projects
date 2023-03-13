using UnityEngine;

public class PortalSpin : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = -100f;
    [SerializeField] private Portal portalParent;

    private void Update()
    {
        transform.Rotate(new Vector3(0f, 0f, rotationSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        portalParent.TeleportToOther(gameObject.name);
    }
}