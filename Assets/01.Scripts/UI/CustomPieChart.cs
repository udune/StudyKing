using UnityEngine;
using UnityEngine.UI;

public class CustomPieChart : BaseChart
{
    [Header("파이차트 전용 설정")] 
    [SerializeField] private float chartRadius = 100f; // 파이차트의 반지름
    [SerializeField] private bool showPercentage = true; // 퍼센트를 표시할지 여부
    [SerializeField] private float labelDistance = 130; // 라벨과 차트 중심 사이의 거리
    [SerializeField] private int circleSegments = 100; // 원을 구성하는 세그먼트 수
    
    // 파이차트를 실제로 그린다.
    protected override void DrawChart()
    {
        if (chartData.Count == 0)
        {
            return;
        }
        
        float totalValue = GetTotalValue();
        if (totalValue <= 0)
        {
            return;
        }

        float currentAngle = 0f;
        int colorIndex = 0;

        foreach (var data in chartData)
        {
            // 각 데이터의 비율을 계산한다.
            float percentage = data.Value / totalValue;
            float sliceAngle = percentage * 360f;
            
            Color sliceColor = GetGradientColor(colorIndex);
            
            // 파이 조각을 그린다.
            GameObject slice = CreatePieSlice(data.Key, currentAngle, sliceAngle, sliceColor);
            slice.transform.SetParent(chartContainer, false);
            
            // 라벨을 생성한다
            if (showLabels)
            {
                CreateSliceLabel(data.Key, data.Value, percentage, currentAngle + sliceAngle / 2);
            }

            currentAngle += sliceAngle;
            colorIndex++;
        }
        
        Debug.Log($"{GetType()}::파이차트 그리기 완료 - {chartData.Count}개 조각");
    }

    private GameObject CreatePieSlice(string label, float startAngle, float sliceAngle, Color color)
    {
        GameObject slice = new GameObject($"PieSlice_{label}");
        
        // Rect
        RectTransform rect = slice.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(chartRadius * 2, chartRadius * 2);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        
        // CanvasRenderer와 UI Graphic 컴포넌트 추가
        slice.AddComponent<CanvasRenderer>();
        PieSliceGraphic graphic = slice.AddComponent<PieSliceGraphic>();
        
        // 파이 조각 설정
        graphic.color = color;
        graphic.SetSliceData(startAngle, sliceAngle, chartRadius, circleSegments);

        return slice;
    }

    // 파이 조각에 대한 라벨을 생성한다.
    private void CreateSliceLabel(string label, float value, float percentage, float angle)
    {
        // 라벨 위치 계산
        float radian = angle * Mathf.Deg2Rad;
        Vector2 label_pos = new Vector2(
            Mathf.Sin(radian) * labelDistance,
            Mathf.Cos(radian) * labelDistance
        );

        string label_text = label;
        if (showPercentage)
        {
            label_text += $"{label}\n{percentage:P1}";
        }
        
        GameObject label_obj = CreateLabel(chartContainer, label_text, label_pos, new Vector2(80, 40));

        if (label_obj != null)
        {
            Text text_component = label_obj.GetComponent<Text>();
            if (text_component != null)
            {
                text_component.color = new Color(0.15f, 0.15f, 0.15f, 1f); // 다크 그레이
                text_component.fontSize = 14;
                text_component.fontStyle = FontStyle.Bold;
                text_component.alignment = TextAnchor.MiddleCenter;
                
                Outline outline = label_obj.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 1f, 1f, 0.8f);
                outline.effectDistance = new Vector2(1f, -1f);
            }
        }
    }
    
    // 그라디언트 색상 생성
    private Color GetGradientColor(int index)
    {
        // 모던한 그라디언트 색상 팔레트
        Color[] gradientColors = new Color[]
        {
            new Color(0.3f, 0.6f, 0.95f, 1f),   // 선명한 파랑
            new Color(1f, 0.5f, 0.3f, 1f),      // 선명한 주황
            new Color(0.4f, 0.85f, 0.5f, 1f),   // 선명한 녹색
            new Color(1f, 0.75f, 0.3f, 1f),     // 선명한 노랑
            new Color(0.85f, 0.4f, 0.75f, 1f),  // 선명한 분홍
            new Color(0.4f, 0.75f, 0.85f, 1f),  // 선명한 하늘색
            new Color(0.7f, 0.5f, 0.95f, 1f),   // 선명한 보라
        };
        
        return gradientColors[index % gradientColors.Length];
    }

    // 파이차트의 반지름을 설정
    public void SetRadius(float radius)
    {
        if (radius <= 0)
        {
            Debug.LogWarning($"{GetType()}::반지름은 0보다 커야 합니다.");
            return;
        }
        
        chartRadius = radius;
        RefreshChart();
    }

    public void SetShowPercentage(bool show)
    {
        showPercentage = show;
        RefreshChart();
    }
}

public class PieSliceGraphic : Graphic
{
    private float startAngle = 0f; // 시작 각도
    private float sliceAngle = 90f; // 조각 각도
    private float radius = 100f; // 반지름
    private int segments = 100; // 세그먼트

    // 파이 조각의 데이터를 설정한다
    public void SetSliceData(float start, float slice, float red, int seg)
    {
        startAngle = start;
        sliceAngle = slice;
        radius = red;
        segments = seg;
        SetVerticesDirty();
    }

    // 파이 조각의 메시를 생성한다.
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        // 기존 메시를 지운다.
        vh.Clear();

        // 각도가 0 이하면 그리지 않는다.
        if (sliceAngle <= 0)
        {
            return;
        }
        
        // 중심점을 추가한다.
        UIVertex centerVertex = UIVertex.simpleVert;
        centerVertex.position = Vector2.zero;
        centerVertex.color = color;
        vh.AddVert(centerVertex);
        
        // 파이 조각의 테두리 점들을 계산한다.
        int actualSegments = Mathf.Max(3, Mathf.RoundToInt(segments * sliceAngle / 360f));

        for (int i = 0; i <= actualSegments; i++)
        {
            // 현재 각도를 계산한다.
            float currentAngle = startAngle + (sliceAngle * i / actualSegments);
            float radian = currentAngle * Mathf.Deg2Rad;
            
            // 테두리 점의 위치를 계산한다.
            Vector3 position = new Vector3(
                Mathf.Sin(radian) * radius,
                Mathf.Cos(radian) * radius,
                0f);
            
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vh.AddVert(vertex);
        }

        for (int i = 0; i < actualSegments; i++)
        {
            vh.AddTriangle(0, i + 1, i + 2);
        }
    }
}