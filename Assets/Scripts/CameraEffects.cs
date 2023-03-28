using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static Cyan.Blit;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;


public class CameraEffects : MonoBehaviour
{
    Material materialClone;
    Material originalMaterial;

    MovementInput movement;
    [SerializeField] Cyan.Blit blit;
    [SerializeField] float lerpTime = 1f;

    private void Awake()
    {
        movement = FindObjectOfType<MovementInput>();

        if (blit == null)
            return;

        originalMaterial = blit.settings.blitMaterial;
        materialClone = new Material(blit.settings.blitMaterial);
        blit.settings.blitMaterial = materialClone;
    }

    private void Update()
    {
        if (blit == null)
            return;

        float alphaValue = movement.isRunning ? (movement.isBoosting ? .02f : .01f) : 0;

        blit.settings.blitMaterial.SetFloat("_Alpha", Mathf.Lerp(blit.settings.blitMaterial.GetFloat("_Alpha"), alphaValue, lerpTime * Time.deltaTime));
    }

    private void OnDestroy()
    {
        if (blit == null)
            return;

        blit.settings.blitMaterial = originalMaterial;
    }


}
