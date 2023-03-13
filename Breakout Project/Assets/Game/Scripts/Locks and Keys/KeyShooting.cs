using UnityEngine;

public class KeyShooting : MonoBehaviour
{
    private const int ICE = 1;
    private const int LAVA = 0;
    [SerializeField] private bool isLavaKey;
    [SerializeField] private LockScript keyLock;

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (keyLock == null) return;
        if (isLavaKey)
        {
            ShootingManager.Shared.DeactivatePhysics(LAVA);
            InventoryManager.Shared.MoveKeyToCenter(LAVA);
        }
        else
        {
            ShootingManager.Shared.DeactivatePhysics(ICE);
            InventoryManager.Shared.MoveKeyToCenter(ICE);
        }
    }

    public void RegisterLock()
    {
        keyLock = isLavaKey
            ? GameObject.FindGameObjectWithTag("Lava Lock").GetComponent<LockScript>()
            : GameObject.FindGameObjectWithTag("Ice Lock").GetComponent<LockScript>();
    }
}