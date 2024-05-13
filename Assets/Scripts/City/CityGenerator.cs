using System;
using System.Collections.Generic;
using System.Linq;

using ProcCityGen.Interfaces.Fields.Tensor;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

using UnityEngine;

[Serializable]
[ShowOdinSerializedPropertiesInInspector]
public class CityGenerator : MonoBehaviour, ISerializationCallbackReceiver
{
    [MinValue(1)]
    public int width, height;

    [OdinSerialize, TypeFilter("GetITensorFieldTypeList"), Delayed, OnValueChanged("GenerateStreamlines", true)]
    public ITensorField field;

    [SerializeField]
    private LineIntegralConvolution lineIntegralConvolution;

    [SerializeField, OnValueChanged("GenerateStreamlines", true)]
    private RoadNetwork roadNetwork;

    [SerializeField]
    private bool autoCreateStreamlines = false;
    
    [Button]
    public void StartCityGeneration()
    {
        RenderLic();
        roadNetwork.GenerateStreamlines(field, width, height);
    }

    private void RenderLic()
    {
        lineIntegralConvolution.RenderLic(field, width, height);
    }
    
    private void GenerateStreamlines()
    {
        if (!autoCreateStreamlines)
        {
            return;
        }
        roadNetwork.GenerateStreamlines(field, width, height);
    }

    private void OnDrawGizmos()
    {
        roadNetwork.DrawStreamlines();
    }

    // ReSharper disable once UnusedMember.Local
    private IEnumerable<Type> GetITensorFieldTypeList()
    {
        IEnumerable<Type> q = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => typeof(ITensorField).IsAssignableFrom(x));

        return q;
    }

    #region ISerializationCallbackReceiver
    [SerializeField, HideInInspector]
    private SerializationData serializationData;

    public void OnAfterDeserialize()
    {
        UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
    }

    public void OnBeforeSerialize()
    {
        UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
    }
    #endregion
}
