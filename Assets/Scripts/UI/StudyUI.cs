using System.Linq;
using Gpm.Ui;
using Logger = Common.Logger;

public class StudyUI : BaseUI
{
    public InfiniteScroll studyScrollList;
    
    public override void Setting(BaseUIData data)
    {
        base.Setting(data);
        
        Setting();
    }

    private void Setting()
    {
        studyScrollList.Clear();

        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData != null)
        {
            foreach (var itemData in userStudyData.StudyItemDataList)
            {
                var itemSlotData = new StudyItemSlotData();
                itemSlotData.Id = itemData.Id;
                itemSlotData.Name = itemData.Name;
                studyScrollList.InsertData(itemSlotData);
            }
        }
    }

    public void OnClickAddStudyItem()
    {
        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData == null)
        {
            Logger.Log($"{GetType()}::UserStudyData does not exist");
            return;
        }

        userStudyData.StudyItemDataList.Add(new StudyItemData(userStudyData.StudyItemDataList.Count + 1, "", false));
        userStudyData.SaveData();
        
        var itemSlotData = new StudyItemSlotData();
        itemSlotData.Id = userStudyData.StudyItemDataList.Count;
        itemSlotData.Name = "";
        studyScrollList.InsertData(itemSlotData);
    }

    public void Refresh()
    {
        Setting();
    }

    public void OnClickStart()
    {
        Logger.Log($"{GetType()}::OnClickStart");
        
        var data = new BaseUIData();
        UIManager.Instance.OpenUI<StudyingUI>(data);
    }
}
