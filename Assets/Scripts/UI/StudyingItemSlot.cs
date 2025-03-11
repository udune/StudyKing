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

    private void Update()
    {
        if (LobbyManager.Instance.IsComplete)
        {
            check.isOn = true;
            check.interactable = false;
        }
    }

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
        
        check.onValueChanged.AddListener(OnClickCheck);
    }

    public void OnClickCheck(bool isChecked)
    {
        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData == null)
        {
            Logger.Log($"{GetType()}::UserStudyData does not exist");
            return;
        }
        
        var data = userStudyData.StudyItemDataList
            .Where(x => x.Id == id)
            .ToList()
            .FirstOrDefault();
        if (data == null)
        {
            Logger.Log($"{GetType()}::this data does not exist in StudyItemSlot");
            return;
        }
        
        data.Check = isChecked;
        userStudyData.SaveData();
    }
}
