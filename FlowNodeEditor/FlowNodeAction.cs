using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public abstract class FlowNodeAction : ISerializationCallbackReceiver
{
    public string Type => GetType().ToString(); //Type is used for JSON Serialization.
    public virtual FlowNodeStepDismisser DependencyDismisser { get; }
    public abstract string ActionName { get; }
    public abstract void Execute();
    public abstract void Dismiss();
    public virtual void OnBeforeSerialize() { }
    public virtual void OnAfterDeserialize() { }

    public string GetJSON() => JsonConvert.SerializeObject(this);

    public static FlowNodeAction ConstructFromJSON(string jsonData)
    {
        Dictionary<string, object> deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
        Type typeToCreate = System.Type.GetType((string)deserialized["Type"]);

        FlowNodeAction action = (FlowNodeAction)JsonConvert.DeserializeObject(jsonData, typeToCreate);
        return action;
    }
}