using System.Collections;
using UnityEngine;

public class LifeDrop : Drop
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Paddle"))
        {
            PowerUpManager.Shared.audioSource.Play();
            LifeManager.Shared.GiveLife();
            PowerUpManager.Shared.lifeDropPool.Release(gameObject);
        }
    }

    protected override IEnumerator DropLifeCycle()
    {
        yield return new WaitForSeconds(dropLifeCycle);
        PowerUpManager.Shared.lifeDropPool.Release(gameObject);
    }
}