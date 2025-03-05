using System.Collections;
using System.Collections.Generic;
using Gpm.Ui;
using UnityEngine;

public class StudyUI : BaseUI
{
    public InfiniteScroll studyScrollList;
    
    public override void Setting(BaseUIData data)
    {
        studyScrollList.Clear();

        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData != null)
        {
            foreach (var itemData in userStudyData.StudyItemDataList)
            {
                var itemSlotData = new StudyItemSlotData();
                itemSlotData.Name = itemData.Name;
                studyScrollList.InsertData(itemSlotData);
            }
        }
    }

    public void OnClickAddStudyItem()
    {
        var itemSlotData = new StudyItemSlotData();
        itemSlotData.Name = "";
        studyScrollList.InsertData(itemSlotData);
    }
}
