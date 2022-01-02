using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMe : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeeds;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationSpeeds*Time.deltaTime);
    }
}
