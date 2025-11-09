using System;
using System.Linq;
using System.Text;
using _01.Scripts.Manager;
using Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Common.Logger;

/// <summary>
/// 대시보드 UI를 관리하는 클래스
/// 학습 시간, AI 조언, 차트 등을 보여줍니다
/// </summary>
public class DashboardTabUI : BaseUI
{
    [Header("텍스트 UI 요소들")] 
    [SerializeField] private Text aiText; // AI 조언을 보여주는 텍스트
    [SerializeField] private TMP_Text totalTimeText; // 총 학습 시간을 보여주는 텍스트
    [SerializeField] private TMP_Text weeklyTotalTime; // 주간 총 학습 시간을 보여주는 텍스트
    [SerializeField] private TMP_Text subjectTime; // 과목별 학습 시간을 보여주는 텍스트
    
    [Header("차트 컴포넌트들")] 
    [SerializeField] private CustomPieChart pieChart; // 파이차트 컴포넌트
    [SerializeField] private CustomBarChart barChart; // 막대차트 컴포넌트
    [SerializeField] private CustomLineChart lineChart; // 꺾은선차트 컴포넌트
    
    [Header("빈 데이터일 때 보여줄 텍스트들")] 
    [SerializeField] private GameObject aiEmptyText; // AI 조언이 없을 때 보여줄 텍스트
    [SerializeField] private GameObject pieChartEmptyText; // 파이차트 데이터가 없을 때 보여줄 텍스트
    [SerializeField] private GameObject barChartEmptyText; // 막대차트 데이터가 없을 때 보여줄 텍스트
    [SerializeField] private GameObject lineChartEmptyText; // 꺾은선차트 데이터가 없을 때 보여줄 텍스트

    [Header("차트 내용 컨테이너들")] 
    [SerializeField] private GameObject pieChartContent; // 파이차트 실제 내용
    [SerializeField] private GameObject barChartContent; // 막대차트 실제 내용
    [SerializeField] private GameObject lineChartContent; // 꺾은선차트 실제 내용

    [Header("에러 핸들링")]
    [SerializeField] private ErrorHandler errorHandler; // 에러 핸들러 컴포넌트
    
    private AIAdviceManager aiAdviceManager; // AI 조언 관리자
    private ChartManager chartManager; // 차트 관리자
    private StatisticsTextManager statisticsTextManager; // 통계 텍스트 관리자
    
    /// <summary>
    /// UI가 열릴 때 호출되는 설정 함수
    /// </summary>
    protected override void OnSetting(BaseUIData data)
    {
        base.OnSetting(data);

        InitializeChartComponents(); // 차트 컴포넌트 초기화 시도
        InitializeManagers(); // 매니저 초기화
        RefreshAllData(); // 모든 데이터를 새로고침
    }

    /// <summary>
    /// 차트 컴포넌트들을 자동으로 찾아서 연결
    /// </summary>
    private void InitializeChartComponents()
    {
        ValidateChartComponent(pieChart, "PieChart"); // 각 차트 컴포넌트가 연결되었는지 확인
        ValidateChartComponent(barChart, "BarChart"); // 연결되지 않았으면 로그를 남김
        ValidateChartComponent(lineChart, "LineChart"); // (자동 연결 시도는 하지 않음)
    }

    private void ValidateChartComponent(MonoBehaviour chart, string chartName) // 차트 컴포넌트가 연결되었는지 확인하는 함수
    {
        if (chart == null) // 컴포넌트가 연결되지 않았으면 오류 로그를 남김
        {
            Logger.LogError($"{GetType()}::{chartName} 컴포넌트가 연결되지 않았습니다");
        }
    }
    
    // 매니저 초기화
    private void InitializeManagers()
    {
        // AI 조언 관리자 초기화
        aiAdviceManager = new AIAdviceManager(this, errorHandler);
        
        // 차트 관리자 초기화
        chartManager = new ChartManager(
            pieChart, pieChartContent, pieChartEmptyText,
            barChart, barChartContent, barChartEmptyText,
            lineChart, lineChartContent, lineChartEmptyText
        );
        
        // 통계 텍스트 관리자 초기화
        statisticsTextManager = new StatisticsTextManager(
            totalTimeText, weeklyTotalTime, subjectTime, errorHandler, RefreshAllData
        );
    }

    private void RefreshAllData()
    {
        try
        {
            errorHandler?.Hide(); // 에러 패널 숨기기
            
            statisticsTextManager?.RefreshAllStatistics(); // 통계 텍스트 새로고침
            chartManager?.UpdateAllCharts(); // 차트 새로고침
            RefreshAIAdvice(); // AI 조언 새로고침
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::RefreshAllData 오류: {e.Message}");
            errorHandler?.Show(ErrorType.DataError, RefreshAllData); // 에러 패널 표시 및 재시도 콜백 설정
        }
    }

    private void RefreshAIAdvice()
    {
        aiAdviceManager?.GetTodayAdvice(ShowAIState);
    }

    private void ShowAIState(string advice)
    {
        aiEmptyText?.SetActive(false); // AI 조언이 없을 때 보여줄 텍스트 숨기기
        
        if (aiText != null) // AI 조언 텍스트가 연결되어 있으면
        {
            aiText.gameObject.SetActive(true); // AI 조언 텍스트 표시
            aiText.text = advice; // AI 조언 텍스트 설정
        }
    }
    
    public void OnRefreshButtonClicked()
    {
        Logger.Log($"{GetType()}::데이터 새로고침 버튼 클릭됨");
        RefreshAllData(); // 모든 데이터 새로고침
    }
}