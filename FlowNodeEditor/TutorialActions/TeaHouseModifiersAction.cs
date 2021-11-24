using UnityEngine;

[System.Serializable]
public class TeaHouseModifiersAction : FlowNodeAction
{
    public bool revertOnDismiss;
    public bool allowPregamePopup;

    public override string ActionName => "Tea House Modifiers";

    public override void Execute()
    {
        ExecuteUIModifiers();
    }

    public override void Dismiss()
    {
        if(revertOnDismiss)
        {
            ExecuteUIModifiers(true);
        }
    }

    private void ExecuteUIModifiers(bool revert = false)
    {
        if(TeaHouseUI == null)
        {
            Debug.LogError($"{GetType()} :: The TeaHouseUI can't be found.");
            return;
        }

        TeaHouseUI.AllowPregamePopup = revert ? !allowPregamePopup : allowPregamePopup;
    }

    private TeaHouseUIManager TeaHouseUI => TeaHouseUIManager.instance;
}
