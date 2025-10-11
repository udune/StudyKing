using System;
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
            var data = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>(); // 사용자 과목 시간 데이터
            bool hasData = data?.SubjectTimeItemDataList?.Count > 0; // 데이터 존재 여부
            
            pieChartContent?.SetActive(hasData); // 데이터가 있으면 콘텐츠 표시
            pieChartEmptyText?.SetActive(!hasData); // 데이터가 없으면 빈 텍스트 표시
            
            if (hasData && pieChart != null) // 데이터가 있는 경우에만 업데이트
            {
                pieChart.ClearData(); // 기존 데이터 초기화
                
                foreach (var item in data.SubjectTimeItemDataList) // 시간 단위로 변환
                {
                    if (item.Time > 0) // 시간이 0보다 큰 경우에만 추가
                    {
                        pieChart.AddData(item.Name, item.Time / 3600f); // 시간 단위로 변환
                    }
                }
                
                pieChart.RefreshChart(); // 차트 새로고침
            }
            else
            {
                pieChart?.ClearData(); // 데이터가 없으면 차트 초기화
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::파이 차트 업데이트 중 오류 발생 - {e.Message}");
        }
    }
    
    private void UpdateBarChart()
    {
        try
        {
            var data = UserDataManager.Instance?.GetUserData<UserDailyTimeData>(); // 사용자 일일 시간 데이터
            bool hasData = data?.DailyTimeItemDataList?.Count > 0; // 데이터 존재 여부
            
            barChartContent?.SetActive(hasData); // 데이터가 있으면 콘텐츠 표시
            barChartEmptyText?.SetActive(!hasData); // 데이터가 없으면 빈 텍스트 표시
            
            if (hasData && barChart != null) // 데이터가 있는 경우에만 업데이트
            {
                barChart.ClearData(); // 기존 데이터 초기화
                DateTime now = DateTime.UtcNow.AddHours(9); // 한국 시간 기준 현재 날짜
                
                for (int i = 6; i >= 0; i--) // 최근 7일치 데이터 확인
                {
                    DateTime date = now.AddDays(-i); // i일 전 날짜
                    var dayData = data.DailyTimeItemDataList.FirstOrDefault(x => x.Date == date.ToString("yyyy-MM-dd")); // 해당 날짜 데이터 찾기
                    barChart.AddData(date.ToString("MM/dd"), dayData?.Time / 3600f ?? 0); // 데이터가 없으면 0으로 추가
                }
                
                barChart.RefreshChart(); // 차트 새로고침
            }
            else
            {
                barChart?.ClearData(); // 데이터가 없으면 차트 초기화
            }
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
            var data = UserDataManager.Instance?.GetUserData<UserDailyTimeData>(); // 사용자 일일 시간 데이터
            bool hasData = data?.DailyTimeItemDataList?.Count > 0; // 데이터 존재 여부
            
            lineChartContent?.SetActive(hasData); // 데이터가 있으면 콘텐츠 표시
            lineChartEmptyText?.SetActive(!hasData); // 데이터가 없으면 빈
            
            if (hasData && lineChart != null) // 데이터가 있는 경우에만 업데이트
            {
                lineChart.ClearData(); // 기존 데이터 초기화
                DateTime now = DateTime.UtcNow.AddHours(9); // 한국 시간 기준 현재 날짜
                
                for (int i = 29; i >= 0; i--) // 최근 30일치 데이터 확인
                {
                    DateTime date = now.AddDays(-i); // i일 전 날짜
                    var dayData = data.DailyTimeItemDataList.FirstOrDefault(x => x.Date == date.ToString("yyyy-MM-dd")); // 해당 날짜 데이터 찾기
                    lineChart.AddData(date.ToString("MM/dd"), dayData?.Time / 3600f ?? 0); // 데이터가 없으면 0으로 추가
                }
                
                lineChart.RefreshChart(); // 차트 새로고침
            }
            else
            {
                lineChart?.ClearData(); // 데이터가 없으면 차트 초기화
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::라인 차트 업데이트 중 오류 발생 - {e.Message}");
        }
    }
}
