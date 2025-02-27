using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XCharts.Runtime;

public class ShowCustomGraphYT : MonoBehaviour, IDataPersistence
{

    [SerializeField] LineChart chart;
    [SerializeField] ButtonsHandllers buttonsHandllers;
    
    private List<float> forceTrail;
    private List<float> realTimeForceInput;
    private float previous_filtered_value = 0f;
    

    // Start is called before the first frame update
    void Start()
    {
        forceTrail = new List<float>();
        realTimeForceInput = new List<float>();
    }

    private void Awake()
    {
        InitChartWithTrail();
    }

    void InitChartWithTrail()
    {
        //var chart = gameObject.GetComponent<LineChart>();
        if (chart == null)
        {
            chart = gameObject.AddComponent<LineChart>();
            chart.Init();
            Debug.Log("Chart Initialised");
        }
        chart.EnsureChartComponent<Title>().show = true;
        chart.EnsureChartComponent<Title>().text = "Custom Trail";

        chart.EnsureChartComponent<Tooltip>().show = true;
        chart.EnsureChartComponent<Legend>().show = false;

        var xAxis = chart.EnsureChartComponent<XAxis>();
        var yAxis = chart.EnsureChartComponent<YAxis>();
        xAxis.show = true;
        yAxis.show = true;
        xAxis.type = Axis.AxisType.Value;
        //xAxis.type = Axis.AxisType.Time;
        xAxis.minMaxType = Axis.AxisMinMaxType.Custom;
        xAxis.min = 0;
        //xAxis.max = 1500;
        xAxis.max = 300;
        xAxis.interval = 30;  // 50

        //xAxis.type = Axis.AxisType.Category;
        yAxis.type = Axis.AxisType.Value;
        yAxis.minMaxType = Axis.AxisMinMaxType.Custom;
        yAxis.min = 0;
        yAxis.max = 30;
        yAxis.interval = yAxis.max / 5;

        xAxis.splitNumber = 300;  // 1500
        xAxis.boundaryGap = false;

        chart.RemoveData();
        var serie0 = chart.AddSerie<Line>("CustomTrail");
        var serie1 = chart.AddSerie<Line>("InputForce");
        //var serie1 = chart.GetSerie<Line>(1);
        serie0.symbol.show = false;
        serie1.symbol.show = false;
        for (int i = 0; i < xAxis.splitNumber; i++)
        {
            //chart.AddXAxisData(Time.time);
            //chart.AddXAxisData("x" + i);
            //chart.AddData("CustomTrail", sldataManager.dataFromCustomTrail[i]);
            chart.AddData("CustomTrail");
            //chart.AddData("RealTimeData", dataManager.randomData[i]);
        }
    }

    public void addRealTimeDataToGraph(float inputForce)
    {
        //var chart = gameObject.GetComponent<LineChart>();
        var serie1 = chart.GetSerie("InputForce");

        if ((this.realTimeForceInput.Count > 299 || serie1.dataCount > 299) && buttonsHandllers.activated())
        {
            // Save data at end point when save button is clicked
            buttonsHandllers.saveGraph();
 
        }


        if (this.realTimeForceInput.Count > 300)
        {
            realTimeForceInput.Clear();
        }
        else
        {
            realTimeForceInput.Add(inputForce);
        }

        if (serie1.dataCount > 300)
        {
            serie1.ClearData();
        }
        else
        {
            //chart.AddData("InputForce", realTimeForceInput.Last());
            chart.AddData("InputForce", inputForce);
            Debug.Log("added data: " + inputForce);
        }
    }

    public void addCustomTrailToGraph()
    {
        
        //var chart = gameObject.GetComponent<LineChart>();
        var serie0 = chart.GetSerie("CustomTrail");
        var serie1 = chart.GetSerie("InputForce");
        serie0.ClearData();
        serie1.ClearData();
        
        for (int i = 0; i < forceTrail.Count; i++)
        {
            chart.AddData("CustomTrail", forceTrail[i]);

        }
    }

    public void cleanRealTimeData()
    {
        realTimeForceInput.Clear();
    }

    public void cleanGraph()
    {
        //var chart = gameObject.GetComponent<LineChart>();
        var serie0 = chart.GetSerie("CustomTrail");
        var serie1 = chart.GetSerie("InputForce");
        serie0.ClearData();
        serie1.ClearData();
    }

    public void loadData(TrailData data)
    {
        this.forceTrail = data.forceTrail;
        Debug.Log("DataLoaded");
    }

    public void SaveData(ref TrailData data)
    {
        // save real time force trail to data.forceTrail
        data.forceTrail = this.realTimeForceInput;
    }


    public float smoothData(float newData, float alpha, float previous_filtered_value)
    {
        float filtered_value = alpha * newData + (1 - alpha) * previous_filtered_value;
        return filtered_value;
    }
}
