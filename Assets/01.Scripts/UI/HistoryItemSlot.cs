using Gpm.Ui;
using TMPro;
using Logger = Common.Logger;

public class HistoryItemSlotData : InfiniteScrollData
{
    public string Date;
    public string Subjects;
}

public class HistoryItemSlot : InfiniteScrollItem
{
    private HistoryItemSlotData historyItemSlotData;

    public TMP_Text date;
    public TMP_Text subjects;

    public override void UpdateData(InfiniteScrollData data)
    {
        base.UpdateData(data);
        
        historyItemSlotData = data as HistoryItemSlotData;
        if (historyItemSlotData == null)
        {
            Logger.Log($"{GetType()}::historyItemSlotData is invalid");
            return;
        }
        
        date.text = historyItemSlotData.Date;
        subjects.text = historyItemSlotData.Subjects;
    }
}
