
using System;
using System.Collections;
using System.Collections.Generic;
using Gpm.Ui;
using UnityEngine;
using UnityEngine.UI;

public class HistoryData
{
    public string Date;
    public string Subjects;
} 

public class HistoryTabUI : BaseUI
{
    private List<string> cachedHistoryDates = new List<string>();
    private List<GameObject> historyItemList = new List<GameObject>();
    [SerializeField] GameObject itemPrefab;
    [SerializeField] Transform content;
    [SerializeField] GameObject emptyText;

    public override void ShowUI()
    {
        base.ShowUI();
#if !UNITY_EDITOR
        StartCoroutine(RefreshLayout());
#endif
    }

    public override void Setting(BaseUIData data)
    {
        base.Setting(data);
#if !UNITY_EDITOR
        Setting();
#endif
    }

    private void Setting()
    {
        var userHistoryData = UserDataManager.Instance.GetUserData<UserHistoryData>();
        if (userHistoryData == null)
        {
            emptyText.SetActive(true);
            return;
        }

        if (userHistoryData.HistoryItemDataList.Count.Equals(0))
        {
            emptyText.SetActive(true);
            return;
        }
        
        emptyText.SetActive(false);

        var newHistoryDates = new List<string>();
        foreach (var historyItemData in userHistoryData.HistoryItemDataList)
        {
            newHistoryDates.Add(historyItemData.Date);
        }

        if (newHistoryDates.Count.Equals(cachedHistoryDates.Count))
        {
            bool isSame = true;

            for (int i = 0; i < newHistoryDates.Count; i++)
            {
                if (!newHistoryDates[i].Equals(cachedHistoryDates[i]))
                {
                    isSame = false;
                    break;
                }
            }

            if (isSame)
            {
                return;
            }
        }
        
        cachedHistoryDates = newHistoryDates;

        foreach (var item in historyItemList)
        {
            Destroy(item);
        }
        historyItemList.Clear();
        
        foreach (var historyItemData in userHistoryData.HistoryItemDataList)
        {
            HistoryData data = new HistoryData();
            data.Date = DateTime.Parse(historyItemData.Date).ToString("yyyy년 M월 d일");
            data.Subjects = string.Join("\n", historyItemData.SubjectList);
                
            var go = Instantiate(itemPrefab, content);
            go.GetComponent<HistoryItemSlot>().UpdateData(data.Date, data.Subjects);
            historyItemList.Add(go);
        }
    }

    private IEnumerator RefreshLayout()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);
    }
}
