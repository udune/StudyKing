using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseChart : MonoBehaviour
{
    [Header("차트 기본 설정")]
    [SerializeField] protected RectTransform chartContainer; // 차트가 그려질 컨테이너
    [SerializeField] protected Color[] chartColors; // 차트에 사용할 색상 배열
    [SerializeField] protected bool showLabels = true; // 라벨을 보여줄지 여부
    [SerializeField] protected Font font; // 라벨에 사용할 폰트
    [SerializeField] protected int labelFontSize = 12; // 라벨 폰트 크기

    protected Dictionary<string, float> chartData = new Dictionary<string, float>();

    protected readonly Color[] defaultColors =
    {
        Color.red, Color.blue, Color.green, Color.yellow,
        Color.cyan, Color.magenta, Color.white, Color.gray,
    };

    private void Start()
    {
        // 차트 컨테이너가 없으면 자동으로 찾기
        if (chartContainer == null)
        {
            chartContainer = GetComponent<RectTransform>();
        }

        // 색상 배열이 비어있으면 기본 색상을 사용한다
        if (chartColors == null || chartColors.Length == 0)
        {
            chartColors = defaultColors;
        }
    }

    // 차트에 데이터를 추가한다.
    public virtual void AddData(string label, float value)
    {
        if (string.IsNullOrEmpty(label))
        {
            Debug.LogWarning($"{GetType()}::라벨이 비어있습니다");
            return;
        }

        chartData[label] = value;
        Debug.Log($"{GetType()}::데이터 추가 - {label}: {value}");
    }

    // 여러 개의 데이터를 한 번에 설정한다.
    public virtual void SetData(Dictionary<string, float> data)
    {
        if (data == null)
        {
            Debug.LogWarning($"{GetType()}::데이터가 없습니다.");
            return;
        }
        
        chartData.Clear();
        foreach (var item in data)
        {
            chartData[item.Key] = item.Value;
        }
        
        Debug.Log($"{GetType()}::전체 데이터 설정 완료 - {data.Count}개");
    }

    // 차트 데이터를 모두 지운다.
    public virtual void ClearData()
    {
        chartData.Clear();
        ClearChart();
        Debug.Log($"{GetType()}::데이터를 모두 지웠습니다.");
    }

    // 차트를 지운다.
    protected virtual void ClearChart()
    {
        if (chartContainer == null)
        {
            return;
        }

        // 차트 컨테이너의 모든 자식 오브젝트를 제거한다.
        for (int i = chartContainer.childCount - 1; i >= 0; i--)
        {
            GameObject child = chartContainer.GetChild(i).gameObject;

            if (Application.isPlaying)
            {
                // 런타임
                Destroy(child);
            }
            else
            {
                // 에디터
                DestroyImmediate(child);
            }
        }
    }

    // 차트를 다시 그린다.
    public virtual void RefreshChart()
    {
        ClearChart();

        if (chartData.Count == 0)
        {
            Debug.Log($"{GetType()}::데이터가 없어서 차트를 그리지 않습니다.");
            return;
        }

        DrawChart();
        Debug.Log($"{GetType()}::차트를 새로 그렸습니다.");
    }
    
    // 각 차트에서 구현
    protected abstract void DrawChart();

    // 지정된 인덱스에 해당하는 색상을 반환한다.
    protected Color GetColor(int index)
    {
        if (chartColors == null || chartColors.Length == 0)
        {
            return defaultColors[index % defaultColors.Length];
        }
        
        return chartColors[index % defaultColors.Length];
    }

    protected GameObject CreateLabel(Transform parent, string text, Vector2 position, Vector2 size)
    {
        if (!showLabels || string.IsNullOrEmpty(text))
        {
            return null;
        }
        
        GameObject label_obj = new GameObject($"Label_{text}");
        label_obj.transform.SetParent(parent, false);
        
        RectTransform label_rect = label_obj.AddComponent<RectTransform>();
        label_rect.sizeDelta = size;
        label_rect.anchoredPosition = position;
        label_rect.localScale = Vector3.one;
        
        Text label_text = label_obj.AddComponent<Text>();
        label_text.text = text;
        label_text.fontSize = labelFontSize;
        label_text.color = Color.black;
        label_text.alignment = TextAnchor.MiddleCenter;

        // 폰트가 있다면 사용, 없으면 기본
        if (font != null)
        {
            label_text.font = font;
        }
        else
        {
            label_text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        
        return label_obj;
    }

    // 현재 차트의 데이터 개수를 반환하다.
    public int GetDataCount()
    {
        return chartData.Count;
    }

    // 현재 차트의 모든 데이터 값의 합을 반환한다.
    public float GetTotalValue()
    {
        float total = 0;
        foreach (var value in chartData.Values)
        {
            total += value;
        }

        return total;
    }

    // 차트 데이터 중 최대값 반환한다.
    public float GetMaxValue()
    {
        float max = 0;
        foreach (var value in chartData.Values)
        {
            if (value > max)
            {
                max = value;
            }
        }
        
        return max;
    }

    // 차트 데이터 중 최소값을 반환한다.
    public float GetMinValue()
    {
        if (chartData.Count == 0)
        {
            return 0;
        }
        
        float min = float.MaxValue;
        foreach (var value in chartData.Values)
        {
            if (value < min)
            {
                min = value;
            }
        }

        return min;
    }

    // 게임 오브젝트 파괴 시 수행
    protected virtual void OnDestroy()
    {
        ClearChart();
        Debug.Log($"{GetType()}::차트가 정리되었습니다.");
    }
}
