using System;
using UnityEngine;

[Serializable]
public class ObjectPointerAction : FlowNodeAction
{
    public IdentifierReference findableReference = new IdentifierReference();
    public AnchoringPresets.Direction pointingDirection = AnchoringPresets.Direction.Bottom;
    public bool rotatePointer = true;
    public bool worldObject = false;

    public override string ActionName => "Object Pointer";
    public override void Execute()
    {
        GameObject found = FindablesManager.GetRegisteredObject(findableReference.AssignedID);
        TeaHouseUtils.CreatePointer(found.transform.position, pointingDirection, rotatePointer, worldObject);
    }

    public override void Dismiss()
    {
        TeaHouseUtils.DeletePointer();
    }

    private FindableObjectsManager FindablesManager => FindableObjectsManager.instance;
}
