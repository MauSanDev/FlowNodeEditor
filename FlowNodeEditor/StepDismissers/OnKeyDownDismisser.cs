using UnityEngine;

[System.Serializable]
public class OnKeyDownDismisser : FlowNodeStepDismisser
{
    [SerializeField] public KeyCode key;

    public override void ListenUpdates()
    {
        if(Input.GetKeyDown(key))
        {
            Dismiss();
        }
    }
}
