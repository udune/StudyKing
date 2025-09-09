using System;
using System.Linq;
using Gpm.Ui;
using TMPro;
using UnityEngine.UI;
using Logger = Common.Logger;

public class StudyingItemSlotData : InfiniteScrollData
{
    public int Id;
    public string Name;
    public bool Check;
}

public class StudyingItemSlot : InfiniteScrollItem
{
    private StudyingItemSlotData _studyingItemSlotData;

    public int id;
    public TMP_InputField nameInput;
    public Toggle check;

    public override void UpdateData(InfiniteScrollData data)
    {
        base.UpdateData(data);

        _studyingItemSlotData = data as StudyingItemSlotData;
        if (_studyingItemSlotData == null)
        {
            Logger.Log($"{GetType()}::studyingItemSlotData is invalid");
            return;
        }

        id = _studyingItemSlotData.Id;
        nameInput.text = _studyingItemSlotData.Name;
        check.isOn = _studyingItemSlotData.Check;

        check.onValueChanged.RemoveAllListeners();
        check.onValueChanged.AddListener(OnClickCheck);

        LobbyManager.Instance.OnCompleteChanged -= UpdateCheckState;
        LobbyManager.Instance.OnCompleteChanged += UpdateCheckState;

        UpdateCheckState();
    }

    private void UpdateCheckState()
    {
        var isComplete = LobbyManager.Instance.IsComplete;
        
        // 전체 완료 상태일 때만 체크박스 비활성화, 개별 체크 상태는 실제 데이터에서 가져옴
        if (isComplete)
        {
            check.interactable = false;
        }
        else
        {
            check.interactable = true;
            // 실제 사용자 데이터에서 체크 상태 가져오기
            var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
            if (userStudyData != null)
            {
                var data = userStudyData.StudyItemDataList.Find(x => x.id == id);
                if (data != null)
                {
                    check.isOn = data.check;
                }
            }
        }
    }

    private void OnClickCheck(bool isChecked)
    {
        try
        {
            var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
            if (userStudyData == null)
            {
                Logger.Log($"{GetType()}::UserStudyData does not exist");
                return;
            }

            var data = userStudyData.StudyItemDataList.FirstOrDefault(x => x.id == id);
            if (data == null)
            {
                Logger.Log($"{GetType()}::this data does not exist in StudyItemSlot");
                return;
            }

            // 체크 상태 변경
            data.check = isChecked;
            userStudyData.SaveData();

            var studyingUI = UIManager.Instance.GetActiveUI<StudyingUI>() as StudyingUI;
            if (studyingUI == null)
            {
                Logger.Log($"{GetType()}::studyingUI is null");
                return;
            }

            // 과목별 시간 데이터 확인 및 생성
            var userSubjectTimeData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
            if (userSubjectTimeData != null)
            {
                var subject = userSubjectTimeData.SubjectTimeItemDataList.FirstOrDefault(x => x.Name.Equals(data.name));
                if (subject == null)
                {
                    subject = new SubjectTimeItemData { Name = data.name, Time = 0 };
                    userSubjectTimeData.SubjectTimeItemDataList.Add(subject);
                    userSubjectTimeData.SaveData();
                    Logger.Log($"{GetType()}::새 과목 생성 - '{data.name}': 0초");
                }
            }

            Logger.Log($"{GetType()}::OnClickCheck - 과목: '{data.name}', 체크상태: {isChecked}");
            
            // 활성 과목 업데이트 (새로운 로직의 핵심)
            studyingUI.UpdateCurrentActiveSubject();

            // 이벤트 기반으로 완료 상태 체크 및 UI 업데이트
            studyingUI.OnStudyItemCheckChanged();
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::OnClickCheck error: {e.Message}");
            var model = new ModalUIData
            {
                Type = ModalType.Ok,
                Title = "오류",
                Desc = "체크 처리 중 오류가 발생했습니다.",
                OkBtnText = "확인"
            };
            UIManager.Instance.OpenUI<ModalUI>(model);
        }
    }

    private void OnDestroy()
    {
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.OnCompleteChanged -= UpdateCheckState;
    }
}