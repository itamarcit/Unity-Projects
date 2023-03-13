using System.Collections;
using UnityEngine;

public abstract class Drop : MonoBehaviour
{
    [SerializeField] protected float dropLifeCycle = 10f;
    protected abstract IEnumerator DropLifeCycle();
    protected Coroutine fallingEffect;

    private IEnumerator FallEffect()
    {
        while (true)
        {
            transform.position -= Vector3.up * Time.deltaTime;
            yield return null;            
        }
    }

    protected void OnEnable()
    {
        StartCoroutine(DropLifeCycle());
        fallingEffect = StartCoroutine(FallEffect());
    }
}