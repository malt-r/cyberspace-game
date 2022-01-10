using UnityEngine;

public class MinigameStatus : MonoBehaviour
{
    [SerializeField] private string name;
    
    public bool isDone = false;
    private StoryTrigger _storyTrigger;

    private Animator _animator;

    public string Name => name;
    
    // Start is called before the first frame update
    void Start()
    {
        _storyTrigger = GetComponent<StoryTrigger>();
        _animator = GetComponent<Animator>();
        _animator.SetTrigger("Start");
    }

    // Update is called once per frame
    void Update()
    {
        
        //if true irgendwas tun?
    }

    public void SetDone()
    {
        isDone = true;
        
        if (_storyTrigger != null)
        {
            _storyTrigger.Activate(null);
        }

        if (_animator != null)
        {
            _animator.SetTrigger("End");
        }
    }
}
