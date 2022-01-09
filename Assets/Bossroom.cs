using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bossroom : MonoBehaviour
{
    private Animator animator;
    void Start()
    { 
        animator = GetComponent<Animator>(); 
        EventManager.StartListening("Boss/Rising",riseLava);   
        EventManager.StartListening("Boss/Death",endAnimation);   
        EventManager.StartListening("Boss/Ready",startLavaAnimation);   
        
        
    }

    private void riseLava(object arg0)
    {
        animator.SetTrigger("BossRising");
    }

    private void endAnimation(object arg0)
    {
        animator.SetTrigger("BossDied");
    }

    private void startLavaAnimation(object arg0)
    {
        animator.SetTrigger("BossReady");
    }

    public void ActivateLava()
    {
        var lava = FindObjectOfType<LavaController>();
        lava.LavaIsActive = true;
    }

    public void DeactivateLava()
    {
        var lava = FindObjectOfType<LavaController>();
        lava.LavaIsActive = false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
