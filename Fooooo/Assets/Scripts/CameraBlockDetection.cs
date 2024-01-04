using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraBlockDetection : MonoBehaviour
{
    /// <summary>
    /// Used from https://github.com/llamacademy/urp-fading-standard-shaders
    /// </summary>
	[SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private Transform player;
    [SerializeField]
    [Range(0, 1f)]
    private float fadedAlpha = 0.2f;
    [SerializeField]
    private Vector3 targetPositionOffset = Vector3.up; 
    [SerializeField]
    private float fadeSpeed = 1;

    [Header("Read Only Data")]
    [SerializeField]
    private List<FadeObjectParent> objectsBlockingView = new List<FadeObjectParent>();
    private readonly Dictionary<FadeObjectParent, Coroutine> _runningCoroutines = new Dictionary<FadeObjectParent, Coroutine>();

    private readonly RaycastHit[] _hits = new RaycastHit[10];
    private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
    private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
    private static readonly int Surface = Shader.PropertyToID("_Surface");

    private void OnEnable()
    {
        StartCoroutine(CheckForObjects());
    }

    private IEnumerator CheckForObjects()
    {
        while (true)
        {
            int hits = Physics.RaycastNonAlloc(
                transform.position,
                (player.transform.position + targetPositionOffset - transform.position).normalized,
                _hits,
                Vector3.Distance(transform.position, player.transform.position + targetPositionOffset),
                layerMask
            );

            if (hits > 0)
            {
                for (int i = 0; i < hits; i++)
                {
                    FadeObjectParent fadingObject = GetFadingObjectFromHit(_hits[i]);

                    if (fadingObject != null && !objectsBlockingView.Contains(fadingObject))
                    {
                        if (_runningCoroutines.ContainsKey(fadingObject))
                        {
                            if (_runningCoroutines[fadingObject] != null)
                            {
                                StopCoroutine(_runningCoroutines[fadingObject]);
                            }

                            _runningCoroutines.Remove(fadingObject);
                        }

                        _runningCoroutines.Add(fadingObject, StartCoroutine(FadeObjectOut(fadingObject)));
                        objectsBlockingView.Add(fadingObject);
                    }
                }
            }

            FadeObjectsNoLongerBeingHit();

            ClearHits();

            yield return null;
        }
    }

    private void FadeObjectsNoLongerBeingHit()
    {
        List<FadeObjectParent> objectsToRemove = new List<FadeObjectParent>(objectsBlockingView.Count);

        foreach (FadeObjectParent fadingObject in objectsBlockingView)
        {
            bool objectIsBeingHit = false;
            for (int i = 0; i < _hits.Length; i++)
            {
                FadeObjectParent hitFadingObject = GetFadingObjectFromHit(_hits[i]);
                if (hitFadingObject != null && fadingObject == hitFadingObject)
                {
                    objectIsBeingHit = true;
                    break;
                }
            }

            if (!objectIsBeingHit)
            {
                if (_runningCoroutines.ContainsKey(fadingObject))
                {
                    if (_runningCoroutines[fadingObject] != null)
                    {
                        StopCoroutine(_runningCoroutines[fadingObject]);
                    }
                    _runningCoroutines.Remove(fadingObject);
                }

                _runningCoroutines.Add(fadingObject, StartCoroutine(FadeObjectIn(fadingObject)));
                 objectsToRemove.Add(fadingObject);
            }
        }

        foreach(FadeObjectParent removeObject in objectsToRemove)
        {
            objectsBlockingView.Remove(removeObject);
        }
    }

    private IEnumerator FadeObjectOut(FadeObjectParent fadingObject)
    {
        foreach (Material material in fadingObject.Materials)
        {
            material.SetInt(SrcBlend, (int)BlendMode.SrcAlpha);
            material.SetInt(DstBlend, (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt(ZWrite, 0);
            material.SetInt(Surface, 1);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetShaderPassEnabled("DepthOnly", false);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        float time = 0;

        while (fadingObject.Materials[0].color.a > fadedAlpha)
        {
            foreach (Material material in fadingObject.Materials)
            {
                if (material.HasProperty("_Color"))
                {
                    material.color = new Color(
                        material.color.r,
                        material.color.g,
                        material.color.b,
                        Mathf.Lerp(1f, fadedAlpha, time * fadeSpeed)
                    );
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        if (_runningCoroutines.ContainsKey(fadingObject))
        {
            StopCoroutine(_runningCoroutines[fadingObject]);
            _runningCoroutines.Remove(fadingObject);
        }
    }

    private IEnumerator FadeObjectIn(FadeObjectParent fadingObject)
    {
        float time = 0;

        while (fadingObject.Materials[0].color.a < 1)
        {
            foreach (Material material in fadingObject.Materials)
            {
                if (material.HasProperty("_Color"))
                {
                    material.color = new Color(
                        material.color.r,
                        material.color.g,
                        material.color.b,
                        Mathf.Lerp(fadedAlpha, 1, time * fadeSpeed)
                    );
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        foreach (Material material in fadingObject.Materials)
        {
            material.SetInt(SrcBlend, (int)BlendMode.One);
            material.SetInt(DstBlend, (int)BlendMode.Zero);
            material.SetInt(ZWrite, 1);
            material.SetInt(Surface, 0);

            material.renderQueue = (int)RenderQueue.Geometry;

            material.SetShaderPassEnabled("DepthOnly", true);

            material.SetOverrideTag("RenderType", "Opaque");
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        if (_runningCoroutines.ContainsKey(fadingObject))
        {
            StopCoroutine(_runningCoroutines[fadingObject]);
            _runningCoroutines.Remove(fadingObject);
        }
    }

    private void ClearHits()
    {
        Array.Clear(_hits, 0, _hits.Length);
    }

    private FadeObjectParent GetFadingObjectFromHit(RaycastHit hit)
    {
        return hit.collider != null ? hit.collider.GetComponent<FadeObjectParent>() : null;
    }
}
