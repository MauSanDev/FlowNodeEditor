using System;

public class FlowNodeEditorField : Attribute
{
    public string Label { get; private set; }

    public FlowNodeEditorField(string label)
    {
        Label = label;
    }
}
