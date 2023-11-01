using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public class SetJengaStacks : MonoBehaviour
{
    

    [Flags]
    public enum StackType
    {
        Sixth_6th = 1,
        Seventh_7th = 2,
        Eighth_8th = 4,
        Algebra_I = 8
    }
    

   public StackType gradeSelection = StackType.Sixth_6th;

    private void OnEnable()
    {
        FetchApiData.OnDataFetched += GetData;
    }

    private void OnDestroy()
    {
        FetchApiData.OnDataFetched -= GetData;
    }

    private string fetchedData;
    public static Action< List<JengaParameters>> OnParamatersReady;
    public static Action<Dictionary<StackType, List<BlockData>>> OnApiDataAsDictionary;
    private List<BlockData> blockData;

    
    private Dictionary<StackType, List<BlockData>> stackTypeBasedOrderedBlockData;

    private void GetData(string data)
    {
        fetchedData = data;
        DeserializeJSONData(fetchedData);
    }

    
    public void DeserializeJSONData(string data)
    {
        blockData = JsonConvert.DeserializeObject<List<BlockData>>(data);
        string[] grades = new string[blockData.Count];

        for (int i = 0; i < blockData.Count; i++)
        {
            grades[i] = blockData[i].grade;
        }

        var uniqueGrades = grades.Distinct().ToList();

        var domainClusterIDOrderedData =
            blockData.OrderBy(y => y.domain).ThenBy(y => y.cluster).ThenBy(y => y.id)
                .GroupBy(x => x.grade).ToDictionary(x => x.Key, x => x.ToList());

        stackTypeBasedOrderedBlockData = new Dictionary<StackType, List<BlockData>>();

        for (int i = 0; i < domainClusterIDOrderedData.Count; i++)
        {
            var gradeSpecificValues = domainClusterIDOrderedData[uniqueGrades[i]];
            stackTypeBasedOrderedBlockData.Add((StackType)Mathf.Pow(2, i), gradeSpecificValues);
        }

       

        stackTypeBasedOrderedBlockData = stackTypeBasedOrderedBlockData.Where(x => (x.Key & gradeSelection) != 0)
            .ToDictionary(x => x.Key, x => x.Value);

       

        SetJengaBlocks();
    }


    public int numberOfBlocksOnALevel = 3;
    public float distanceBetweenBlocks = 0.25f;
    public float rotationAngleAroundYAxis = 90f;
    public GameObject stackParent;
    [SerializeField] private float distanceBetweenStacks = 5f;
    [HideInInspector]public List<JengaParameters> parameters;
     public List<GameObject> jengaBlocksTypes;

   
    public struct JengaParameters
    {
        public int blockLevel;
        public int numberOfJengas;
        public List<int> blockType;
        public int stackLocation;
        public GameObject stackGO;
        public List<GameObject> jengaBlocks;
        public StackType stackType;
        public  List<BlockData> stackData;
    }

    private GameObject CreateStackParent(int stack)
    {
        Vector3 posOffset = new Vector3(distanceBetweenStacks * stack, 0, 0);
      return  Instantiate(stackParent, stackParent.transform.position + posOffset, Quaternion.identity);
    }
    private void SetJengaBlocks()
    {
       
        parameters = new List<JengaParameters>();
 
        Vector3 offsetZ;
        Vector3 offsetX;
        Quaternion blockRotator;
        Quaternion blockOrientation = Quaternion.identity;
        foreach (var jenga in stackTypeBasedOrderedBlockData)
        {
            
            if ((jenga.Key & gradeSelection) != 0)
            {

                var parameter = new JengaParameters();
                parameter.jengaBlocks = new List<GameObject>();
                parameter.blockType = new List<int>();
                
                parameter. numberOfJengas = jenga.Value.Count;
                parameter. blockLevel = -1;
                parameter. stackLocation = (int)Mathf.Log((float)jenga.Key, 2);
                 parameter.stackGO = CreateStackParent( parameter. stackLocation);
                 parameter.stackType = (StackType)(jenga.Key & gradeSelection);
                 parameter.stackData = stackTypeBasedOrderedBlockData[parameter.stackType];
                 
                 // Debug.Log(parameter.stackType);
                 
                for (int i = 0; i < parameter.numberOfJengas; i++)
                {

                    var nextJengaLocation = i % numberOfBlocksOnALevel;
                    parameter. blockLevel = nextJengaLocation == 0 ? parameter. blockLevel + 1 : parameter. blockLevel;

                  


                    offsetZ = new Vector3(
                        jengaBlocksTypes [jenga.Value[i].mastery].transform.localScale.x * nextJengaLocation +
                        distanceBetweenBlocks * nextJengaLocation,
                        jengaBlocksTypes[jenga.Value[i].mastery].transform.localScale.y * parameter. blockLevel, 0);

                    offsetX = new Vector3(jengaBlocksTypes[jenga.Value[i].mastery].transform.localScale.z,
                        jengaBlocksTypes[jenga.Value[i].mastery].transform.localScale.y * parameter. blockLevel,
                        jengaBlocksTypes[jenga.Value[i].mastery].transform.localScale.x * nextJengaLocation +
                        distanceBetweenBlocks * nextJengaLocation);

                    Vector3 currentlyActiveOffset = parameter. blockLevel % 2 == 0 ? offsetZ : offsetX;


                    if (nextJengaLocation != 0)
                    {
                        parameter = InstantiateJengaBlock(parameter. blockLevel, parameter, jenga, i,  parameter.stackGO, currentlyActiveOffset);
                    }

                    else
                    {
                        parameter = InstantiateJengaBlock(parameter. blockLevel, parameter, jenga, i,  parameter.stackGO, currentlyActiveOffset);
                    }
                }
                
                parameters.Add(parameter);
            }
        }
        
        OnParamatersReady?.Invoke( parameters);
    }

    private JengaParameters InstantiateJengaBlock(int blockLevel, JengaParameters jengaParameter, KeyValuePair<StackType, List<BlockData>> jenga, int i, GameObject stack, Vector3 currentlyActiveOffset)
    {
        Quaternion blockRotator;
        Quaternion blockOrientation;
        
        blockRotator = blockLevel % 2 == 0
            ? Quaternion.identity
            : Quaternion.Euler(0, -rotationAngleAroundYAxis, 0);
        
        blockOrientation = jengaBlocksTypes[jenga.Value[i].mastery].transform.rotation * blockRotator;


        var blockInstance = Instantiate(jengaBlocksTypes[jenga.Value[i].mastery],
            stack.transform.position + currentlyActiveOffset, blockOrientation, stack.transform);
        var jengaComponent =blockInstance.AddComponent<JengaBlock>();
        jengaComponent.index = i;
        
        jengaParameter.blockType.Add(jenga.Value[i].mastery) ;
        jengaParameter.jengaBlocks.Add(blockInstance);
        return jengaParameter;
    }
}