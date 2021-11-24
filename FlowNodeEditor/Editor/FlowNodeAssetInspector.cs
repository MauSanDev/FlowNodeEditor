using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class FlowNodeAssetHandler
{
    [OnOpenAsset]
    public static bool OpenOnTutorialEditor(int instanceId, int line)
    {
        if(EditorUtility.InstanceIDToObject(instanceId) is FlowNodeScriptData tutorial)
        {
            FlowNodeEditor.Open(tutorial);
            return true;
        }
        return false;
    }
}

public class FlowNodeAssetInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open on Tutorial Editor"))
        {
            FlowNodeEditor.Open(target as FlowNodeScriptData);
        }

        base.OnInspectorGUI();
    }
}
