using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class CannotSee : MonoBehaviour
{
    private Image _img;
    // Start is called before the first frame update
    void Start()
    {
        _img = GetComponent<UnityEngine.UI.Image>();
    }

    public void SetVisibility(bool canSee)
    {
        _img.enabled = !canSee;
    }
}
