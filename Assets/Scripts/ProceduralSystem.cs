using UnityEngine;
using UnityEngine.Animations.Rigging;
using DG.Tweening;

public class ProceduralSystem : MonoBehaviour
{

    ArrowSystem arrowSystem;
    TargetSystem targetSystem;
    bool rigEnabled;

    [Header("Animation Rigging Connections")]
    [SerializeField] Rig aimRig;
    [SerializeField] Rig ikRig;
    [SerializeField] Rig pullRig;
    [SerializeField] Transform aimRigTarget;

    [Header("Bow Connections")]
    [SerializeField] Transform bowTransform;
    [SerializeField] Transform bowDefaultReference;
    [SerializeField] Transform bowActionReference;
    [SerializeField] LineRenderer bowLineRenderer;
    [SerializeField] Transform bowLineCenter;
    [SerializeField] Transform bowLineHandReference;
    [SerializeField] GameObject arrowInLineGO;

    [Header("Settings")]
    [SerializeField] float transitionSpeed = .1f;
    [SerializeReference] float stringPullAmount = 0.2f;

    private void Start()
    {
        arrowSystem = GetComponent<ArrowSystem>();
        targetSystem = GetComponent<TargetSystem>();

        arrowSystem.OnInputStart.AddListener(EnableRig);
        arrowSystem.OnInputRelease.AddListener(DisableRig);
        arrowSystem.OnTargetLost.AddListener(DisableRig);

        arrowInLineGO.SetActive(false);
    }

    void Update()
    {
        if (arrowSystem.isCharging && targetSystem.storedTarget != null)
            aimRigTarget.position = targetSystem.storedTarget.transform.position;

        bowLineRenderer.SetPosition(1, bowLineCenter.localPosition);

    }

    void EnableRig()
    {
        rigEnabled = true;
        arrowInLineGO.SetActive(true);

        DOTween.Complete(5);
        DOTween.Complete(7);
        DOVirtual.Float(aimRig.weight, 1, transitionSpeed/2, SetAimRigWeight).SetId(5);

        SetPullWeight(0);
        DOVirtual.Float(pullRig.weight, 1, transitionSpeed, SetPullWeight).SetId(6);
        bowLineCenter.DOLocalMoveZ(-stringPullAmount, transitionSpeed, false);
    }

    void DisableRig()
    {
        rigEnabled = false;
        arrowInLineGO.SetActive(false); 

        bowLineCenter.DOLocalMoveZ(0, .4f, false).SetEase(Ease.OutElastic);
        bowLineCenter.DOLocalMoveX(0, .4f, false).SetEase(Ease.OutElastic);

        DOTween.Complete(5);
        DOTween.Kill(6);

        DOVirtual.Float(aimRig.weight, 0, transitionSpeed, SetAimRigWeight).SetDelay(.4f).SetId(7);

    }


    public void SetAimRigWeight(float weight)
    {
        aimRig.weight = weight;
        ikRig.weight = weight;
    }

    void SetPullWeight(float weight)
    {
        pullRig.weight = weight;
    }

}
