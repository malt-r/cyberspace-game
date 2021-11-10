using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorStats))]
public class Enemy : MonoBehaviour
{
    public Transform player;
    public float Speed = 1f;
    public float minFollowDistance = 5f;
    public float maxFollowDistance = 10f;
    [SerializeField]
    private bool isFollowing = false;
    public float distance;
    private ActorStats stats;

    public bool inverse = true;
    // Start is called before the first frame update
    void Start()
    {
        stats = GetComponent<ActorStats>();
        Debug.Log(transform.localScale);
    }

    // Update is called once per frame
    void Update()
    {
        float deadRatio = 1;
        
        deadRatio = (stats.CurrentHealth+1)  / stats.maxHealth;

        if (inverse)
        {
            deadRatio = 2/ (deadRatio+1);
        }
        var newScale = new Vector3(100f * deadRatio, 100f * deadRatio, 100f * deadRatio);
    
        transform.localScale = newScale;
        distance = Vector3.Distance(transform.position, player.transform.position);

        isFollowing = false;
        if (distance > maxFollowDistance) { return; }
        if (distance < minFollowDistance) { return; }
        isFollowing = true;
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Time.deltaTime * Speed);


        //transform.position = Vector3.Lerp(transform.position, player.position, Time.deltaTime/10);
    }
}
