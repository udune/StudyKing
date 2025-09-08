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
        check.isOn = isComplete;
        check.interactable = !isComplete;
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

            data.check = isChecked;
            userStudyData.SaveData();

            var studyingUI = UIManager.Instance.GetActiveUI<StudyingUI>() as StudyingUI;
            if (studyingUI == null)
            {
                Logger.Log($"{GetType()}::studyingUI is null");
                return;
            }

            if (isChecked)
            {
                var userSubjectTimeData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
                if (userSubjectTimeData == null)
                {
                    Logger.Log($"{GetType()}::UserSubjectTimeData is null");
                    return;
                }

                var now = DateTime.UtcNow;
                var elapsedTime = now - studyingUI.StartTime;
                var elapsedSeconds = (long)elapsedTime.TotalSeconds;

                var subject = userSubjectTimeData.SubjectTimeItemDataList.FirstOrDefault(x => x.Name.Equals(data.name));
                if (subject == null)
                {
                    subject = new SubjectTimeItemData { Name = data.name, Time = 0 };
                    userSubjectTimeData.SubjectTimeItemDataList.Add(subject);
                }

                subject.Time += elapsedSeconds;
                userSubjectTimeData.SaveData();

                studyingUI.StartTime = now;
            }
            else
            {
                studyingUI.StartTime = DateTime.UtcNow;
            }

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