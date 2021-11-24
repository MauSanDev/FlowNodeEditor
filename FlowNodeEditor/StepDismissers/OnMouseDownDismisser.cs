using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OnMouseDownDismisser : FlowNodeStepDismisser
{
    public enum MouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2
    }

    public MouseButton button;

    public override void ListenUpdates()
    {
        if (Input.GetMouseButtonDown((int)button))
        {
            Dismiss();
        }
    }
}
