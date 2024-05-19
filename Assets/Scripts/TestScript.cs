using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] Transform A_Tr;
    [SerializeField] Transform B_Tr;

    [ContextMenu("CalculateRotation")]
    public void CalculateRotation()
    {
        A_Tr.LookAt(B_Tr.position);
    }
}
