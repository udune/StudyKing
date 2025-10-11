using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Common.Logger;

// 차트 관리자 클래스
public class ChartManager
{
    private readonly CustomPieChart pieChart; // 파이 차트 컴포넌트
    private readonly CustomBarChart barChart; // 바 차트 컴포넌트
    private readonly CustomLineChart lineChart; // 라인 차트 컴포넌트

    private readonly GameObject pieChartContent; // 파이 차트 콘텐츠 오브젝트
    private readonly GameObject pieChartEmptyText; // 파이 차트 빈 텍스트 오브젝트
    private readonly GameObject barChartContent; // 바 차트 콘텐츠 오브젝트
    private readonly GameObject barChartEmptyText; // 바 차트 빈 텍스트 오브젝트
    private readonly GameObject lineChartContent; // 라인 차트 콘텐츠 오브젝트
    private readonly GameObject lineChartEmptyText; // 라인 차트 빈 텍스트 오브젝트

    // 생성자
    public ChartManager(
        CustomPieChart pieChart, GameObject pieContent, GameObject pieEmpty,
        CustomBarChart barChart, GameObject barContent, GameObject barEmpty,
        CustomLineChart lineChart, GameObject lineContent, GameObject lineEmpty)
    {
        this.pieChart = pieChart;
        this.pieChartContent = pieContent;
        this.pieChartEmptyText = pieEmpty;

        this.barChart = barChart;
        this.barChartContent = barContent;
        this.barChartEmptyText = barEmpty;

        this.lineChart = lineChart;
        this.lineChartContent = lineContent;
        this.lineChartEmptyText = lineEmpty;
    }

    public void UpdateAllCharts() // 모든 차트 업데이트
    {
        UpdatePieChart();
        UpdateBarChart();
        UpdateLineChart();
    }

    private void UpdatePieChart() // 파이 차트 업데이트
    {
        try
        {
            if (pieChart == null) // 파이 차트 컴포넌트가 없으면 종료
            {
                return;
            }
            
            var data = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>(); // 사용자 과목 시간 데이터
            if (data == null) // 데이터가 없으면 종료
            {
                return;
            }
            
            var chartData = new Dictionary<string, float>(); // 차트 데이터 초기화
            foreach (var item in data.SubjectTimeItemDataList) // 데이터 매핑
            {
                chartData[item.Name] = item.Time; // 시간 단위로 변환
            }

            if (chartData.Count == 0) // 데이터가 없으면 빈 텍스트 표시
            {
                ShowEmpty(pieChartEmptyText, pieChartContent); // 빈 텍스트 표시
                return;
            }
            
            ShowChart(pieChartEmptyText, pieChartContent); // 차트 표시
            pieChart.SetData(chartData); // 차트 데이터 설정
            pieChart.RefreshChart(); // 차트 새로고침
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::파이 차트 업데이트 중 오류 발생 - {e.Message}");
        }
    }
    
    // 바 차트 업데이트
    private void UpdateBarChart()
    {
        try
        {
            if (barChart == null) // 바 차트 컴포넌트가 없으면 종료
            {
                return;
            }
            
            var data = UserDataManager.Instance?.GetUserData<UserDailyTimeData>(); // 사용자 일일 시간 데이터
            if (data == null) // 데이터가 없으면 종료
            {
                return;
            }
            
            var chartData = GetWeeklyData(data); // 최근 7일 데이터 가져오기
            if (chartData.Count == 0) // 데이터가 없으면 빈 텍스트 표시
            {
                ShowEmpty(barChartEmptyText, barChartContent); // 빈 텍스트 표시
                return;
            }
            
            ShowChart(barChartEmptyText, barChartContent); // 차트 표시
            barChart.SetData(chartData); // 차트 데이터 설정
            barChart.RefreshChart(); // 차트 새로고침
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::바 차트 업데이트 중 오류 발생 - {e.Message}");
        }
    }
    
    private void UpdateLineChart() // 라인 차트 업데이트
    {
        try
        {
            if (lineChart == null) // 라인 차트 컴포넌트가 없으면 종료
            {
                return;
            }
            
            var data = UserDataManager.Instance?.GetUserData<UserDailyTimeData>(); // 사용자 일일 시간 데이터
            if (data == null) // 데이터가 없으면 종료
            {
                return;
            }
            
            var chartData = GetMonthlyData(data); // 최근 30일 데이터 가져오기
            if (chartData.Count == 0) // 데이터가 없으면 빈 텍스트 표시
            {
                ShowEmpty(lineChartEmptyText, lineChartContent); // 빈 텍스트 표시
                return;
            }
            
            ShowChart(lineChartEmptyText, lineChartContent); // 차트 표시
            lineChart.SetData(chartData); // 차트 데이터 설정
            lineChart.RefreshChart(); // 차트 새로고침
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::라인 차트 업데이트 중 오류 발생 - {e.Message}");
        }
    }

    // 최근 7일간의 데이터를 "M/d" 형식의 라벨과 시간(시간 단위)으로 매핑
    private Dictionary<string, float> GetWeeklyData(UserDailyTimeData data)
    {
        var result = new Dictionary<string, float>(); // 결과 딕셔너리 초기화
        DateTime now = DateTime.UtcNow.AddHours(9); // 한국 시간 기준 현재 날짜

        for (int i = 6; i >= 0; i--) // 최근 7일치 데이터 확인
        {
            DateTime date = now.AddDays(-i); // i일 전 날짜
            string dateStr = date.ToString("yyyy-MM-dd"); // "yyyy-MM-dd" 형식의 날짜 문자열
            string label = date.ToString("M/d"); // "M/d" 형식으로 라벨 생성
            
            var dayData = data.DailyTimeItemDataList.FirstOrDefault(x => x.Date == dateStr); // 해당 날짜 데이터 찾기
            result[label] = dayData?.Time ?? 0; // 시간 단위로 변환
        }

        return result; // 결과 반환
    }
    
    private Dictionary<string, float> GetMonthlyData(UserDailyTimeData data)
    {
        var result = new Dictionary<string, float>(); // 결과 딕셔너리 초기화
        DateTime now = DateTime.UtcNow.AddHours(9); // 한국 시간 기준 현재 날짜

        for (int i = 29; i >= 0; i--) // 최근 30일치 데이터 확인
        {
            DateTime date = now.AddDays(-i); // i일 전 날짜
            string dateStr = date.ToString("yyyy-MM-dd"); // "yyyy-MM-dd" 형식의 날짜 문자열
            string label = date.ToString("M/d"); // "M/d" 형식으로 라벨 생성
            
            var dayData = data.DailyTimeItemDataList.FirstOrDefault(x => x.Date == dateStr); // 해당 날짜 데이터 찾기
            result[label] = dayData?.Time ?? 0; // 시간 단위로 변환
        }

        return result; // 결과 반환
    }

    // 빈 텍스트 표시
    private void ShowEmpty(GameObject emptyText, GameObject content)
    {
        emptyText?.SetActive(true); // 빈 텍스트 표시
        content?.SetActive(false); // 차트 숨기기
    }
    
    // 차트 표시
    private void ShowChart(GameObject emptyText, GameObject content)
    {
        emptyText?.SetActive(false); // 빈 텍스트 숨기기
        content?.SetActive(true); // 차트 표시
    }
}
