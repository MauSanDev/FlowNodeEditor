using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ObjectHighlighterAction : FlowNodeAction
{
    public IdentifierReference objectToHighlight = new IdentifierReference();
    public Highlight highlight = Highlight.Yes;
    public bool touchable = true;
    public bool revertOnDismiss = true;
    [field: NonSerializedAttribute()] private Highlighter toHighlight = null;


    public enum Highlight
    {
        Yes,
        No
    }

    public override string ActionName => "Highlight Object";

    public override void Execute()
    {
        toHighlight = FindableObjectsManager.instance.GetRegisteredObject<Highlighter>(objectToHighlight.AssignedID);

        if(toHighlight == null)
        {
            Debug.LogError($"The object of GUID {objectToHighlight.AssignedID} doesn't has a Highlighter!");
            return;
        }
        toHighlight.Highlight(highlight == Highlight.Yes, touchable);
    }

    public override void Dismiss()
    {
        if(toHighlight != null && revertOnDismiss)
        {
            toHighlight.Highlight(false);
        }
    }
}
