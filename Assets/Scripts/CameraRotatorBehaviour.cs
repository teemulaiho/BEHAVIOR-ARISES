using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotatorBehaviour : MonoBehaviour
{
    public float speed = 50f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, speed * Time.deltaTime, 0);
    }
}
