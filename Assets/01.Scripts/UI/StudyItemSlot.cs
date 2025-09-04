using System.Linq;
using Gpm.Ui;
using TMPro;
using Logger = Common.Logger;

public class StudyItemSlotData : InfiniteScrollData
{
    public int Id;
    public string Name;
    public bool Check;
}

public class StudyItemSlot : InfiniteScrollItem
{
    private StudyItemSlotData _studyItemSlotData;

    public int id;
    public TMP_InputField nameInput;

    public override void UpdateData(InfiniteScrollData data)
    {
        base.UpdateData(data);
        
        _studyItemSlotData = data as StudyItemSlotData;
        if (_studyItemSlotData == null)
        {
            Logger.Log($"{GetType()}::studyItemSlotData is invalid");
            return;
        }
        
        id = _studyItemSlotData.Id;
        nameInput.text = _studyItemSlotData.Name;
        
        // 기존 리스너 제거 후 새로 등록
        nameInput.onEndEdit.RemoveAllListeners();
        nameInput.onEndEdit.AddListener(OnInputEnd);
    }

    private void OnInputEnd(string nameStr)
    {
        Logger.Log($"{GetType()}::OnEndEdit(text={nameStr})");
        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData == null)
        {
            Logger.Log($"{GetType()}::UserStudyData does not exist");
            return;
        }

        var data = userStudyData.StudyItemDataList
            .Where(x => x.id == id)
            .ToList()
            .FirstOrDefault();
        if (data == null)
        {
            Logger.Log($"{GetType()}::this data does not exist in StudyItemSlot");
            return;
        }
        
        data.name = nameStr;
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
            .Where(x => x.id == id)
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

        studyUI.RefreshStudyList();
    }
}
