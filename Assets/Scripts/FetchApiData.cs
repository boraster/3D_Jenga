using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FetchApiData : MonoBehaviour
{
    public static Action<string> OnDataFetched;

    void Start()
    {
        StartCoroutine(GetApiDataByHttp());
    }

    IEnumerator GetApiDataByHttp()
    {
        UnityWebRequest www =
            UnityWebRequest.Get("https://ga1vqcu3o1.execute-api.us-east-1.amazonaws.com/Assessment/stack");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            var fetchedRawData = www.downloadHandler.text;
            OnDataFetched?.Invoke(fetchedRawData);
        }
    }
}