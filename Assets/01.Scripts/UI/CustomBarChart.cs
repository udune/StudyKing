using UnityEngine;
using UnityEngine.UI;

public class CustomBarChart : BaseChart
{
    [Header("바 차트 전용 설정")] 
    [SerializeField] private float barWidthRatio = 0.6f; // 막대 너비 비율
    [SerializeField] private float maxBarHeight = 200f; // 최대 막대 높이
    [SerializeField] private bool showValues = true; // 막대 위에 값을 표시할지
    [SerializeField] private bool showGrid = true; // 격자선을 표시할지
    [SerializeField] private int gridLineCount = 5; // 격자선 갯수
    [SerializeField] private Color gridColor = new Color(0.8f, 0.8f, 0.8f, 0.5f); // 격자선 색상
    
    protected override void DrawChart()
    {
        if (chartData.Count == 0)
        {
            Debug.Log($"{GetType()}::데이터가 없습니다.");
            return;
        }

        float maxValue = GetMaxValue();
        if (maxValue <= 0)
        {
            Debug.LogWarning($"{GetType()}::최대 값이 0 이하입니다.");
            return;
        }

        if (showGrid)
        {
            DrawGridLines(maxValue);
        }

        float chartWidth = chartContainer.rect.width;
        float barWidth = (chartWidth / chartData.Count) * barWidthRatio;
        float barSpacing = chartWidth / chartData.Count;
        
        Debug.Log($"{GetType()}::차트 너비: {chartWidth}, 막대 너비: {barWidth}, 막대 간격: {barSpacing}");

        int index = 0;
        foreach (var data in chartData)
        {
            float barHeight = (data.Value / maxValue) * maxBarHeight;
            float xPos = (index + 0.5f) * barSpacing - chartWidth / 2;
            float yPos = barHeight / 2 - maxBarHeight / 2;
            
            Color barColor = GetGradientColor(index);
            
            Debug.Log($"{GetType()}::막대 #{index} - 과목: {data.Key}, 값: {data.Value}, width: {barWidth}, height: {barHeight}, xPos: {xPos}, yPos: {yPos}");
            
            CreateBar(data.Key, data.Value, barWidth, barHeight, xPos, yPos, barColor);

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
            gridLine.transform.SetParent(chartContainer, false);
        }
    }

    // 격자선을 생성한다.
    private GameObject CreateGridLine(float width, float yPos, float value)
    {
        GameObject gridLine = new GameObject($"GridLine_{value:F1}");
        
        RectTransform rect = gridLine.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 2f);
        rect.anchoredPosition = new Vector2(0, yPos);
        rect.localScale = Vector3.one;
        
        Image image = gridLine.AddComponent<Image>();
        image.color = gridColor;

        if (showLabels && value > 0)
        {
            GameObject valueLabel = CreateLabel(gridLine.transform, value.ToString("F0"),
                new Vector2(-width / 2 - 40, 0), new Vector2(60, 25));

            if (valueLabel != null)
            {
                Text labelText = valueLabel.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                    labelText.alignment = TextAnchor.MiddleRight;
                    labelText.fontSize = 12;
                    labelText.fontStyle = FontStyle.Normal;
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
        
        bar.transform.SetParent(chartContainer, false);
        
        // 막대 몸체 설정
        RectTransform rect = bar.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(xPos, yPos);
        rect.localScale = Vector3.one;
        
        Image image = bar.AddComponent<Image>();
        image.color = color;
        
        Debug.Log($"{GetType()}::막대 생성 - {label}: 높이={height}, 위치=({xPos}, {yPos}), 색상={color}");

        if (showLabels)
        {
            GameObject bottomLabel = CreateLabel(bar.transform, label, new Vector2(0, -height / 2 - 30),
                new Vector2(width + 20, 30));

            if (bottomLabel != null)
            {
                Text labelText = bottomLabel.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.alignment = TextAnchor.MiddleCenter;
                    labelText.fontSize = 14;
                    labelText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                    labelText.fontStyle = FontStyle.Bold;
                }
            }
            
            if (showValues && value > 0)
            {
                string valueText;
                if (value >= 1.0f)
                {
                    valueText = value.ToString("F1") + "h";
                }
                else
                {
                    int minutes = Mathf.RoundToInt(value * 60);
                    valueText = minutes + "m";
                }
                
                GameObject valueLabel = CreateLabel(bar.transform, valueText, new Vector2(0, height / 2 + 20),
                    new Vector2(width + 20, 25));

                if (valueLabel != null)
                {
                    Text labelText = valueLabel.GetComponent<Text>();
                    if (labelText != null)
                    {
                        labelText.alignment = TextAnchor.MiddleCenter;
                        labelText.fontSize = 13;
                        labelText.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                        labelText.fontStyle = FontStyle.Bold;
                    }
                }
            }
        }
    }

    private Color GetGradientColor(int index)
    {
        // 모던한 그라디언트 색상 팔레트
        Color[] gradientColors = new Color[]
        {
            new Color(0.4f, 0.7f, 1f, 1f),      // 밝은 파랑
            new Color(1f, 0.6f, 0.4f, 1f),      // 밝은 주황
            new Color(0.6f, 0.9f, 0.6f, 1f),    // 밝은 녹색
            new Color(1f, 0.8f, 0.4f, 1f),      // 밝은 노랑
            new Color(0.9f, 0.5f, 0.8f, 1f),    // 밝은 분홍
            new Color(0.5f, 0.8f, 0.9f, 1f),    // 밝은 하늘색
            new Color(0.8f, 0.6f, 1f, 1f),      // 밝은 보라
        };
        
        return gradientColors[index % gradientColors.Length];
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
