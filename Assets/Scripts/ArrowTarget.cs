using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[SelectionBase]
public class ArrowTarget : MonoBehaviour
{
    Transform player;
    MovementInput playerMovement;
    private Renderer detectorRenderer;
    private Color originalColor;

    [Header("Booleans")]
    public bool isAvailable = true;
    public bool isReachable = false;

    [Header("Connections")]
    [SerializeField] Renderer visualRenderer;
    [SerializeField] Renderer flameRenderer;
    [SerializeField] ParticleSystem hitParticle;
    [SerializeField] Collider particleDetector;
    [SerializeField] GameObject arrowObject;
    [SerializeField] Transform imageParent;

    private void Start()
    {
        detectorRenderer = GetComponent<Renderer>();
        originalColor = visualRenderer.material.color;

        playerMovement = FindObjectOfType<MovementInput>();
        player = playerMovement.transform;
    }

    private void Update()
    {
        Vector3 playerToObject = transform.position - playerMovement.transform.position;
        bool isBehindPlayer = (Vector3.Dot(Camera.main.transform.forward, playerToObject) < 0) && playerMovement.isRunning;

        if (Vector3.Distance(transform.position, player.position) < TargetSystem.instance.minReachDistance && !isBehindPlayer && !isReachable)
        {
            isReachable = true;
            if (TargetSystem.instance.targets.Contains(this))
                TargetSystem.instance.reachableTargets.Add(this);
        }

        if ((Vector3.Distance(transform.position, player.position) > TargetSystem.instance.minReachDistance || isBehindPlayer) && isReachable)
        {
            isReachable = false;
            if (TargetSystem.instance.reachableTargets.Contains(this))
                TargetSystem.instance.reachableTargets.Remove(this);

            if (TargetSystem.instance.currentTarget == this)
            {
                TargetSystem.instance.StopTargetFocus();
            }
        }

    }

    private void OnBecameVisible()
    {
        if (!TargetSystem.instance.targets.Contains(this) && isAvailable)
        {
            TargetSystem.instance.targets.Add(this);

            if(this.isReachable)
                TargetSystem.instance.reachableTargets.Add(this);
        }
    }

    private void OnBecameInvisible()
    {
        if (TargetSystem.instance.targets.Contains(this))
        {
            TargetSystem.instance.targets.Remove(this);

            if(TargetSystem.instance.reachableTargets.Contains(this))
                TargetSystem.instance.reachableTargets.Remove(this);
        }

        if(TargetSystem.instance.currentTarget == this)
        {
            TargetSystem.instance.StopTargetFocus();
        }
     }

    public void DisableTarget(Vector3 dir)
    {
        isAvailable = false;
        particleDetector.enabled = false;
        arrowObject.SetActive(true);

        visualRenderer.transform.DOPunchPosition(-Vector3.forward * 7, .5f, 2, 1, false);
        visualRenderer.material.color = Color.black;
        flameRenderer.enabled = false;
        hitParticle.Play();

        if (TargetSystem.instance.targets.Contains(this))
            TargetSystem.instance.targets.Remove(this);

        if (TargetSystem.instance.reachableTargets.Contains(this))
            TargetSystem.instance.reachableTargets.Remove(this);

        StartCoroutine(ReactivateCoroutine());

        IEnumerator ReactivateCoroutine()
        {
            yield return new WaitForSeconds(TargetSystem.instance.targetDisableCooldown);
            isAvailable = true;
            visualRenderer.material.color = originalColor;
            flameRenderer.enabled = true;
            particleDetector.enabled = true;
            arrowObject.SetActive(false);

            if (detectorRenderer.isVisible && !TargetSystem.instance.targets.Contains(this))
            {
                TargetSystem.instance.targets.Add(this);
                if (this.isReachable)
                    TargetSystem.instance.reachableTargets.Add(this);
            }

        }
    }

}
