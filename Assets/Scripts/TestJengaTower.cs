using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestJengaTower : MonoBehaviour
{
    private List<SetJengaStacks.JengaParameters> data;
    private Rigidbody[] rigidbodies;
    [SerializeField] private Button testButton;

    private void OnEnable()
    {
        SetJengaStacks.OnParamatersReady += GetParameters;
    }

    private void OnDestroy()
    {
        SetJengaStacks.OnParamatersReady -= GetParameters;
    }

    private void GetParameters(List<SetJengaStacks.JengaParameters> obj)
    {
        data = obj;
        HandleRigidbodyAndColliders(data, true);
    }


    private void HandleRigidbodyAndColliders(List<SetJengaStacks.JengaParameters> parameters, bool isActive)
    {
        rigidbodies = new Rigidbody[parameters.Count];
        for (int i = 0; i < parameters.Count; i++)
        {
            for (int j = 0; j < parameters[i].numberOfJengas; j++)
            {
                rigidbodies[i] = parameters[i].jengaBlocks[j].GetComponent<Rigidbody>();
                rigidbodies[i].isKinematic = isActive;
            }
        }
    }

    private void RemoveGlassBlocks(List<SetJengaStacks.JengaParameters> parameters)
    {
        rigidbodies = new Rigidbody[parameters.Count];
        for (int i = 0; i < parameters.Count; i++)
        {
            for (int j = 0; j < parameters[i].numberOfJengas; j++)
            {
                if (parameters[i].blockType[j] == 0)
                {
                    Destroy(parameters[i].jengaBlocks[j]);
                }
            }
        }
    }


    public static Action OnStackTested;

    public void TestMyStack()
    {
        testButton.interactable = false;
        RemoveGlassBlocks(data);
        HandleRigidbodyAndColliders(data, false);
        OnStackTested?.Invoke();
    }
}