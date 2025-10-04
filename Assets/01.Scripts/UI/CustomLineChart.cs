using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomLineChart : BaseChart
{
    [Header("라인 차트 전용 설정")] 
    [SerializeField] private float lineWidth = 3f; // 선 두께
    [SerializeField] private float pointSize = 10f; // 데이터 포인트 크기
    [SerializeField] private bool showPoints = true; // 데이터 포인트를 표시할지
    [SerializeField] private bool showArea = true; // 선을 표시할지
    [SerializeField] private bool showGrid = true; // 격자선을 표시할지
    [SerializeField] private Color lineColor = new Color(0.2f, 0.6f, 1f, 1f); // 선 색상
    [SerializeField] private Color pointColor = new Color(0.1f, 0.5f, 0.9f, 1f); // 데이터 포인트 색상
    [SerializeField] private Color areaColor = new Color(0.3f, 0.7f, 1f, 0.3f); // 영역 색상
    [SerializeField] private Color gridColor = new Color(0.7f, 0.7f, 0.7f, 0.5f); // 격자선 색상
    [SerializeField] private int gridLineCount = 5; // 격자선 갯수
    
    private List<Vector2> dataPoints = new List<Vector2>();
    
    protected override void DrawChart()
    {
        if (chartData.Count == 2)
        {
            Debug.LogWarning($"{GetType()}::데이터가 2개 미만입니다. 선을 그릴 수 없습니다.");
            return;
        }

        CalculateDataPoints();
        
        // 격자선을 먼저 그립니다
        if (showGrid)
        {
            DrawGridLines();
        }

        // 영역을 그립니다 (옵션이 켜져있는 경우)
        if (showArea)
        {
            DrawArea();
        }

        // 선을 그립니다
        DrawLines();

        // 데이터 포인트를 그립니다 (옵션이 켜져있는 경우)
        if (showPoints)
        {
            DrawPoints();
        }

        // 축 라벨을 그립니다
        if (showLabels)
        {
            DrawAxisLabels();
        }

        DrawAxisLabels();

        Debug.Log($"{GetType()}::선차트 그리기 완료 - {dataPoints.Count}개 포인트");
    }
    
    private void CalculateDataPoints()
    {
        dataPoints.Clear();

        float chartWidth = chartContainer.rect.width * 0.8f;  // 여백을 위해 80% 사용
        float chartHeight = chartContainer.rect.height * 0.8f;
        
        float maxValue = GetMaxValue();
        float minValue = GetMinValue();
        float valueRange = maxValue - minValue;

        if (valueRange <= 0)
        {
            valueRange = 1f;
        }

        var sortedKeys = chartData.Keys.OrderBy(x => x).ToList();

        for (int i = 0; i < sortedKeys.Count; i++)
        {
            float xPercent = sortedKeys.Count > 1 ? (float)i / (sortedKeys.Count - 1) : 0.5f;
            float yPercent = (chartData[sortedKeys[i]] - minValue) / valueRange;
            
            Vector2 position = new Vector2(
                (xPercent - 0.5f) * chartWidth,
                (yPercent - 0.5f) * chartHeight
            );
            
            dataPoints.Add(position);
        }
    }
    
    private void DrawGridLines()
    {
        float chartWidth = chartContainer.rect.width * 0.8f;
        float chartHeight = chartContainer.rect.height * 0.8f;
        
        // 수평 격자선 (Y축)
        for (int i = 0; i <= gridLineCount; i++)
        {
            float yPercent = (float)i / gridLineCount;
            float yPosition = (yPercent - 0.5f) * chartHeight;

            GameObject gridLine = CreateHorizontalGridLine(chartWidth, yPosition);
            gridLine.transform.SetParent(chartContainer, false);
        }

        // 수직 격자선 (X축) - 데이터 포인트 개수에 따라
        var sortedKeys = chartData.Keys.OrderBy(x => x).ToList();
        for (int i = 0; i < sortedKeys.Count; i++)
        {
            float xPercent = (float)i / (sortedKeys.Count - 1);
            float xPosition = (xPercent - 0.5f) * chartWidth;

            GameObject gridLine = CreateVerticalGridLine(chartHeight, xPosition, sortedKeys[i]);
            gridLine.transform.SetParent(chartContainer, false);
        }
    }

    /// <summary>
    /// 수평 격자선을 생성하는 함수
    /// </summary>
    private GameObject CreateHorizontalGridLine(float width, float yPos)
    {
        GameObject gridLine = new GameObject($"HGridLine_{yPos}");
        
        RectTransform rectTransform = gridLine.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(width, 2f);
        rectTransform.anchoredPosition = new Vector2(0, yPos);
        rectTransform.localScale = Vector3.one;

        Image image = gridLine.AddComponent<Image>();
        image.color = gridColor;

        return gridLine;
    }

    /// <summary>
    /// 수직 격자선을 생성하는 함수
    /// </summary>
    private GameObject CreateVerticalGridLine(float height, float xPos, string label)
    {
        GameObject gridLine = new GameObject($"VGridLine_{label}");
        
        RectTransform rectTransform = gridLine.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(2f, height);
        rectTransform.anchoredPosition = new Vector2(xPos, 0);
        rectTransform.localScale = Vector3.one;

        Image image = gridLine.AddComponent<Image>();
        image.color = gridColor;

        return gridLine;
    }

    /// <summary>
    /// 선 아래 영역을 채우는 함수
    /// </summary>
    private void DrawArea()
    {
        if (dataPoints.Count < 2)
        {
            return;
        }

        GameObject areaObj = new GameObject("LineChart_Area");
        areaObj.transform.SetParent(chartContainer, false);

        CanvasRenderer canvasRenderer = areaObj.AddComponent<CanvasRenderer>();
        LineAreaGraphic areaGraphic = areaObj.AddComponent<LineAreaGraphic>();
        
        // 영역 색상 설정 (투명도 적용)
        Color areaColorWithAlpha = areaColor;
        areaColorWithAlpha.a = 0.3f; // 30% 투명도
        
        areaGraphic.color = areaColorWithAlpha;
        areaGraphic.SetAreaData(dataPoints, chartContainer.rect.height * 0.8f);
    }

    /// <summary>
    /// 선들을 그리는 함수
    /// </summary>
    private void DrawLines()
    {
        for (int i = 0; i < dataPoints.Count - 1; i++)
        {
            GameObject line = CreateLine(dataPoints[i], dataPoints[i + 1]);
            line.transform.SetParent(chartContainer, false);
        }
    }

    /// <summary>
    /// 두 점 사이의 선을 생성하는 함수
    /// </summary>
    private GameObject CreateLine(Vector2 start, Vector2 end)
    {
        GameObject line = new GameObject($"Line_{start}_{end}");
        
        RectTransform rectTransform = line.AddComponent<RectTransform>();
        Image image = line.AddComponent<Image>();
        image.color = lineColor;

        // 선의 방향과 길이 계산
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        rectTransform.sizeDelta = new Vector2(distance, lineWidth);
        rectTransform.anchoredPosition = (start + end) / 2;
        rectTransform.rotation = Quaternion.FromToRotation(Vector2.right, direction);
        rectTransform.localScale = Vector3.one;

        return line;
    }

    /// <summary>
    /// 데이터 포인트들을 그리는 함수
    /// </summary>
    private void DrawPoints()
    {
        var sortedKeys = chartData.Keys.OrderBy(x => x).ToList();
        
        for (int i = 0; i < dataPoints.Count; i++)
        {
            GameObject point = CreatePoint(dataPoints[i], sortedKeys[i], chartData[sortedKeys[i]]);
            point.transform.SetParent(chartContainer, false);
        }
    }

    /// <summary>
    /// 데이터 포인트 하나를 생성하는 함수
    /// </summary>
    private GameObject CreatePoint(Vector2 position, string label, float value)
    {
        GameObject point = new GameObject($"Point_{label}");
        
        RectTransform rectTransform = point.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(pointSize, pointSize);
        rectTransform.anchoredPosition = position;
        rectTransform.localScale = Vector3.one;

        Image image = point.AddComponent<Image>();
        image.color = pointColor;
        
        // 원형 모양으로 만들기 위해 스프라이트 설정
        // 없으면 기본 사각형으로 표시됩니다

        // 포인트 위에 값 표시 (옵션)
        if (showLabels)
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
            
            GameObject valueLabel = CreateLabel(point.transform, valueText, 
                new Vector2(0, pointSize + 15), new Vector2(60, 25));
            
            if (valueLabel != null)
            {
                Text labelText = valueLabel.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.alignment = TextAnchor.MiddleCenter;
                    labelText.fontSize = 12;
                    labelText.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                    labelText.fontStyle = FontStyle.Bold;
                }
            }
        }

        return point;
    }

    /// <summary>
    /// 축 라벨을 그리는 함수
    /// </summary>
    private void DrawAxisLabels()
    {
        float chartWidth = chartContainer.rect.width * 0.8f;
        float chartHeight = chartContainer.rect.height * 0.8f;
        
        var sortedKeys = chartData.Keys.OrderBy(x => x).ToList();

        // X축 라벨들
        for (int i = 0; i < sortedKeys.Count; i++)
        {
            float xPercent = (float)i / (sortedKeys.Count - 1);
            float xPosition = (xPercent - 0.5f) * chartWidth;
            float yPosition = -chartHeight / 2 - 35;

            GameObject xLabel = CreateLabel(chartContainer, sortedKeys[i], 
                new Vector2(xPosition, yPosition), new Vector2(60, 20));
            
            if (xLabel != null)
            {
                Text labelText = xLabel.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.alignment = TextAnchor.MiddleCenter;
                    labelText.fontSize = 12;
                    labelText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                    labelText.fontStyle = FontStyle.Bold;
                }
            }
        }

        // Y축 라벨들
        float maxValue = GetMaxValue();
        float minValue = GetMinValue();
        
        for (int i = 0; i <= gridLineCount; i++)
        {
            float yPercent = (float)i / gridLineCount;
            float yPosition = (yPercent - 0.5f) * chartHeight;
            float value = minValue + (maxValue - minValue) * yPercent;

            GameObject yLabel = CreateLabel(chartContainer, value.ToString("F0"), 
                new Vector2(-chartWidth / 2 - 45, yPosition), new Vector2(70, 25));
            
            if (yLabel != null)
            {
                Text labelText = yLabel.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.alignment = TextAnchor.MiddleRight;
                    labelText.fontSize = 12;
                    labelText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                    labelText.fontStyle = FontStyle.Bold;
                }
            }
        }
    }

    /// <summary>
    /// 선의 색상을 설정하는 함수
    /// </summary>
    public void SetLineColor(Color color)
    {
        lineColor = color;
        RefreshChart();
    }

    /// <summary>
    /// 포인트 색상을 설정하는 함수
    /// </summary>
    public void SetPointColor(Color color)
    {
        pointColor = color;
        RefreshChart();
    }

    /// <summary>
    /// 선의 두께를 설정하는 함수
    /// </summary>
    public void SetLineWidth(float width)
    {
        lineWidth = Mathf.Max(1f, width);
        RefreshChart();
    }

    /// <summary>
    /// 포인트 크기를 설정하는 함수
    /// </summary>
    public void SetPointSize(float size)
    {
        pointSize = Mathf.Max(2f, size);
        RefreshChart();
    }

    /// <summary>
    /// 포인트 표시 여부를 설정하는 함수
    /// </summary>
    public void SetShowPoints(bool show)
    {
        showPoints = show;
        RefreshChart();
    }

    /// <summary>
    /// 영역 표시 여부를 설정하는 함수
    /// </summary>
    public void SetShowArea(bool show)
    {
        showArea = show;
        RefreshChart();
    }
}

