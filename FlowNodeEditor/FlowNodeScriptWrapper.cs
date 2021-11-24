using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FlowNode/FlowNode List")]
public class FlowNodeScriptWrapper : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] private List<FlowNodeScriptData> scriptList = new List<FlowNodeScriptData>();
    public List<FlowNodeScriptData> List => scriptList;

    public bool TryGetScript(string id, out FlowNodeScriptData flowNodeScript)
    {
        flowNodeScript = scriptList.Find(x => x.Identifier == id);

        if (flowNodeScript == null)
        {
            return false;
        }

        if (flowNodeScript.Dependencies.Count > 0)
        {
            foreach(string dependencyID in flowNodeScript.Dependencies)
            {
                FlowNodeScriptData dependency = scriptList.Find(x => x.Identifier == dependencyID);
                if(dependency == null)
                {
                    Debug.LogError($"The tutorial {flowNodeScript.name} as a Dependency Identified as {dependencyID} that couldn't be found. Will be skipped.");
                    continue;
                }

                if(!dependency.WasShown)
                {
                    Debug.LogError($"The tutorial {flowNodeScript.name} depends on {dependencyID} and it wasn't seen yet.");
                    flowNodeScript = null;
                    return false;
                }
            }
        }

        //TODO: CHeck if this is needed
        // flowNodeScript.Deserialize();
        return true;
    }

    public void OnBeforeSerialize()
    {
        scriptList.RemoveAll(script => script == null);
    }

    public void OnAfterDeserialize() { }
}
