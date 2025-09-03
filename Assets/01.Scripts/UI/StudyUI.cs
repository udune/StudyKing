using System.Linq;
using Gpm.Ui;
using UnityEngine;
using Logger = Common.Logger;

/// <summary>
/// 공부 계획을 관리하는 UI 클래스
/// 사용자가 공부할 항목들을 추가, 수정, 삭제할 수 있습니다
/// </summary>
public class StudyUI : BaseUI
{
    [Header("스크롤 리스트")]
    [SerializeField] private InfiniteScroll studyScrollList; // 공부 항목들을 보여주는 무한 스크롤 리스트
    
    [Header("UI 버튼들")]
    [SerializeField] private GameObject addButton;    // 항목 추가 버튼
    [SerializeField] private GameObject startButton;  // 공부 시작 버튼
    
    // 현재 사용자의 공부 데이터
    private UserStudyData _currentUserStudyData;

    /// <summary>
    /// UI 초기화 시 호출되는 함수
    /// </summary>
    protected override void OnInit()
    {
        base.OnInit();
        
        // 필수 컴포넌트 확인
        ValidateComponents();
    }
    
    /// <summary>
    /// 필수 컴포넌트들이 제대로 연결되었는지 확인하는 함수
    /// </summary>
    private void ValidateComponents()
    {
        if (studyScrollList == null)
        {
            Logger.LogError($"{GetType()}::studyScrollList가 연결되지 않았습니다");
        }
        
        if (addButton == null)
        {
            Logger.LogWarning($"{GetType()}::addButton이 연결되지 않았습니다");
        }
        
        if (startButton == null)
        {
            Logger.LogWarning($"{GetType()}::startButton이 연결되지 않았습니다");
        }
    }

    /// <summary>
    /// UI 설정 시 호출되는 함수
    /// </summary>
    protected override void OnSetting(BaseUIData data)
    {
        base.OnSetting(data);
        
        // 공부 리스트를 새로고침합니다
        RefreshStudyList();
    }

    /// <summary>
    /// 공부 리스트를 새로고침하는 함수
    /// 사용자 데이터에서 공부 항목들을 가져와서 화면에 표시합니다
    /// </summary>
    private void RefreshStudyList()
    {
        Logger.Log($"{GetType()}::공부 리스트를 새로고침합니다");
        
        try
        {
            // 기존 리스트를 지웁니다
            if (studyScrollList != null)
            {
                studyScrollList.Clear();
            }

            // 사용자 공부 데이터를 가져옵니다
            _currentUserStudyData = UserDataManager.Instance?.GetUserData<UserStudyData>();
            
            if (_currentUserStudyData == null)
            {
                Logger.LogWarning($"{GetType()}::UserStudyData가 없습니다. 새로 생성합니다");
                CreateNewStudyData();
                return;
            }

            // 공부 항목들을 리스트에 추가합니다
            AddStudyItemsToList();
            
            Logger.Log($"{GetType()}::공부 리스트 새로고침 완료 - 총 {_currentUserStudyData.StudyItemDataList.Count}개 항목");
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::공부 리스트 새로고침 중 오류 발생: {e.Message}");
        }
    }
    
    /// <summary>
    /// 새로운 공부 데이터를 생성하는 함수
    /// </summary>
    private void CreateNewStudyData()
    {
        try
        {
            _currentUserStudyData = new UserStudyData
            {
                StudyItemDataList = new System.Collections.Generic.List<StudyItemData>()
            };

            // 기본 공부 항목 하나 추가
            var defaultItem = new StudyItemData 
            { 
                id = 1, 
                name = "", 
                check = false 
            };
            _currentUserStudyData.StudyItemDataList.Add(defaultItem);
            
            // 데이터 저장
            _currentUserStudyData.SaveData();
            
            // 리스트에 추가
            AddStudyItemsToList();
            
            Logger.Log($"{GetType()}::새로운 공부 데이터를 생성했습니다");
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::새로운 공부 데이터 생성 중 오류 발생: {e.Message}");
        }
    }
    
    /// <summary>
    /// 공부 항목들을 리스트에 추가하는 함수
    /// </summary>
    private void AddStudyItemsToList()
    {
        if (studyScrollList == null || _currentUserStudyData?.StudyItemDataList == null)
        {
            return;
        }
        
        foreach (var studyItem in _currentUserStudyData.StudyItemDataList)
        {
            // 스크롤 리스트용 데이터로 변환
            var itemSlotData = new StudyItemSlotData
            {
                Id = studyItem.id,
                Name = studyItem.name ?? "", // null 방지
                Check = studyItem.check
            };
            
            studyScrollList.InsertData(itemSlotData);
        }
    }

