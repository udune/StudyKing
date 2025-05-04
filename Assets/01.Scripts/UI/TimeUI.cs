using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Logger = Common.Logger;

public class TimeUI : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    private StringBuilder sb = new StringBuilder();

    public void SetValue()
    {
        var userTimeData = UserDataManager.Instance.GetUserData<UserTimeData>();
        if (userTimeData == null)
        {
            Logger.Log("No User data found");
        }
        else
        {
            timeText.text = CalculateTimeFormat(userTimeData.Time);
        }
    }

    private string CalculateTimeFormat(long time)
    {
        sb.Clear();
        
        int hour = (int)(time / 3600);
        int minute = (int)((time % 3600) / 60);
        int second = (int)(time % 60);

        if (hour > 0) sb.Append(hour).Append("시간 ");
        if (minute > 0) sb.Append(minute).Append("분 ");
        if (second > 0) sb.Append(second).Append("초 ");

        return sb.Length > 0 ? sb.ToString().TrimEnd() : "0초";
    }
}