public class LineAreaGraphic : Graphic
{
    private List<Vector2> points = new List<Vector2>();
    private float baselineY = 0f;
    private float chartHeight = 0f;

    /// <summary>
    /// 영역 데이터를 설정하는 함수
    /// </summary>
    public void SetAreaData(List<Vector2> areaPoints, float height)
    {
        points = new List<Vector2>(areaPoints);
        chartHeight = height;
        SetVerticesDirty();
    }

    /// <summary>
    /// 영역의 메시를 생성하는 함수
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points.Count < 2) return;

        // 영역을 채우기 위한 정점들 생성
        float bottomY = -chartHeight / 2;

        // 첫 번째 하단 점
        UIVertex vertex = UIVertex.simpleVert;
        vertex.position = new Vector3(points[0].x, bottomY, 0);
        vertex.color = color;
        vh.AddVert(vertex);

        // 데이터 포인트들 추가
        foreach (Vector2 point in points)
        {
            vertex.position = new Vector3(point.x, point.y, 0);
            vertex.color = color;
            vh.AddVert(vertex);
        }

        // 마지막 하단 점
        vertex.position = new Vector3(points[points.Count - 1].x, bottomY, 0);
        vertex.color = color;
        vh.AddVert(vertex);

        // 삼각형 생성
        int vertCount = vh.currentVertCount;
        for (int i = 0; i < vertCount - 2; i++)
        {
            vh.AddTriangle(0, i + 1, i + 2);
        }
    }
}