using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    MovementInput movement;
    ArrowSystem arrowSystem;
    TargetSystem targetSystem;

    [Header("Connections")]
    [SerializeField] CinemachineFreeLook thirdPersonCam;
    [SerializeField] CinemachineInputProvider cameraInputProvider;
    [SerializeField] CinemachineImpulseSource defaultImpulse;
    [SerializeField] CinemachineImpulseSource perfectImpulse;

    [Header("Camera Settings")]
    [SerializeField] float boostFieldOfView = 100;
    [SerializeField] float runFieldOfView = 85;
    [SerializeField] float defaultFieldOfView = 40;
    [SerializeField] float orbitBoostingRadius = 2.5f;
    [SerializeField] float orbitRunningRadius = 3.5f;
    [SerializeField] float orbitDefaultRadius = 15;
    [SerializeField] float cameraOffsetAmount = .25f;
    private float originalCameraOffsetAmount;
    [SerializeField] float cameraOffsetLerp = 1;
    private float originalCameraOffsetLerp;

    void Start()
    {
        movement = GetComponent<MovementInput>();
        arrowSystem = GetComponent<ArrowSystem>();
        targetSystem = TargetSystem.instance;

        arrowSystem.OnTargetHit.AddListener(Shake);
        arrowSystem.OnInputStart.AddListener(LockCamera);
        arrowSystem.OnInputRelease.AddListener(UnlockCamera);
        arrowSystem.OnTargetLost.AddListener(UnlockCamera);
        arrowSystem.OnArrowRelease.AddListener(ArrowChargeCheck);

        originalCameraOffsetAmount = cameraOffsetAmount;
        originalCameraOffsetLerp = cameraOffsetLerp;
    }

    void Update()
    {
        //Replicate movement booleans
        bool isBoosting = movement.isBoosting;
        bool isRunning = movement.isRunning;
        bool finishedBoost = movement.finishedBoost;

        //Set variables for readability
        float fov = isBoosting ? boostFieldOfView : (isRunning ? runFieldOfView : defaultFieldOfView);
        float lerpAmount = finishedBoost ? .006f : .01f;

        thirdPersonCam.m_Lens.FieldOfView = Mathf.Lerp(thirdPersonCam.m_Lens.FieldOfView, fov,lerpAmount);

        // Loop through all the Cinemachine Camera orbits
        for (int i = 0; i < 3; i++)
        {
            float newRadius = isBoosting ? orbitBoostingRadius : (isRunning ? orbitRunningRadius : orbitDefaultRadius);
            thirdPersonCam.m_Orbits[i].m_Radius = Mathf.Lerp(thirdPersonCam.m_Orbits[i].m_Radius, newRadius, lerpAmount);

            CinemachineComposer composer = thirdPersonCam.GetRig(i).GetCinemachineComponent<CinemachineComposer>();
            float targetScreenPos = targetSystem.lerpedTargetPos.x;
            float characterScreenPos = Camera.main.WorldToScreenPoint(transform.position).x;

            cameraOffsetAmount = arrowSystem.isCharging ? originalCameraOffsetAmount * 3 : originalCameraOffsetAmount;
            float targetCharacterDistance = ExtensionMethods.Remap(targetScreenPos - characterScreenPos, -800, 800, -cameraOffsetAmount, cameraOffsetAmount);
            targetCharacterDistance = Mathf.Clamp(targetCharacterDistance, -cameraOffsetAmount, cameraOffsetAmount);

            cameraOffsetLerp = originalCameraOffsetLerp;
            composer.m_ScreenX = Mathf.Lerp(composer.m_ScreenX, isRunning ? .5f - targetCharacterDistance : .5f, cameraOffsetLerp * Time.deltaTime);
        }
    }

    void ArrowChargeCheck(float charge)
    {
        if(arrowSystem.HalfCharge())
            perfectImpulse.GenerateImpulse();
    }

    void Shake()
    {
        if(movement.isRunning || movement.holdRunInput)
            defaultImpulse.GenerateImpulse();
    }

    void LockCamera()
    {
        if (cameraInputProvider != null)
            cameraInputProvider.enabled = false;
    }

    void UnlockCamera()
    {
        if (cameraInputProvider != null)
            cameraInputProvider.enabled = true;
    }

}