    /// <summary>
    /// 공부 항목 추가 버튼 클릭 시 호출되는 함수
    /// </summary>
    public void OnClickAddStudyItem()
    {
        Logger.Log($"{GetType()}::공부 항목 추가 버튼이 클릭되었습니다");
        
        try
        {
            // 사용자 데이터 확인
            if (_currentUserStudyData == null)
            {
                Logger.LogError($"{GetType()}::UserStudyData가 없어서 항목을 추가할 수 없습니다");
                ShowErrorModal("데이터 오류", "공부 데이터가 없습니다. 다시 시도해주세요.");
                return;
            }

            // 새로운 공부 항목 생성
            var newStudyItem = CreateNewStudyItem();
            
            // 데이터에 추가
            _currentUserStudyData.StudyItemDataList.Add(newStudyItem);
            
            // 데이터 저장
            _currentUserStudyData.SaveData();
            
            // UI에 추가
            AddStudyItemToUI(newStudyItem);
            
            Logger.Log($"{GetType()}::새로운 공부 항목을 추가했습니다 (ID: {newStudyItem.id})");
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::공부 항목 추가 중 오류 발생: {e.Message}");
            ShowErrorModal("오류", "공부 항목을 추가하는데 실패했습니다.");
        }
    }
    
    /// <summary>
    /// 새로운 공부 항목을 생성하는 함수
    /// </summary>
    private StudyItemData CreateNewStudyItem()
    {
        int newId = _currentUserStudyData.StudyItemDataList.Count > 0 
            ? _currentUserStudyData.StudyItemDataList.Max(item => item.id) + 1 
            : 1;
            
        return new StudyItemData 
        { 
            id = newId, 
            name = "", 
            check = false 
        };
    }
    
    /// <summary>
    /// UI에 공부 항목을 추가하는 함수
    /// </summary>
    private void AddStudyItemToUI(StudyItemData studyItem)
    {
        if (studyScrollList == null) return;
        
        var itemSlotData = new StudyItemSlotData
        {
            Id = studyItem.id,
            Name = studyItem.name ?? "",
            Check = studyItem.check
        };
        
        studyScrollList.InsertData(itemSlotData);
    }

    /// <summary>
    /// 외부에서 리스트를 새로고침할 때 호출하는 함수
    /// StudyItemSlot에서 데이터가 변경되었을 때 사용됩니다
    /// </summary>
    public void RefreshFromExternal()
    {
        Logger.Log($"{GetType()}::외부에서 리스트 새로고침 요청");
        RefreshStudyList();
    }

    /// <summary>
    /// 공부 시작 버튼 클릭 시 호출되는 함수
    /// </summary>
    public void OnClickStartStudy()
    {
        Logger.Log($"{GetType()}::공부 시작 버튼이 클릭되었습니다");
        
        try
        {
            // 데이터 유효성 검사
            if (!ValidateStudyData())
            {
                return; // 검사 실패 시 함수 내에서 모달 표시
            }
            
            // 공부 시작
            StartStudySession();
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::공부 시작 중 오류 발생: {e.Message}");
            ShowErrorModal("오류", "공부를 시작하는데 실패했습니다.");
        }
    }
    
