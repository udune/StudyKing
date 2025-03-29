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
        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData != null)
        {
            if (userStudyData.StudyItemDataList.Count <= 0)
            {
                Logger.Log($"{GetType()}::UserStudyData does not exist");
                
                var data1 = new ModalUIData();
                data1.Type = ModalType.OK;
                data1.Desc = "최소한 한 가지 공부 계획은 있어야 해요.";
                data1.OkBtnText = "확인";
                UIManager.Instance.OpenUI<ModalUI>(data1);
                
                return;
            }
            
            var emptyData = userStudyData.StudyItemDataList.FirstOrDefault(data => string.IsNullOrEmpty(data.Name));
            if (emptyData != null)
            {
                Logger.Log($"{GetType()}::UserStudyData Name is empty");
                
                var data2 = new ModalUIData();
                data2.Type = ModalType.OK;
                data2.Desc = $"{emptyData.Id} 번째 공부 계획 작성해주세요.";
                data2.OkBtnText = "확인";
                UIManager.Instance.OpenUI<ModalUI>(data2);
                
                return;
            }
            
            Logger.Log($"{GetType()}::OnClickStart");
        
            var data = new BaseUIData();
            UIManager.Instance.OpenUI<StudyingUI>(data);
        }
        
        Logger.Log($"{GetType()}::userStudyData is null");
    }
}
