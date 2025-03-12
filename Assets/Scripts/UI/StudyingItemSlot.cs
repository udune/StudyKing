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
        var studyingUI = UIManager.Instance.GetActiveUI<StudyingUI>() as StudyingUI;
        studyingUI?.CheckCompleted();
        userStudyData.SaveData();
    }

    private void OnDestroy()
    {
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.OnCompleteChanged -= UpdateCheckState;
    }
}
