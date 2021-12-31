using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DDOL : MonoBehaviour
{
    public void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
    }
}
