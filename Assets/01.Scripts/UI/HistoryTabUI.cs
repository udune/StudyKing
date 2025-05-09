
using System;
using Gpm.Ui;

public class HistoryTabUI : BaseUI
{
    public InfiniteScroll historyScrollList;

    public override void Setting(BaseUIData data)
    {
        base.Setting(data);

#if !UNITY_EDITOR
        Setting();
#endif
    }

    private void Setting()
    {
        historyScrollList.Clear();

        var userHistoryData = UserDataManager.Instance.GetUserData<UserHistoryData>();
        if (userHistoryData != null)
        {
            foreach (var itemData in userHistoryData.HistoryItemDataList)
            {
                var itemSlotData = new HistoryItemSlotData();
                itemSlotData.Date = DateTime.Parse(itemData.Date).ToString("yyyy년 M월 d일");
                itemSlotData.Subjects = string.Join("\n", itemData.SubjectList);
                historyScrollList.InsertData(itemSlotData);
            }
        }
    }
}
