using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class StoryState : MonoBehaviour
{
    // TODO: how to edit in a nice way through editor?
    // - needs to be associated with story marker
    // - maybe some states needs specific triggers.. would be nice to link these in editor (specific collider for boundary checking?)
    // - could also use component to attach to story marker
    // - OR: link everything to StoryManager, and pass to state, so that it can be used in states
    //   - how to distinguish between different story markers? do they need to have names?

    public StoryMarker CorrespondingMarker;

    public StoryState()
    {

    }

    public StoryState UpdateState(object data)
    {
        return null;
    }

    public void OnEnter()
    {

    }

    public void OnExit()
    {

    }
}