    /// <summary>
    /// 공부 데이터의 유효성을 검사하는 함수
    /// </summary>
    private bool ValidateStudyData()
    {
        // 데이터 존재 확인
        if (_currentUserStudyData?.StudyItemDataList == null)
        {
            Logger.LogWarning($"{GetType()}::공부 데이터가 없습니다");
            ShowInfoModal("알림", "공부 데이터가 없습니다. 항목을 추가해주세요.");
            return false;
        }
        
        // 최소 하나의 항목 확인
        if (_currentUserStudyData.StudyItemDataList.Count == 0)
        {
            Logger.LogWarning($"{GetType()}::공부 항목이 없습니다");
            ShowInfoModal("알림", "최소한 한 가지 공부 계획은 있어야 해요.");
            return false;
        }
        
        // 빈 항목 확인
        var emptyItem = _currentUserStudyData.StudyItemDataList.FirstOrDefault(item => string.IsNullOrEmpty(item.name?.Trim()));
        if (emptyItem != null)
        {
            Logger.LogWarning($"{GetType()}::빈 공부 항목이 있습니다 (ID: {emptyItem.id})");
            ShowInfoModal("알림", $"{emptyItem.id}번째 공부 계획을 작성해주세요.");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 공부 세션을 시작하는 함수
    /// </summary>
    private void StartStudySession()
    {
        Logger.Log($"{GetType()}::공부 세션을 시작합니다");
        
        // 공부 UI로 전환
        var studyingUIData = new BaseUIData();
        UIManager.Instance?.OpenUI<StudyingUI>(studyingUIData);
        
        // 현재 UI 닫기
        CloseUI();
    }
    
    /// <summary>
    /// 정보 모달을 표시하는 함수
    /// </summary>
    private void ShowInfoModal(string title, string message)
    {
        try
        {
            var modalData = new ModalUIData
            {
                Type = ModalType.OK,
                Title = title,
                Desc = message,
                OkBtnText = "확인"
            };
            
            UIManager.Instance?.OpenUI<ModalUI>(modalData);
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::모달 표시 중 오류 발생: {e.Message}");
        }
    }
    
    /// <summary>
    /// 에러 모달을 표시하는 함수
    /// </summary>
    private void ShowErrorModal(string title, string message)
    {
        try
        {
            var modalData = new ModalUIData
            {
                Type = ModalType.OK,
                Title = title,
                Desc = message,
                OkBtnText = "확인"
            };
            
            UIManager.Instance?.OpenUI<ModalUI>(modalData);
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::에러 모달 표시 중 오류 발생: {e.Message}");
        }
    }
    
    /// <summary>
    /// 특정 공부 항목을 삭제하는 함수
    /// StudyItemSlot에서 호출할 수 있습니다
    /// </summary>
    /// <param name="itemId">삭제할 항목의 ID</param>
    public void DeleteStudyItem(int itemId)
    {
        Logger.Log($"{GetType()}::공부 항목 삭제 요청 (ID: {itemId})");
        
        try
        {
            if (_currentUserStudyData?.StudyItemDataList == null)
            {
                Logger.LogWarning($"{GetType()}::삭제할 데이터가 없습니다");
                return;
            }
            
            // 항목 찾기 및 삭제
            var itemToRemove = _currentUserStudyData.StudyItemDataList.FirstOrDefault(item => item.id == itemId);
            if (itemToRemove != null)
            {
                _currentUserStudyData.StudyItemDataList.Remove(itemToRemove);
                _currentUserStudyData.SaveData();
                
                // UI 새로고침
                RefreshStudyList();
                
                Logger.Log($"{GetType()}::공부 항목 삭제 완료 (ID: {itemId})");
            }
            else
            {
                Logger.LogWarning($"{GetType()}::삭제할 항목을 찾을 수 없습니다 (ID: {itemId})");
            }
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::공부 항목 삭제 중 오류 발생: {e.Message}");
        }
    }
    
    /// <summary>
    /// 공부 항목의 내용을 업데이트하는 함수
    /// StudyItemSlot에서 호출할 수 있습니다
    /// </summary>
    /// <param name="itemId">업데이트할 항목의 ID</param>
    /// <param name="newName">새로운 이름</param>
    /// <param name="isChecked">체크 상태</param>
    public void UpdateStudyItem(int itemId, string newName, bool isChecked)
    {
        Logger.Log($"{GetType()}::공부 항목 업데이트 요청 (ID: {itemId}, Name: {newName}, Checked: {isChecked})");
        
        try
        {
            if (_currentUserStudyData?.StudyItemDataList == null)
            {
                Logger.LogWarning($"{GetType()}::업데이트할 데이터가 없습니다");
                return;
            }
            
            // 항목 찾기 및 업데이트
            var itemToUpdate = _currentUserStudyData.StudyItemDataList.FirstOrDefault(item => item.id == itemId);
            if (itemToUpdate != null)
            {
                itemToUpdate.name = newName ?? "";
                itemToUpdate.check = isChecked;
                
                _currentUserStudyData.SaveData();
                Logger.Log($"{GetType()}::공부 항목 업데이트 완료 (ID: {itemId})");
            }
            else
            {
                Logger.LogWarning($"{GetType()}::업데이트할 항목을 찾을 수 없습니다 (ID: {itemId})");
            }
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::공부 항목 업데이트 중 오류 발생: {e.Message}");
        }
    }
    
    /// <summary>
    /// UI가 닫힐 때 호출되는 함수
    /// </summary>
    protected override void OnClose()
    {
        base.OnClose();
        
        // 데이터 정리
        _currentUserStudyData = null;
        
        Logger.Log($"{GetType()}::StudyUI가 정리되었습니다");
    }
}