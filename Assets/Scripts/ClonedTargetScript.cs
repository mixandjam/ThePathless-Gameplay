using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ClonedTargetScript : MonoBehaviour
{
    Slider slider;
    Transform referenceTransform;
    Transform playerTransform;
    RectTransform rect;
    CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] float fadeDuration;
    [SerializeField] float scaleDuration;
    [SerializeField] float scaleAmount;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        slider = GetComponent<Slider>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.DOFade(0, fadeDuration);
        transform.DOScale(scaleAmount, scaleDuration).SetEase(Ease.OutSine).OnComplete(()=> OnComplete());

    }

    private void Update()
    {

        if (referenceTransform != null)
        {
            Vector3 playerToObject = referenceTransform.position - playerTransform.position;
            bool isBehindPlayer = (Vector3.Dot(Camera.main.transform.forward, playerToObject) < 0);

            transform.position = isBehindPlayer ? transform.position : Camera.main.WorldToScreenPoint(referenceTransform.position);
        }
    }

    void OnComplete()
    {
        Destroy(gameObject);
    }

    public void SetupClone(Transform reference, Transform player, Vector2 sizeDelta, float sliderValue)
    {
        referenceTransform = reference;
        playerTransform = player;
        rect.sizeDelta = sizeDelta;
        slider.value = sliderValue;

        if(sliderValue == .5f || sliderValue == 1)
        {   
            transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.red;
        }
    }

    Vector3 ClampedScreenPosition(Vector3 targetPos)
    {
        Vector3 WorldToScreenPos = Camera.main.WorldToScreenPoint(targetPos);
        Vector3 clampedPosition = new Vector3(Mathf.Clamp(WorldToScreenPos.x, 0, Screen.width), Mathf.Clamp(WorldToScreenPos.y, 0, Screen.height), targetPos.z);
        return clampedPosition;
    }

}
