
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;

public class TargetSystem : MonoBehaviour
{
    public static TargetSystem instance;

    [HideInInspector] public UnityEvent<Transform> OnTargetChange;

    ArrowSystem arrowSystem;
    MovementInput movement;

    public List<ArrowTarget> targets;
    public List<ArrowTarget> reachableTargets;
    [HideInInspector] public Vector3 lerpedTargetPos;

    [Header("Target")]
    public ArrowTarget currentTarget;
    public ArrowTarget storedTarget;
    private Transform cachedTarget;

    [Header("Parameters")]
    //Weight values that determine what distance (screen/player) gets prioritized
    [SerializeField] float screenDistanceWeight = 1;
    [SerializeField] float positionDistanceWeight = 8;
    //Min Distance for targets
    public float minReachDistance = 70;
    public float targetDisableCooldown = 4;

    //Test stuff
    [Header("User Interface")]
    public RectTransform rectImage;
    public float rectSizeMultiplier = 2;
    [SerializeField] ClonedTargetScript targetClonePrefab;

    //Animation Rigging
    [Header("Procedural Animation")]
    [SerializeField] Rig aimRig;
    [SerializeField] Transform aimRigTarget;

    [Header("Focus Point")]
    [SerializeField] RectTransform focusPointRect;
    [SerializeField] float deltaSpeed = 100;


    private void Awake()
    {
        instance = this;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        arrowSystem = GetComponent<ArrowSystem>();
        movement = GetComponent<MovementInput>();

        arrowSystem.OnArrowRelease.AddListener(CloneInterface);
        arrowSystem.OnInputStart.AddListener(AddTargetCache);
    }


    void Update()
    {
        SetFocusPoint();

        if (reachableTargets.Count < 1 || !arrowSystem.active)
        {
            storedTarget = null;
            rectImage.gameObject.SetActive(false);

            return;
        }

        CheckTargetChange();

        currentTarget = arrowSystem.isCharging ? currentTarget : reachableTargets[TargetIndex()];

        //User Interface

        rectImage.gameObject.SetActive(true);
        rectImage.transform.position = ClampedScreenPosition(currentTarget.transform.position);
        float distanceFromTarget = Vector3.Distance(currentTarget.transform.position, transform.position);
        rectImage.sizeDelta = new Vector2(Mathf.Clamp(115 - (distanceFromTarget - rectSizeMultiplier),50,200), Mathf.Clamp(115 - (distanceFromTarget - rectSizeMultiplier),50,200));


    }

    void SetFocusPoint()
    {
        Vector3 targetPositionX;
        if (currentTarget != null && movement.isRunning)
            targetPositionX = new Vector3(ClampedScreenPosition(currentTarget.transform.position).x, Screen.height / 2, 0);
        else
            targetPositionX = new Vector3(Screen.width / 2, Screen.height / 2, 0);


        //Vector3 targetPositionX = currentTarget != null ? new Vector3(ClampedScreenPosition(currentTarget.transform.position).x, Screen.height / 2, 0) : new Vector3(Screen.width / 2, Screen.height / 2, 0);

        focusPointRect.position = Vector3.MoveTowards(focusPointRect.position, targetPositionX, deltaSpeed * Time.deltaTime);

        lerpedTargetPos = focusPointRect.transform.position;
    }

    Vector3 ClampedScreenPosition(Vector3 targetPos)
    {
        Vector3 WorldToScreenPos = Camera.main.WorldToScreenPoint(targetPos);
        Vector3 clampedPosition = new Vector3(Mathf.Clamp(WorldToScreenPos.x, 0, Screen.width), Mathf.Clamp(WorldToScreenPos.y, 0, Screen.height), WorldToScreenPos.z);
        return clampedPosition;
    }



    public int TargetIndex()
    {
        //Creates an array where the distances between the target and the screen/player will be stored
        float[] distances = new float[reachableTargets.Count];

        //Populates the distances array with the sum of the Target distance from the screen center and the Target distance from the player
        for (int i = 0; i < reachableTargets.Count; i++)
        {

            distances[i] =
                (Vector2.Distance(Camera.main.WorldToScreenPoint(reachableTargets[i].transform.position), MiddleOfScreen()) * screenDistanceWeight)
                +
                (Vector3.Distance(transform.position, reachableTargets[i].transform.position) * positionDistanceWeight);
        }

        //Finds the smallest of the distances
        float minDistance = Mathf.Min(distances);

        int index = 0;

        //Find the index number relative to the target with the smallest distance
        for (int i = 0; i < distances.Length; i++)
        {
            if (minDistance == distances[i])
                index = i;
        }

        return index;

    }

    public void StopTargetFocus()
    {
        arrowSystem.CancelFire(false);
        currentTarget = null;
    }

    void CheckTargetChange()
    {
        if (storedTarget != currentTarget)
        {
            storedTarget = currentTarget;
            rectImage.DOComplete();
            rectImage.DOScale(4, .2f).From();
        }
    }

    Vector2 MiddleOfScreen()
    {
        return new Vector2(Screen.width / 2, Screen.height / 2);
    }

    public void ClearStoredTarget()
    {
        storedTarget = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(lerpedTargetPos, 1f);
    }

    void CloneInterface(float chargeValue)
    {
        if (targetClonePrefab == null)
            return;

        ClonedTargetScript clonedTarget = Instantiate(targetClonePrefab, rectImage.position, rectImage.rotation, rectImage.parent);
        float sliderValue = chargeValue;
        clonedTarget.SetupClone(cachedTarget,transform, rectImage.sizeDelta, sliderValue);

    }

    void AddTargetCache()
    {
        cachedTarget = currentTarget.transform;
    }
}
