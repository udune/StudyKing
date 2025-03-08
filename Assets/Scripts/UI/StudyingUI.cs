using Gpm.Ui;

public class StudyingUI : BaseUI
{
    public InfiniteScroll studyingScrollList;

    public override void Setting(BaseUIData data)
    {
        base.Setting(data);
        
        studyingScrollList.Clear();

        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData != null)
        {
            foreach (var itemData in userStudyData.StudyItemDataList)
            {
                var itemSlotData = new StudyingItemSlotData();
                itemSlotData.Id = itemData.Id;
                itemSlotData.Name = itemData.Name;
                itemSlotData.Check = itemData.Check;
                studyingScrollList.InsertData(itemSlotData);
            }
        }
    }

    public void OnClickFinishStudyItem()
    {
        
    }
}
