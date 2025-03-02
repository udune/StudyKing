using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Logger = Common.Logger;

public class TimeUI : MonoBehaviour
{
    public TextMeshProUGUI timeText;

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
        int hour = (int) time / 60;
        int minute = (int) time % 60;

        if (hour > 0 && minute > 0)
        {
            return $"{hour}시간 {minute}분";
        }

        if (hour > 0)
            return $"{hour}시간";
        
        return $"{minute}분";
    }
}
