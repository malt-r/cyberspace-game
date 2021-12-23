using UnityEngine;

public class LeavingTrigger : StoryTrigger
{
    BoxCollider _collider;

    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<BoxCollider>();
        if (null == _collider)
        {
            Debug.LogError("Collider is null");
        }
        else
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered collider");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
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
                Activate();
            } 
            else
            {
                Debug.Log("Player exitted NOT in exit dir");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
