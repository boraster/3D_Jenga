using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Intro Canvas")] public Canvas intro;
    public float fadingTime = 1.0f;
    private CanvasGroup fader;

    [Header("In-game Canvas")] public Button testMyStackButton;
    public Button restartButton;
    public GameObject stackChangerPanel;

    [Header("Stack Label")] public Canvas label;
    public float offset = 1.0f;
    private Action OnStacksReady;

    [Header("Block Info")] public Canvas blockInfoCanvas;
    private CanvasGroup _canvasGroup;
    private GameObject parentGroup;
    private string[] textObjs;
    private TextMeshProUGUI gradeDomainText;
    private TextMeshProUGUI clusterText;
    private TextMeshProUGUI standardIdDescriptionText;
    private TextMeshProUGUI[] textComponents;
    [SerializeField] private Camera _camera;
    public LayerMask block;
    private Material objMat;

    [Header("Mouse Cursor")] public Texture2D cursor;

    private bool isHoveredOver;
    private List<SetJengaStacks.JengaParameters> _parametersList;
    private Dictionary<SetJengaStacks.StackType, List<BlockData>> _apiDataAsDictionary;
    private int selectedStack = 1;
    private Transform hitObj;
    private bool isHit;

    private void Awake()
    {
        fader = intro.GetComponent<CanvasGroup>();
        textObjs = new string[3];
        objMat = new Material(Shader.Find("Shader Graphs/BlockHighlight"));
        hitObj = transform;
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
        StartCoroutine(FadeOut(fadingTime));
    }

    private void OnEnable()
    {
        SetJengaStacks.OnParamatersReady += GetParameters;
        OnStacksReady += SetStackLabels;
        CameraController.OnSelectedStackChanged += SelectedStack;
        TestJengaTower.OnStackTested += DeactivateTestButton;
    }

    private void OnDestroy()
    {
        SetJengaStacks.OnParamatersReady -= GetParameters;
        OnStacksReady -= SetStackLabels;
        CameraController.OnSelectedStackChanged -= SelectedStack;
        TestJengaTower.OnStackTested -= DeactivateTestButton;
    }

    private void Start()
    {
        blockInfoCanvas = CreateInfoCanvas();
    }

    private void DeactivateTestButton()
    {
        testMyStackButton.interactable = false;
        testMyStackButton.gameObject.SetActive(false);
        stackChangerPanel.SetActive(false);
        restartButton.gameObject.SetActive(true);
    }

    public void RestartTheGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    private void SelectedStack(int stackIndex)
    {
        selectedStack = stackIndex;
    }

    private void ShowText()
    {
        if (Input.GetMouseButton(1))
        {
            var text = hitObj.GetComponent<JengaBlock>().infoText;
            var calcPoint = Vector3.zero;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(blockInfoCanvas.transform as RectTransform,
                Input.mousePosition, null, out calcPoint);
            Array.Clear(textObjs, 0, textObjs.Length);

            textObjs = text.Split("\r\n");
            parentGroup.transform.position = calcPoint + new Vector3(650 / 2f + 100f, 0, 0);
            _canvasGroup.alpha = 1f;
            gradeDomainText.text = textObjs[0];
            clusterText.text = textObjs[1];
            standardIdDescriptionText.text = textObjs[2];
        }
        else
        {
            _canvasGroup.alpha = 0f;
        }
    }


    private void Update()
    {
        HoverOverBlock();
        ShowText();
    }


    private void HoverOverBlock()
    {
        isHit = Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, 200f, block.value,
            QueryTriggerInteraction.Ignore);


        if (isHit)
        {
            objMat.SetFloat("_IsHoveredOver", 0);
            hitObj = hit.transform;
            objMat = hitObj.GetComponent<MeshRenderer>().material;
            objMat.SetFloat("_IsHoveredOver", 1);
        }
    }

    private Canvas CreateInfoCanvas()
    {
        var infoPanel = Instantiate(blockInfoCanvas);

        _canvasGroup = infoPanel.GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;

        parentGroup = infoPanel.transform.GetChild(0).gameObject;
        textComponents = infoPanel.GetComponentsInChildren<TextMeshProUGUI>();
        gradeDomainText = textComponents[0];
        clusterText = textComponents[1];
        standardIdDescriptionText = textComponents[2];

        return infoPanel;
    }

    private Dictionary<SetJengaStacks.StackType, List<string>> infos;

    private void CreateInfoStrings()
    {
        infos = new Dictionary<SetJengaStacks.StackType, List<string>>();
        var texts = new List<string>();


        for (var i = 0; i < _parametersList.Count; i++)
        {
            texts.Clear();
            for (var j = 0; j < _parametersList[i].stackData.Count; j++)
            {
                var infoString = _parametersList[i].stackData[j].grade + ": " +
                                 _parametersList[i].stackData[j].domain + "\r\n" +
                                 _parametersList[i].stackData[j].cluster + "\r\n" +
                                 _parametersList[i].stackData[j].standardid + ": " +
                                 _parametersList[i].stackData[j].standarddescription;

                var jengaComponent = _parametersList[i].jengaBlocks[j].GetComponent<JengaBlock>();
                jengaComponent.infoText = infoString;
                texts.Add(infoString);
            }

            infos[_parametersList[i].stackType] = texts;
        }
    }

    private void GetParameters(List<SetJengaStacks.JengaParameters> parametersList)
    {
        _parametersList = parametersList;
        // PrepareColliderList();  //Unfinished feature related two lines
        // ActivateSelectedStackColliders();
    }


    private void SetStackLabels()
    {
        for (var i = 0; i < _parametersList.Count; i++)
        {
            var stackPos = _parametersList[i].stackGO.transform.position;
            var stackRot = _parametersList[i].stackGO.transform.rotation;
            var offset = -Vector3.forward * this.offset;
            var labelCanvas = Instantiate(label.gameObject, stackPos + offset, stackRot,
                _parametersList[i].stackGO.transform);

            var textInstance = labelCanvas.GetComponentInChildren<TMP_Text>();
            var rawText = _parametersList[i].stackType.ToString().Split("_");

            var constructedString = string.Concat(rawText[1], " Grade");
            textInstance.text = constructedString;
        }
    }

    private IEnumerator FadeOut(float fadingTimer)
    {
        yield return new WaitForSeconds(fadingTime);
        OnStacksReady?.Invoke();
        CreateInfoStrings();

        while (fader.alpha > 0)
        {
            fader.alpha -= Time.deltaTime / fadingTimer;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        fader.blocksRaycasts = false;
    }

    #region UnfinishedFeature

    /*
     When focused stack is chnaged by the user,
     disable other stacks' interactivity
     */
    // private void ActivateSelectedStackColliders()
    // {
    //     
    //         for (int i = 0; i < _parametersList.Count; i++)
    //         {
    //             if (selectedStack == _parametersList[i].stackLocation)
    //             {
    //                 for (int j = 0; j < colliderPerStack[selectedStack].Count; j++)
    //                 {
    //                      colliderPerStack[selectedStack][j].isTrigger = false;
    //                 }
    //                
    //             }
    //             else if (selectedStack != _parametersList[i].stackLocation)
    //             {
    //                 for (int j = 0; j < colliderPerStack[i].Count; j++)
    //                 {
    //                     colliderPerStack[i][j].isTrigger = true;
    //                 }
    //                 
    //             }
    //         }
    //     
    //
    //     
    // }
    // private Dictionary<int, List<BoxCollider>> colliderPerStack;
    // private void PrepareColliderList()
    // {
    //     colliderPerStack = new Dictionary<int, List<BoxCollider>>();
    //     for (int i = 0; i < _parametersList.Count; i++)
    //     {
    //         List<BoxCollider> colliderList = new List<BoxCollider>();
    //         for (int j = 0; j < _parametersList[i].jengaBlocks.Count; j++)
    //         {
    //             colliderList.Add(_parametersList[i].jengaBlocks[j].GetComponent<BoxCollider>());
    //         }
    //         colliderPerStack.Add(i,colliderList);
    //     }
    // }

    #endregion
}