using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixAndJamExtra : MonoBehaviour
{

    private ArrowSystem arrow;
    private TargetSystem targetSystem;
    [SerializeField] Transform target;

    // Start is called before the first frame update
    void Start()
    {
        arrow = FindObjectOfType<ArrowSystem>();
        targetSystem = FindObjectOfType<TargetSystem>();  

        arrow.OnInputStart.AddListener(PlaceTarget);
    }


    void PlaceTarget()
    {
        target.position = targetSystem.currentTarget.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
