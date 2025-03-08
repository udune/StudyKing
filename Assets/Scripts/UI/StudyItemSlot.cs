using System.Linq;
using Gpm.Ui;
using TMPro;
using Logger = Common.Logger;

public class StudyItemSlotData : InfiniteScrollData
{
    public int Id;
    public string Name;
}

public class StudyItemSlot : InfiniteScrollItem
{
    private StudyItemSlotData studyItemSlotData;

    public int id;
    public TMP_InputField name;

    public override void UpdateData(InfiniteScrollData data)
    {
        base.UpdateData(data);
        
        studyItemSlotData = data as StudyItemSlotData;
        if (studyItemSlotData == null)
        {
            Logger.Log($"{GetType()}::studyItemSlotData is invalid");
            return;
        }
        
        id = studyItemSlotData.Id;
        name.text = studyItemSlotData.Name;
        
        name.onEndEdit.AddListener(OnInputEnd);
    }

    public void OnInputEnd(string name)
    {
        Logger.Log($"{GetType()}::OnEndEdit(text={name})");
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
        
        data.Name = name;
        userStudyData.SaveData();
    }

    public void OnClickDelete()
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
        
        userStudyData.StudyItemDataList.Remove(data);
        userStudyData.SaveData();

        var studyUI = UIManager.Instance.GetActiveUI<StudyUI>() as StudyUI;
        if (studyUI == null)
        {
            Logger.Log($"{GetType()}::studyUI does not exist");
            return;
        }

        studyUI.Refresh();
    }
}
