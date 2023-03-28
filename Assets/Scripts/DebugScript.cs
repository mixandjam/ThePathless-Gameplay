using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DebugScript : MonoBehaviour
{

#if UNITY_EDITOR
    void OnPause()
    {
        EditorApplication.isPaused = !EditorApplication.isPaused;
    }
#endif
}
