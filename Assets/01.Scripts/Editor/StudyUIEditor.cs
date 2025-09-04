#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// UI 컴포넌트 자동 연결 도우미 스크립트
/// </summary>
[CustomEditor(typeof(StudyUI))]
public class StudyUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        StudyUI studyUI = (StudyUI)target;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("UI 컴포넌트 자동 연결"))
        {
            AutoConnectComponents(studyUI);
        }
    }
    
    private void AutoConnectComponents(StudyUI studyUI)
    {
        // InfiniteScroll 찾기
        var infiniteScroll = studyUI.GetComponentInChildren<Gpm.Ui.InfiniteScroll>();
        if (infiniteScroll != null)
        {
            SerializedProperty scrollProp = serializedObject.FindProperty("studyScrollList");
            scrollProp.objectReferenceValue = infiniteScroll;
            Debug.Log("InfiniteScroll 연결 완료");
        }
        
        // 버튼들 찾기
        Button[] buttons = studyUI.GetComponentsInChildren<Button>();
        
        foreach (Button button in buttons)
        {
            if (button.name.ToLower().Contains("add"))
            {
                SerializedProperty addProp = serializedObject.FindProperty("addButton");
                addProp.objectReferenceValue = button.gameObject;
                
                // 버튼 이벤트 자동 연결
                var onClick = button.onClick;
                onClick.RemoveAllListeners();
                onClick.AddListener(studyUI.OnAddStudyItem);
                
                Debug.Log("Add Button 연결 완료");
            }
            else if (button.name.ToLower().Contains("start"))
            {
                SerializedProperty startProp = serializedObject.FindProperty("startButton");
                startProp.objectReferenceValue = button.gameObject;
                
                // 버튼 이벤트 자동 연결
                var onClick = button.onClick;
                onClick.RemoveAllListeners();
                onClick.AddListener(studyUI.OnStartStudy);
                
                Debug.Log("Start Button 연결 완료");
            }
        }
        
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(studyUI);
        
        Debug.Log("UI 컴포넌트 자동 연결 완료!");
    }
}
#endif

/// <summary>
/// 범용 UI 자동 연결 컴포넌트
/// </summary>
public class UIAutoConnector : MonoBehaviour
{
    [SerializeField] private bool autoConnectOnAwake = true;
    
    private void Awake()
    {
        if (autoConnectOnAwake)
        {
            ConnectUIComponents();
        }
    }
    
    /// <summary>
    /// UI 컴포넌트들을 이름 기반으로 자동 연결
    /// </summary>
    private void ConnectUIComponents()
    {
        var baseUI = GetComponent<BaseUI>();
        if (baseUI == null) return;
        
        // 모든 Button 컴포넌트 찾기
        Button[] buttons = GetComponentsInChildren<Button>(true);
        
        foreach (Button button in buttons)
        {
            string buttonName = button.name.ToLower();
            
            // 버튼 이름에 따른 자동 메서드 연결
            if (buttonName.Contains("close") || buttonName.Contains("back"))
            {
                button.onClick.AddListener(() => baseUI.CloseUI());
            }
        }
        
        Debug.Log($"{baseUI.GetType().Name} UI 컴포넌트 자동 연결 완료");
    }
}