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
    private StudyingItemSlotData studyingItemSlotData;

    public int id;
    public TMP_InputField name;
    public Toggle check;

    public override void UpdateData(InfiniteScrollData data)
    {
        base.UpdateData(data);
        
        studyingItemSlotData = data as StudyingItemSlotData;
        if (studyingItemSlotData == null)
        {
            Logger.Log($"{GetType()}::studyingItemSlotData is invalid");
            return;
        }
        
        id = studyingItemSlotData.Id;
        name.text = studyingItemSlotData.Name;
        check.isOn = studyingItemSlotData.Check;
        
        check.onValueChanged.RemoveAllListeners();
        check.onValueChanged.AddListener(OnClickCheck);

        LobbyManager.Instance.OnCompleteChanged -= UpdateCheckState;
        LobbyManager.Instance.OnCompleteChanged += UpdateCheckState;
        
        UpdateCheckState();
    }

    private void UpdateCheckState()
    {
        bool isComplete = LobbyManager.Instance.IsComplete;
        check.isOn = isComplete;
        check.interactable = !isComplete;
    }

    public void OnClickCheck(bool isChecked)
    {
        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData == null)
        {
            Logger.Log($"{GetType()}::UserStudyData does not exist");
            return;
        }
        
        var data = userStudyData.StudyItemDataList.FirstOrDefault(x => x.Id == id);
        if (data == null)
        {
            Logger.Log($"{GetType()}::this data does not exist in StudyItemSlot");
            return;
        }
        
        data.Check = isChecked;
        userStudyData.SaveData();
        LobbyManager.Instance.IsComplete = false;

        if (isChecked)
        {
            var studyingUI = UIManager.Instance.GetActiveUI<StudyingUI>() as StudyingUI;
            if (studyingUI == null)
            {
                Logger.Log($"{GetType()}::studyingUI is null");
                return;
            }
            
            var userSubjectTimeData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
            if (userSubjectTimeData == null)
            {
                Logger.Log($"{GetType()}::UserSubjectTimeData is null");
                return;
            }

            DateTime now = DateTime.UtcNow;
            TimeSpan elapsedTime = now - studyingUI.startTime;
            long elapsedSeconds = (long) elapsedTime.TotalSeconds;

            var subject = userSubjectTimeData.SubjectTimeItemDataList.FirstOrDefault(x => x.Name.Equals(data.Name));
            if (subject == null)
            {
                subject = new SubjectTimeItemData { Name = data.Name, Time = 0 };
                userSubjectTimeData.SubjectTimeItemDataList.Add(subject);
            }
            
            subject.Time += elapsedSeconds;
            userSubjectTimeData.SaveData();

            studyingUI.startTime = now;
            
            if (studyingUI.CheckCompleted())
            {
                LobbyManager.Instance.Pause();
                
                var modal = new ModalUIData();
                modal.Type = ModalType.OK_CANCEL;
                modal.Title = "정말 다 하셨어요?";
                modal.Desc = "공부 스케줄을 종료합니다.";
                modal.OkBtnText = "종료";
                modal.CancelBtnText = "취소";
                modal.OKAction = () =>
                {
                    data.Check = true;
                    userStudyData.SaveData();
                    
                    LobbyManager.Instance.IsComplete = true;
                };
                modal.CANCELAction = () =>
                {
                    LobbyManager.Instance.Resume();
                    check.isOn = false;
                };
            
                UIManager.Instance.OpenUI<ModalUI>(modal);
            }
        }
    }

    private void OnDestroy()
    {
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.OnCompleteChanged -= UpdateCheckState;
    }
}
