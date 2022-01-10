using UnityEngine;
using UnityEngine.Serialization;

public class ColliderTrigger : StoryTrigger
{
    [SerializeField]
    BoxCollider Collider;

    [SerializeField] 
    bool TriggerOnEnter;
    

    // Start is called before the first frame update
    void Start()
    {
        if (Collider == null)
        {
            Collider = GetComponent<BoxCollider>();
        }
        if (null == Collider)
        {
            Debug.LogError("Collider is null");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered collider");
            if (TriggerOnEnter)
            {
                Activate(null);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player left collider");
            if (!TriggerOnEnter) // trigger on leave 
            {
                var go = other.gameObject;

                // check, if we are really exiting the room
                var triggerDir = this.transform.right;
                var exitPos = go.transform.position;
                var diff = this.transform.position - exitPos;
                var dot = Vector3.Dot(triggerDir, diff);
                if (dot > 0)
                {
                    Debug.Log("Player exitted in exit dir");
                    Activate(null);
                } 
                else
                {
                    Debug.Log("Player exitted NOT in exit dir");
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
