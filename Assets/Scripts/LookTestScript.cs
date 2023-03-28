using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookTestScript : MonoBehaviour
{
    Transform target;
    ArrowTarget arrowTargetSystem;

    // Start is called before the first frame update
    void Start()
    {
        target = FindObjectOfType<MovementInput>().transform;
        arrowTargetSystem = GetComponentInChildren<ArrowTarget>();
    }

    // Update is called once per frame
    void Update()
    {
        if(arrowTargetSystem.isAvailable)
            transform.LookAt(target);
    }
}
