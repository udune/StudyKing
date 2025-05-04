using UnityEngine;
using System.Collections;
using ChartAndGraph;
public class PieChartFeed : MonoBehaviour
{
    public Material[] materials;
    private ChartDynamicMaterial cdm = new ChartDynamicMaterial();
    
	void Start ()
    {
        PieChart pie = GetComponent<PieChart>();
        if (pie != null)
        {
            pie.DataSource.AddCategory("과학", cdm, 1, 1, 1);
            pie.DataSource.SetValue("과학", 5);
            pie.DataSource.SetMaterial("과학", materials[0]);
            pie.DataSource.AddCategory("수학", cdm, 1, 1, 1);
            pie.DataSource.SetValue("수학", 10);
            pie.DataSource.SetMaterial("수학", materials[1]);
        }
	}
}
