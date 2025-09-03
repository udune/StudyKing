using UnityEngine;
using UnityEngine.UI;

public class CustomBarChart : BaseChart
{
    [Header("바 차트 전용 설정")] 
    [SerializeField] private float barWidthRatio = 0.8f; // 막대 너비 비율
    [SerializeField] private float maxBarHeight = 200f; // 최대 막대 높이
    [SerializeField] private bool showValues = true; // 막대 위에 값을 표시할지
    [SerializeField] private bool showGrid = true; // 격자선을 표시할지
    [SerializeField] private int gridLineCount = 5; // 격자선 갯수
    [SerializeField] private Color gridColor = Color.gray; // 격자선 색상
    
    protected override void DrawChart()
    {
        if (chartData.Count == 0)
        {
            return;
        }

        float maxValue = GetMaxValue();
        if (maxValue <= 0)
        {
            return;
        }

        if (showGrid)
        {
            DrawGridLines(maxValue);
        }

        float chartWidth = chartContainer.rect.width;
        float barWidth = (chartWidth / chartData.Count) * barWidthRatio;
        float barSpacing = chartWidth / chartData.Count;

        int index = 0;
        foreach (var data in chartData)
        {
            // 막대 높이 계산
            float barHeight = (data.Value / maxValue) * maxBarHeight;
            
            // 막대 위치 계산
            float xPos = (index + 0.5f) * barSpacing - chartWidth / 2;
            float yPos = barHeight / 2 - maxBarHeight / 2;
            
            // 막대를 생성
            CreateBar(data.Key, data.Value, barWidth, barHeight, xPos, yPos, GetColor(index));
            transform.SetParent(chartContainer);

            index++;
        }
        
        Debug.Log($"{GetType()}::막대 차트 그리기 완료 - {chartData.Count}개 막대");
    }

    // 격자선을 그린다
    private void DrawGridLines(float maxValue)
    {
        if (gridLineCount <= 0)
        {
            return;
        }

        float chartWidth = chartContainer.rect.width;
        
        // 수평 격자선들을 그린다.
        for (int i = 0; i <= gridLineCount; i++)
        {
            float yPercent = (float)i / gridLineCount;
            float yPos = yPercent * maxBarHeight - maxBarHeight / 2;
            float gridValue = yPercent * maxValue;
            
            // 격자선 생성
            GameObject gridLine = CreateGridLine(chartWidth, yPos, gridValue);
            gridLine.transform.SetParent(chartContainer);
        }
    }

    // 격자선을 생성한다.
    private GameObject CreateGridLine(float width, float yPos, float value)
    {
        GameObject gridLine = new GameObject($"GridLine_{value:F1}");
        
        RectTransform rect = gridLine.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 1f);
        rect.anchoredPosition = new Vector2(0, yPos);
        
        Image image = gridLine.AddComponent<Image>();
        image.color = gridColor;

        if (showLabels && value > 0)
        {
            GameObject valueLabel = CreateLabel(gridLine.transform, value.ToString("F0"),
                new Vector2(-width / 2 - 30, 0), new Vector2(50, 20));

            if (valueLabel != null)
            {
                Text labelText = valueLabel.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.color = gridColor;
                    labelText.alignment = TextAnchor.MiddleCenter;
                    labelText.fontSize = 10;
                }
            }
        }
        
        return gridLine;
    }

    // 막대를 생성한다.
    private void CreateBar(string label, float value, float width, float height, float xPos, float yPos,
        Color color)
    {
        GameObject bar = new GameObject($"Bar_{label}");
        
        // 막대 몸체 설정
        RectTransform rect = bar.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(xPos, yPos);
        
        Image image = bar.AddComponent<Image>();
        image.color = color;

        if (showLabels)
        {
            GameObject bottomLabel = CreateLabel(bar.transform, label, new Vector2(0, -height / 2 - 20),
                new Vector2(width + 10, 20));

            if (bottomLabel != null)
            {
                Text labelText = bottomLabel.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.alignment = TextAnchor.MiddleCenter;
                    labelText.fontSize = labelFontSize;
                }
            }
            
            if (showValues && value > 0)
            {
                GameObject valueLabel = CreateLabel(bar.transform, value.ToString("F1"), new Vector2(0, height / 2 + 15),
                    new Vector2(width + 10, 20));

                if (valueLabel != null)
                {
                    Text labelText = valueLabel.GetComponent<Text>();
                    if (labelText != null)
                    {
                        labelText.alignment = TextAnchor.MiddleCenter;
                        labelText.fontSize = labelFontSize - 2;
                        labelText.color = Color.black;
                    }
                }
            }
        }
    }
    
    // 막대 너비 비율을 설정한다.
    public void SetBarWidthRatio(float ratio)
    {
        barWidthRatio = Mathf.Clamp01(ratio);
        RefreshChart();
    }

    // 최대 막대 높이를 설정한다.
    public void SetMaxBarHeight(float height)
    {
        if (height <= 0)
        {
            Debug.LogWarning($"{GetType()}::최대 막대 높이는 0보다 커야 합니다.");
            return;
        }
        
        maxBarHeight = height;
        RefreshChart();
    }

    // 막대 위에 값을 표시할지 설정한다.
    public void SetShowValues(bool show)
    {
        showValues = show;
        RefreshChart();
    }
    
    // 격자선을 표시할지 설정한다.
    public void SetShowGrid(bool show)
    {
        showGrid = show;
        RefreshChart();
    }
    
    // 격자선 갯수를 설정한다.
    public void SetGridLineCount(int count)
    {
        gridLineCount = Mathf.Max(0, count);
        RefreshChart();
    }
    
    // 격자선 색상을 설정한다.
    public void SetGridColor(Color color)
    {
        gridColor = color;
        RefreshChart();
    }
}
