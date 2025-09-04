using System.Linq;
using Gpm.Ui;
using UnityEngine;
using Logger = Common.Logger;

/// <summary>
/// 단순화된 공부 계획 관리 UI 클래스
/// </summary>
public class StudyUI : BaseUI
{
    [Header("필수 컴포넌트")]
    [SerializeField] private InfiniteScroll studyScrollList;
    [SerializeField] private GameObject addButton;
    [SerializeField] private GameObject startButton;
    
    private UserStudyData _studyData;

    protected override void OnInit()
    {
        base.OnInit();
        ValidateComponents();
    }
    
    protected override void OnSetting(BaseUIData data)
    {
        base.OnSetting(data);
        RefreshStudyList();
    }

    /// <summary>
    /// 컴포넌트 검증 - 단순화
    /// </summary>
    private void ValidateComponents()
    {
        bool isValid = studyScrollList != null && addButton != null && startButton != null;
        
        if (!isValid)
        {
            Logger.LogError($"{GetType()}::필수 컴포넌트가 누락되었습니다.");
        }
    }

    /// <summary>
    /// 공부 리스트 새로고침 - 간소화
    /// </summary>
    public void RefreshStudyList()
    {
        studyScrollList?.Clear();
        
        _studyData = UserDataManager.Instance?.GetUserData<UserStudyData>();
        
        if (_studyData?.StudyItemDataList == null)
        {
            Logger.LogWarning($"{GetType()}::공부 데이터가 없습니다.");
            return;
        }
        
        var scrollDataArray = _studyData.StudyItemDataList
            .Select(_ => new InfiniteScrollData()) // 필요시 생성자에 데이터 전달
            .ToArray();

        if (studyScrollList != null)
        {
            studyScrollList.InsertData(scrollDataArray, true);
        }

        Logger.Log($"{GetType()}::공부 리스트 새로고침 완료");
    }

    /// <summary>
    /// 공부 항목 추가 - 단순화
    /// </summary>
    public void OnAddStudyItem()
    {
        _studyData ??= UserDataManager.Instance?.GetUserData<UserStudyData>();

        if (_studyData != null)
        {
            var newItem = new StudyItemData
            {
                id = _studyData.StudyItemDataList.Count + 1,
                name = "",
                check = false
            };

            _studyData.StudyItemDataList.Add(newItem);
        }

        RefreshStudyList();
        
        Logger.Log($"{GetType()}::새 공부 항목 추가");
    }

    /// <summary>
    /// 공부 시작 - 검증 및 시작 로직 간소화
    /// </summary>
    public void OnStartStudy()
    {
        if (!IsStudyDataValid())
        {
            ShowSimpleMessage("공부 계획을 먼저 작성해주세요.");
            return;
        }

        StartStudySession();
    }

    /// <summary>
    /// 데이터 유효성 검사 - 단순화
    /// </summary>
    private bool IsStudyDataValid()
    {
        if (_studyData?.StudyItemDataList == null || _studyData.StudyItemDataList.Count == 0)
            return false;

        return _studyData.StudyItemDataList.All(item => !string.IsNullOrEmpty(item.name?.Trim()));
    }

    /// <summary>
    /// 공부 세션 시작
    /// </summary>
    private void StartStudySession()
    {
        UIManager.Instance?.OpenUI<StudyingUI>(new BaseUIData());
        CloseUI();
    }

    /// <summary>
    /// 간단한 메시지 표시
    /// </summary>
    private void ShowSimpleMessage(string message)
    {
        var modalData = new ModalUIData
        {
            Type = ModalType.Ok,
            Title = "알림",
            Desc = message,
            OkBtnText = "확인"
        };
        
        UIManager.Instance?.OpenUI<ModalUI>(modalData);
    }

    /// <summary>
    /// 공부 항목 삭제
    /// </summary>
    public void DeleteStudyItem(int itemId)
    {
        if (_studyData?.StudyItemDataList == null) return;

        var itemToRemove = _studyData.StudyItemDataList.FirstOrDefault(item => item.id == itemId);
        if (itemToRemove != null)
        {
            _studyData.StudyItemDataList.Remove(itemToRemove);
            _studyData.SaveData();
            RefreshStudyList();
        }
    }

    /// <summary>
    /// 공부 항목 업데이트
    /// </summary>
    public void UpdateStudyItem(int itemId, string newName, bool isChecked)
    {
        if (_studyData?.StudyItemDataList == null) return;

        var item = _studyData.StudyItemDataList.FirstOrDefault(x => x.id == itemId);
        if (item != null)
        {
            item.name = newName ?? "";
            item.check = isChecked;
            _studyData.SaveData();
        }
    }
}