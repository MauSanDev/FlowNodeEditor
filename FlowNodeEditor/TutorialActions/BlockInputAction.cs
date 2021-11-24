using System.Collections.Generic;

[System.Serializable]
public class BlockInputAction : FlowNodeAction
{
    [FlowNodeEditorField("Revert on Dismiss:")] public bool revertOnDismiss = false;
    [FlowNodeEditorField("To Do:")]public BlockParameter toDo = BlockParameter.Block;
    [FlowNodeEditorField("Block Back Button:")]public bool blockBackButton = true;
    [FlowNodeEditorField("To Ignore:")]public List<IdentifierReference> toIgnore = new List<IdentifierReference>();

    public override string ActionName => "Block Input";

    public enum BlockParameter
    {
        Block,
        Enable
    }

    public override void Execute()
    {
        bool blockStatus = toDo == BlockParameter.Block ? false : true;
        InputManager.EnableInput(blockStatus, FindablesToIgnore);
        InputManager.BackButtonEnabled = !blockBackButton;
    }

    public override void Dismiss()
    {
        if(revertOnDismiss)
        {
            bool blockStatus = toDo == BlockParameter.Block ? true : false;
            InputManager.EnableInput(blockStatus, FindablesToIgnore);
            InputManager.BackButtonEnabled = blockBackButton;
        }
    }

    private List<string> FindablesToIgnore
    {
        get
        {
            List<string> ignore = new List<string>();
            foreach (IdentifierReference findable in toIgnore)
            {
                ignore.Add(findable.AssignedID);
            }
            return ignore;
        }
    }


    private InputManager InputManager => InputManager.Instance;
}
