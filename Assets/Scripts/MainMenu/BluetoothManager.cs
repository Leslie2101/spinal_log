using System.Collections;
using System.Collections.Generic;
using UnityEngine.Android;
using UnityEngine;
using UnityEngine.UI;
using System;
using JetBrains.Annotations;
using System.Linq;
using TMPro;

public class BluetoothManager : MonoBehaviour
{
    public Text deviceAdd;
    public Text dataToSend;
    public Text receivedData;
    public GameObject devicesListContainer;
    public GameObject deviceMACText;
    private bool isConnected;
    private string devicename = "spinal log";
    private System.Random random;
    

    // stiffness status send back to SpinalLog Device, 0 = default, 1 = hard
    private bool isStiff;
    private bool stiffOn = false;

    //private IEnumerator stiffnessCoroutine;

    public string inputdata;

    private static AndroidJavaClass unity3dbluetoothplugin;
    private static AndroidJavaObject BluetoothConnector;

    public bool IsStiff { get => isStiff; set => isStiff = value; }

    //public event EventHandler<BluetoothDataEventArgs> BluetoothDataReceived;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        //GenerateRandomData();
    }

    // Start is called before the first frame update   
    void Start()
    {
        InitBluetooth();
        isConnected = false;
        IsStiff = false;
        stiffOn = false;

        // if (stiffnessCoroutine != null)
        // {
        //     StopCoroutine(stiffnessCoroutine);
        // }
        // stiffnessCoroutine = sCoroutine(5.0f);
        // StartCoroutine(stiffnessCoroutine);
    }

    // creating an instance of the bluetooth class from the plugin 
    public void InitBluetooth()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        // Check BT and location permissions
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation)
            || !Permission.HasUserAuthorizedPermission(Permission.FineLocation)
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADMIN")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADVERTISE")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
        {

            Permission.RequestUserPermissions(new string[] {
                        Permission.CoarseLocation,
                            Permission.FineLocation,
                            "android.permission.BLUETOOTH_ADMIN",
                            "android.permission.BLUETOOTH",
                            "android.permission.BLUETOOTH_SCAN",
                            "android.permission.BLUETOOTH_ADVERTISE",
                             "android.permission.BLUETOOTH_CONNECT"
                    });

        }

        unity3dbluetoothplugin = new AndroidJavaClass("com.example.unity3dbluetoothplugin.BluetoothConnector");
        BluetoothConnector = unity3dbluetoothplugin.CallStatic<AndroidJavaObject>("getInstance");
    }

    // Start device scan
    public void StartScanDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        // Destroy devicesListContainer child objects for new scan display
        foreach (Transform child in devicesListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        BluetoothConnector.CallStatic("StartScanDevices");
    }

    // Stop device scan
    public void StopScanDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnector.CallStatic("StopScanDevices");
    }

    // This function will be called by Java class to update the scan status,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ScanStatus(string status)
    {
        Toast("Scan Status: " + status);
    }

    // This function will be called by Java class whenever a new device is found,
    // and delivers the new devices as a string data="MAC+NAME"
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void NewDeviceFound(string data)
    {
        GameObject newDevice = deviceMACText;
        newDevice.GetComponent<Text>().text = data;
        Instantiate(newDevice, devicesListContainer.transform);  
    }

    // Get paired devices from BT settings
    public void GetPairedDevices()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        // This function when called returns an array of PairedDevices as "MAC+Name" for each device found
        string[] data = BluetoothConnector.CallStatic<string[]>("GetPairedDevices"); ;

        // Destroy devicesListContainer child objects for new Paired Devices display
        foreach (Transform child in devicesListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Display the paired devices
        foreach (var d in data)
        {
            GameObject newDevice = deviceMACText;
            newDevice.GetComponent<Text>().text = d;
            Instantiate(newDevice, devicesListContainer.transform);
        }
    }

    // Start BT connect using device MAC address "deviceAdd"
    public void StartConnection()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnector.CallStatic("StartConnection", deviceAdd.text.ToString().ToUpper());

    }

    // Stop BT connetion
    public void StopConnection()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnector.CallStatic("StopConnection");
        
    }

    // This function will be called by Java class to update BT connection status,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ConnectionStatus(string status)
    {
        string additionalDetail = string.Empty;
        if (status == "connected")
        {
            additionalDetail = " to " + devicename;
        }

        Toast("Connection Status: " + status + additionalDetail);
        isConnected = status == "connected";
    }

    // This function will be called by Java class whenever BT data is received,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ReadData(string data)
    {
        //Debug.Log("BT Stream: " + data);
        //receivedData.text = data;
        inputdata = data;
        //receivedData.text = data;
        //BluetoothDataReceived?.Invoke(this, new BluetoothDataEventArgs(data));
    }

    // Write data to the connected BT device
    public void WriteData()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        if (isConnected)
            //should be sending stiffness 
            BluetoothConnector.CallStatic("WriteData", dataToSend.text.ToString());
    }

    // This function will be called by Java class to send Log messages,
    // DO NOT CHANGE ITS NAME OR IT WILL NOT BE FOUND BY THE JAVA CLASS
    public void ReadLog(string data)
    {
        Debug.Log(data);
    }


    // Function to display an Android Toast message
    public void Toast(string data)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        BluetoothConnector.CallStatic("Toast", data);
    }

    // Function to connect paired device by one click
    public void ConnectToPairedDevice()
    {
        InitBluetooth();
        if (Application.platform != RuntimePlatform.Android) return;

        // Get the list of paired devices
        string[] data = BluetoothConnector.CallStatic<string[]>("GetPairedDevices");

        // Connect to devices based on stiff mode on or not
        foreach (var d in data)
        {
            if (!stiffOn)
            {
                if (d.Contains("SpinalLog"))
                {
                    string[] parts = d.Split('+');
                    string macAddress = parts[0];
                    devicename = "SpinalLog";
                    BluetoothConnector.CallStatic("StartConnection", macAddress);
                    break; // Break out of the loop since we found the device to connect to
                }
            } else
            {
                if (d.Contains("Air Case")) // stiff device 
                {
                    string[] parts = d.Split('+');
                    string macAddress = parts[0];
                    devicename = "AirCase";
                    BluetoothConnector.CallStatic("StartConnection", macAddress);
                    break; // Break out of the loop since we found the device to connect to
                }
            }
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }

    
    ///Testing, Generating random inputdata.///
    public void GenerateRandomData(){
        float[] numbers = Enumerable.Range(0, 8)
            .Select(_ => (float)(random.NextDouble() * 25.0))
            .ToArray();

        inputdata = string.Join(",", numbers.Select(n => n.ToString("F2")));
    }

    public void SendData(string dataSend)
    {
        if (isConnected)
            //should be sending stiffness 
            BluetoothConnector.CallStatic("WriteData", dataSend);
    }

    public void setStiff(bool mode)
    {
        stiffOn = mode; // change the stiff value

        Debug.Log("Stiff checked: "+ stiffOn);
    }
}
