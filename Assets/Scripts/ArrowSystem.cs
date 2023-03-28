using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Animations;
using UnityEngine.Events;

public class ArrowSystem : MonoBehaviour
{
    //Events
    [HideInInspector] public UnityEvent OnTargetHit;
    [HideInInspector] public UnityEvent OnInputStart;
    [HideInInspector] public UnityEvent OnInputRelease;
    [HideInInspector] public UnityEvent<float> OnArrowRelease;
    [HideInInspector] public UnityEvent OnTargetLost;

    TargetSystem targetSystem;
    ArrowTarget lockedTarget;
    Coroutine arrowSystemCooldown;
    MovementInput movement;

    public bool active;

    [Header("Arrow Settings")]
    [SerializeField] float arrowCooldown = .5f;
    [SerializeField] ParticleSystem wrongArrowEmission;
    [SerializeField] ParticleSystem correctArrowEmission;
    [SerializeReference] Transform arrowReleasePoint;
 
    [Header("Charge Settings")]
    public bool isCharging;
    private bool releaseCooldown;
    [SerializeField] float chargeDuration = .8f;
    [SerializeField] Ease chargeEase;
    private float chargeAmount;
    public float middleChargePrecision = .5f;

    [Header("Parameters")]
    [SerializeField] float slowDownInterval;


    [Header("UI Connections")]
    public Slider arrowPrecisionSlider;

    [Header("Input")]
    public InputActionReference fireAction;

    public ParticleSystem particleTest;

    void Start()
    {

        //Declarations
        targetSystem = GetComponent<TargetSystem>();
        movement = GetComponent<MovementInput>();

        //Inputs
        fireAction.action.performed += FireAction_performed;
        fireAction.action.canceled += FireAction_canceled;
    }

    private void Update()
    {
        GetComponent<Animator>().SetBool("isCharging", isCharging);
        GetComponent<Animator>().SetBool("releaseCooldown", releaseCooldown);
        GetComponent<Animator>().SetBool("isRunning", FindObjectOfType<MovementInput>().isRunning);
    }

    private void CheckArrowRelease()
    {
        DisableSystemForPeriod();
        StartCoroutine(ReleaseCooldown());

        if (HalfCharge())
            chargeAmount = .5f;

        if (FullCharge())
            chargeAmount = 1;

        OnArrowRelease.Invoke(chargeAmount);

        if(chargeAmount >= 1 - middleChargePrecision || (chargeAmount > .5f - middleChargePrecision && chargeAmount < .5f + middleChargePrecision))
        {
            if(chargeAmount > .5f - middleChargePrecision && chargeAmount < .5f + middleChargePrecision)
                StartCoroutine(SlowTime());

            ReleaseCorrectArrow();
        }
        else
        {
            ReleaseWrongArrow();
        }

        IEnumerator SlowTime()
        {
            float scale = .2f;
            Time.timeScale = scale;
            yield return new WaitForSeconds(slowDownInterval / (1/scale) );
            Time.timeScale = 1;
        }

        IEnumerator ReleaseCooldown()
        {
            releaseCooldown = true;
            yield return new WaitForSeconds(.4f);
            releaseCooldown = false;
        }

    }

    public bool HalfCharge()
    {
        return chargeAmount > .5f - middleChargePrecision && chargeAmount < .5f + middleChargePrecision;
    }

    public bool FullCharge()
    {
        return chargeAmount >= 1 - middleChargePrecision;
    }


    void ReleaseCorrectArrow()
    {
        lockedTarget = targetSystem.currentTarget;
        correctArrowEmission.transform.position = lockedTarget.transform.position;
        var shape = correctArrowEmission.shape;
        shape.position = correctArrowEmission.transform.InverseTransformPoint(arrowReleasePoint.position);
        correctArrowEmission.Play();
    }

    void ReleaseWrongArrow()
    {
        lockedTarget = targetSystem.currentTarget;
        wrongArrowEmission.transform.position = arrowReleasePoint.position;
        wrongArrowEmission.transform.LookAt(targetSystem.currentTarget.transform);
        wrongArrowEmission.Play();
    }

    public void TargetHit(Vector3 dir)
    {
        OnTargetHit.Invoke();

        active = true;
        releaseCooldown = false;
        lockedTarget.DisableTarget(dir);

        //Particle Settings
        var shape = particleTest.shape;
        shape.position = transform.InverseTransformPoint(lockedTarget.transform.position + (Vector3.down * .5f));
        particleTest.Play();
    }

    void DisableSystemForPeriod()
    {
        active = false;

        if(arrowSystemCooldown != null)
            StopCoroutine(arrowSystemCooldown);
        arrowSystemCooldown = StartCoroutine(ArrowCooldown());
        IEnumerator ArrowCooldown()
        {
            yield return new WaitForSeconds(arrowCooldown);
            active = true;
        }
    }


    void StartFire()
    {
        if (!active)
            return;

        if (targetSystem.currentTarget == null || !targetSystem.currentTarget.isAvailable)
            return;

        isCharging = true;
        OnInputStart.Invoke();

        DOVirtual.Float(0, 1, chargeDuration, SetChargeAmount).SetId(0).SetEase(chargeEase);
    }


    public void CancelFire(bool input)
    {

        if (input)
        {
            if (targetSystem.currentTarget == null || !targetSystem.currentTarget.isAvailable)
                return;

            if (!isCharging)
                return;

            OnInputRelease.Invoke();

            CheckArrowRelease();
            targetSystem.ClearStoredTarget();
        }

        OnTargetLost.Invoke();
        isCharging = false;

        SetChargeAmount(0);
        DOTween.Kill(0);
    }

    void SetChargeAmount(float charge)
    {
        chargeAmount = charge;
        arrowPrecisionSlider.value = chargeAmount;
    }

    #region Input

    private void FireAction_performed(InputAction.CallbackContext obj)
    {
        StartFire();
    }

    private void FireAction_canceled(InputAction.CallbackContext obj)
    {
        CancelFire(true);
    }

    #endregion


}
