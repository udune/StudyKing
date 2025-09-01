using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomLineChart : BaseChart
{
    [Header("라인 차트 전용 설정")] 
    [SerializeField] private float lineWidth = 2f; // 선 두께
    [SerializeField] private float pointSize = 6f; // 데이터 포인트 크기
    [SerializeField] private bool showPoints = true; // 데이터 포인트를 표시할지
    [SerializeField] private bool showArea; // 선을 표시할지
    [SerializeField] private Color lineColor = Color.blue; // 선 색상
    [SerializeField] private Color pointColor = Color.red; // 데이터 포인트 색상
    [SerializeField] private Color areaColor = Color.blue; // 영역 색상
    [SerializeField] private bool showGrid = true; // 격자선을 표시할지
    [SerializeField] private int gridLineCount = 5; // 격자선 갯수
    [SerializeField] private Color gridColor = Color.gray; // 격자선 색상
    
    private List<Vector2> dataPoints = new List<Vector2>();
    
    protected override void DrawChart()
    {
        if (chartData.Count == 0)
        {
            return;
        }
        
        PrepareDataPoints();
        
        if (dataPoints.Count < 2) return; // 점이 2개 미만이면 선을 그릴 수 없습니다

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

        Debug.Log($"{GetType()}::선차트 그리기 완료 - {dataPoints.Count}개 포인트");
    }
    
    private void PrepareDataPoints()
    {
        dataPoints.Clear();

        if (chartData.Count == 0) return;

        float chartWidth = chartContainer.rect.width * 0.8f;  // 여백을 위해 80% 사용
        float chartHeight = chartContainer.rect.height * 0.8f;

        float maxValue = GetMaxValue();
        float minValue = GetMinValue();
        float valueRange = maxValue - minValue;

        if (valueRange == 0) valueRange = 1; // 0으로 나누기 방지

        // 데이터를 키 순서대로 정렬 (시간 순서)
        var sortedData = chartData.OrderBy(x => x.Key).ToList();

        for (int i = 0; i < sortedData.Count; i++)
        {
            // X 좌표: 균등하게 분배
            float xPercent = (float)i / (sortedData.Count - 1);
            float x = (xPercent - 0.5f) * chartWidth;

            // Y 좌표: 값에 따라 계산
            float yPercent = (sortedData[i].Value - minValue) / valueRange;
            float y = (yPercent - 0.5f) * chartHeight;

            dataPoints.Add(new Vector2(x, y));
        }
    }

    private void DrawGridLines()
    {
        if (gridLineCount <= 0) return;

        float chartWidth = chartContainer.rect.width * 0.8f;
        float chartHeight = chartContainer.rect.height * 0.8f;
        float maxValue = GetMaxValue();
        float minValue = GetMinValue();

        // 수평 격자선 (Y축)
        for (int i = 0; i <= gridLineCount; i++)
        {
            float yPercent = (float)i / gridLineCount;
            float yPosition = (yPercent - 0.5f) * chartHeight;
            float gridValue = minValue + (maxValue - minValue) * yPercent;

            GameObject gridLine = CreateHorizontalGridLine(chartWidth, yPosition, gridValue);
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
    private GameObject CreateHorizontalGridLine(float width, float yPos, float value)
    {
        GameObject gridLine = new GameObject($"HGridLine_{value:F1}");
        
        RectTransform rectTransform = gridLine.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(width, 1f);
        rectTransform.anchoredPosition = new Vector2(0, yPos);

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
        rectTransform.sizeDelta = new Vector2(1f, height);
        rectTransform.anchoredPosition = new Vector2(xPos, 0);

        Image image = gridLine.AddComponent<Image>();
        image.color = gridColor;

        return gridLine;
    }

    /// <summary>
    /// 선 아래 영역을 채우는 함수
    /// </summary>
    private void DrawArea()
    {
        if (dataPoints.Count < 2) return;

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

        Image image = point.AddComponent<Image>();
        image.color = pointColor;
        
        // 원형 모양으로 만들기 위해 스프라이트 설정
        // 없으면 기본 사각형으로 표시됩니다

        // 포인트 위에 값 표시 (옵션)
        if (showLabels)
        {
            GameObject valueLabel = CreateLabel(point.transform, value.ToString("F1"), 
                new Vector2(0, pointSize + 10), new Vector2(50, 20));
            
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
            float yPosition = -chartHeight / 2 - 25;

            GameObject xLabel = CreateLabel(chartContainer, sortedKeys[i], 
                new Vector2(xPosition, yPosition), new Vector2(60, 20));
            
            if (xLabel != null)
            {
                Text labelText = xLabel.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.alignment = TextAnchor.MiddleCenter;
                    labelText.fontSize = labelFontSize - 2;
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
                new Vector2(-chartWidth / 2 - 35, yPosition), new Vector2(60, 20));
            
            if (yLabel != null)
            {
                Text labelText = yLabel.GetComponent<Text>();
                if (labelText != null)
                {
                    labelText.alignment = TextAnchor.MiddleRight;
                    labelText.fontSize = labelFontSize - 2;
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

    /// <summary>
    /// 영역 데이터를 설정하는 함수
    /// </summary>
    public void SetAreaData(List<Vector2> areaPoints, float chartHeight)
    {
        points.Clear();
        points.AddRange(areaPoints);
        baselineY = -chartHeight / 2; // 차트 하단을 기준선으로 설정
        SetVerticesDirty();
    }

    /// <summary>
    /// 영역의 메시를 생성하는 함수
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points.Count < 2) return;

        // 영역을 삼각형들로 분할해서 채웁니다
        for (int i = 0; i < points.Count - 1; i++)
        {
            // 4개의 꼭짓점으로 사각형을 만들고 두 개의 삼각형으로 분할
            Vector3 topLeft = new Vector3(points[i].x, points[i].y, 0f);
            Vector3 topRight = new Vector3(points[i + 1].x, points[i + 1].y, 0f);
            Vector3 bottomLeft = new Vector3(points[i].x, baselineY, 0f);
            Vector3 bottomRight = new Vector3(points[i + 1].x, baselineY, 0f);

            // 버텍스 추가
            int vertexIndex = vh.currentVertCount;
            
            UIVertex vertex1 = UIVertex.simpleVert;
            vertex1.position = bottomLeft;
            vertex1.color = color;
            vh.AddVert(vertex1);

            UIVertex vertex2 = UIVertex.simpleVert;
            vertex2.position = bottomRight;
            vertex2.color = color;
            vh.AddVert(vertex2);

            UIVertex vertex3 = UIVertex.simpleVert;
            vertex3.position = topRight;
            vertex3.color = color;
            vh.AddVert(vertex3);

            UIVertex vertex4 = UIVertex.simpleVert;
            vertex4.position = topLeft;
            vertex4.color = color;
            vh.AddVert(vertex4);

            // 두 개의 삼각형 생성
            vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
            vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex);
        }
    }
}