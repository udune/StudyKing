using Gpm.Ui;
using TMPro;
using UnityEngine;

public class HistoryItemSlot : MonoBehaviour
{
    public TMP_Text date;
    public TMP_Text subjects;

    public void UpdateData(string date, string subjects)
    {
        this.date.text = date;
        this.subjects.text = subjects;
    }
}
