using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardBehaviour : MonoBehaviour
{
    public Transform cam;

    public void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}
