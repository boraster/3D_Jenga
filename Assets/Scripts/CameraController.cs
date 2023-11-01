    
    using System;
    using System.Collections.Generic;
    using Cinemachine;
    using UnityEngine;

    

   
    public class CameraController: MonoBehaviour
    {
        [SerializeField]private CinemachineFreeLook cam;
        [SerializeField] CinemachineVirtualCamera mainCam;
        private GameObject fokalPointGO;
        private List<GameObject> _stacks;
        private List<SetJengaStacks.JengaParameters> _parametersList;

      
        private void OnEnable()
        {
            SetJengaStacks.OnParamatersReady += GetParameters;
            
        }

        private void OnDestroy()
        {
            SetJengaStacks.OnParamatersReady -= GetParameters;
        }

        private void ChangeCam()
        {
            if (Input.GetMouseButton(0))
            {
                mainCam.Priority = 9;
            }

            else
            {
                mainCam.Priority = 11;
            }
        }

        [SerializeField]private int stackIndex = 1;
        private void ChangeFocusedStack()
        {
            if (Input.GetKeyDown(KeyCode.A) )
            {
                stackIndex--;
                SetCamToSelectedStack(allFokalPoints, stackIndex);
                SetRigOverrideValues(rigHeights, FokalPointDatas, stackIndex );
                OnSelectedStackChanged?.Invoke(stackIndex);
            }
            
            if (Input.GetKeyDown(KeyCode.D) )
            {
                stackIndex++;
                SetCamToSelectedStack(allFokalPoints,stackIndex);
                SetRigOverrideValues(rigHeights, FokalPointDatas, stackIndex);
                OnSelectedStackChanged?.Invoke(stackIndex);
            }
        }
        private void Update()
        {
           ChangeCam();
           
           ChangeFocusedStack();
        }

        private Dictionary<int, FokalPointData> FokalPointDatas;
        public struct FokalPointData
        {
            public List<Transform> camFokalPointsForCam;
            public int midPoint;
        }
        private Dictionary<int, FokalPointData> CalculateFokalPoint( List<SetJengaStacks.JengaParameters> parametersList)
        {
            FokalPointDatas = new Dictionary<int, FokalPointData>();
            
            for (int i = 0; i < parametersList.Count; i++)
            {
                FokalPointData data = new FokalPointData();
                data.camFokalPointsForCam = new List<Transform>();
                
                var bottomPoint =  1;
                data.camFokalPointsForCam.Add(CalculatePosForCamFokalPoint(parametersList,i, bottomPoint, parametersList[i].jengaBlocks));
              
                var midPoint = Mathf.CeilToInt(parametersList[i].numberOfJengas / 2.0f);
                
                FindMiddleBlock(parametersList, ref data, midPoint, i); 
                
                var topPoint = parametersList[i].numberOfJengas-1;
                FindMiddleBlock(parametersList,  ref data, topPoint, i); 
                
                FokalPointDatas.Add(i, data);
            }
    
            return  FokalPointDatas;
        }

        private void FindMiddleBlock(List<SetJengaStacks.JengaParameters> parametersList, ref FokalPointData data, int midPoint,
            int i)
        {
            var isThisMiddleBlock = midPoint % 3 == 1;
            
            if (isThisMiddleBlock)
            {
                data.midPoint = midPoint;
                data.camFokalPointsForCam. Add(CalculatePosForCamFokalPoint(parametersList, i, midPoint, parametersList[i].jengaBlocks));
            }
            else
            {
               
                midPoint--;
                FindMiddleBlock(parametersList, ref data, midPoint, i);
            }
        }

        private float lowOrbitHeight;
        private float highOrbitHeight;
        private int blockHeightFromOrigin;
        private Transform CalculatePosForCamFokalPoint(List<SetJengaStacks.JengaParameters> parametersList, int i, int blockIndex, List<GameObject> blocks )
        {
            fokalPointGO = new GameObject("Fokal Point for Cam");
            var fokalPointGOTransform = fokalPointGO.transform;
            var fokalPointGOPos = fokalPointGOTransform.position;
            
            var jengaBlockTransform = blocks[blockIndex].transform;
            var jengaBlockPos = jengaBlockTransform.position;
            var jengaBlockScale = jengaBlockTransform.localScale;
    
            fokalPointGOTransform.SetParent(jengaBlockTransform);


            var levelOfOurBlock = Mathf.FloorToInt((float)blockIndex / 3) ;
            var isBlockLevelRotated = Mathf.FloorToInt((float)levelOfOurBlock % 2) == 1;
            
            if (isBlockLevelRotated)
            {
                fokalPointGOPos = jengaBlockPos + new Vector3(-jengaBlockScale.z / 2.0f, jengaBlockScale.y / 2.0f, jengaBlockScale.x / 2.0f);
                fokalPointGOTransform.position = fokalPointGOPos;
            }
            else
            {
                fokalPointGOPos = jengaBlockPos + new Vector3(jengaBlockScale.x / 2.0f, jengaBlockScale.y / 2.0f, jengaBlockScale.z / 2.0f);
                fokalPointGOTransform.position = fokalPointGOPos;
            }
            
            
            return fokalPointGOTransform;
        }
    
        private void GetParameters(List<SetJengaStacks.JengaParameters> parametersList)
        {
            
            _parametersList = parametersList;
            InitialSetup(_parametersList);
            
        }

        private Dictionary<int, FokalPointData> allFokalPoints;
        private void InitialSetup(List<SetJengaStacks.JengaParameters> parametersList)
        {
             allFokalPoints = CalculateFokalPoint(parametersList);


            
            SetCamToSelectedStack(allFokalPoints);
            
            rigHeights = CalculateRigHeights(parametersList, FokalPointDatas);
            SetRigOverrideValues(rigHeights, FokalPointDatas);
        }

        public static Action<int> OnSelectedStackChanged;
        private void SetCamToSelectedStack(Dictionary<int, FokalPointData> data,  int selectedStack =1)
        {
            selectedStack %= 3;
            mainCam.Follow = data[selectedStack].camFokalPointsForCam[1];
            mainCam.LookAt = mainCam.Follow;
            cam.Follow = mainCam.Follow;
            cam.LookAt = mainCam.Follow;

            
        }
        private Dictionary<int, List<float>> rigHeights;
        private Dictionary<int, List<float>> CalculateRigHeights(List<SetJengaStacks.JengaParameters> parametersList, Dictionary<int, FokalPointData> fokalPointData )
        {
            rigHeights = new Dictionary<int, List<float>>();
        
            for (int i = 0; i < parametersList.Count; i++)
            {
                List<float> heights = new List<float>();
                
                for (int j =0 ; j <  parametersList[i].stackLocation ; j++)
                {
                   
                    var heightOfAJengaBlock = parametersList[j].jengaBlocks[j].transform.localScale.y;

                    var fokalPointBlockLevel = (fokalPointData[i].midPoint / 3) * heightOfAJengaBlock;
                    
                    cam.m_Orbits[2].m_Height = heightOfAJengaBlock / 2.0f ;
                    heights.Add(cam.m_Orbits[0].m_Height);
                    
                    cam.m_Orbits[1].m_Height = fokalPointBlockLevel;
                    heights.Add(cam.m_Orbits[1].m_Height);
                    highOrbitHeight = heightOfAJengaBlock * 1.5f * parametersList[j].blockLevel;
                    cam.m_Orbits[0].m_Height = highOrbitHeight;
                    heights.Add(cam.m_Orbits[2].m_Height);
                }
                rigHeights.Add(i, heights);
            }

            return rigHeights;

        }

        private void SetRigOverrideValues(Dictionary<int, List<float>> heights, Dictionary<int, FokalPointData> fokalPointData, int selectedStack =1)
        { 
            selectedStack %= 3;

            var index = heights.Keys.Count -1;
            for (int i = 0; i < heights.Keys.Count ; i++)
            {
               
                    cam.GetRig(index).m_LookAt = fokalPointData[selectedStack].camFokalPointsForCam[i];
                    index--;
            }
        }
        
        
       
    }
