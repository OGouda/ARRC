using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using System;
using Vuforia;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using Image = UnityEngine.UI.Image;

public class Bandwidth : MonoBehaviour
{
    private DatabaseReference DataBTV;
    private DatabaseReference DataBAC;
    private MqttClient client;
    private String ONF, VolU, VolD, ChU, ChD;
    private String ACONF, TEMPU, TEMPD, MOD;
    public Text AC_txt, TV_txt, Online, Online2;
    private String ACStatus = "100", TVStatus = "100";
    private String[] messages = new String[10];
    public Image ACBATTERY, TVBATTERY, TVimage, ACimage;
    private int count = 0, ME = 0;
    public float fadeTime;
    private bool startsend = false;
    public Sprite FULL, GOOD, MEDIEUM, LOW;
    // Use this for initialization
    void Start()
    {
        // Set up the Editor before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://arrcsenior.firebaseio.com/");

        // Get the root reference location of the database.
        DataBTV = FirebaseDatabase.DefaultInstance.GetReference("TV");
        DataBAC = FirebaseDatabase.DefaultInstance.GetReference("AC");
        GetIRCodes();

        // create client instance 
        client = new MqttClient(IPAddress.Parse("192.168.137.78"), 1883, false, null);

        // register to message received 
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

        string clientId = Guid.NewGuid().ToString();
        client.Connect(clientId);

        // subscribe to the topic "/home/temperature" with QoS 2 

        client.Subscribe(new string[] { "IamOnline" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        client.Subscribe(new string[] { "home/bedroom/AC" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        client.Subscribe(new string[] { "home/livingroom/TV" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        client.Subscribe(new string[] { "home/livingroom/TVBATT" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        client.Subscribe(new string[] { "home/start" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });


        updateStatus();
    }



    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        if (e.Topic == "home/start")
        {
            //Thread.Sleep(500);
            startsend = true;
            // Debug.Log("<color=blue> Received: </color>" + System.Text.Encoding.UTF8.GetString(e.Message));
            //OnlineStatus= System.Text.Encoding.UTF8.GetString(e.Message);
            messages[count] = System.Text.Encoding.UTF8.GetString(e.Message);
            count++;
        }

        else if (e.Topic == "home/bedroom/AC")
        {
            //Debug.Log("<color=blue> Received: </color>" + System.Text.Encoding.UTF8.GetString(e.Message));
            ACStatus = System.Text.Encoding.UTF8.GetString(e.Message);
        }
        else if (e.Topic == "home/livingroom/TV")
        {
            // Debug.Log("<color=blue> Received: </color>" + System.Text.Encoding.UTF8.GetString(e.Message));
            TVStatus = System.Text.Encoding.UTF8.GetString(e.Message);
        }
        else if (e.Topic == "home/livingroom/TVBATT")
        {
            // Debug.Log("<color=blue> Received: </color>" + System.Text.Encoding.UTF8.GetString(e.Message));
            TVStatus = System.Text.Encoding.UTF8.GetString(e.Message);
            Debug.Log(TVStatus);
        }
        else if (e.Topic == "home/livingroom/ACBATT")
        {
            // Debug.Log("<color=blue> Received: </color>" + System.Text.Encoding.UTF8.GetString(e.Message));
            ACStatus = System.Text.Encoding.UTF8.GetString(e.Message);
        }

    }
    public void updateStatus()
    {
        int ACBATT = int.Parse(ACStatus);
        int TVBATT = int.Parse(TVStatus);
        if (TVBATT > 75)
        {
            TVBATTERY.color = Color.green;
            TVBATTERY.sprite = FULL;
        }
        else if (TVBATT <= 75 && TVBATT > 50)
        {
            TVBATTERY.color = Color.yellow;
            TVBATTERY.sprite = GOOD;
        }
        else if (TVBATT <= 50 && TVBATT > 25)
        {
            Color Orange = new Color(1f, 0.6f, 0f);
            TVBATTERY.color = Orange;
            TVBATTERY.sprite = MEDIEUM;
        }
        else if (TVBATT <= 25 && TVBATT > 1)
        {
            TVBATTERY.color = Color.red;
            TVBATTERY.sprite = LOW;
        }
        TV_txt.text = TVStatus + "%";





        ListOnline();
    }
    public void ListOnline()
    {
        if (messages[ME] == "home/livingroom/TV" || messages[ME + 1] == "home/livingroom/TV")
            TVimage.enabled = true;
        else
            TVimage.enabled = false;
        if (messages[ME] == "home/bedroom/AC" || messages[ME + 1] == "home/bedroom/AC")
            ACimage.enabled = true;
        else
            ACimage.enabled = false;
    }
    public void AC_ON_Click()
    {
        Debug.Log("sending...");
        client.Publish("home/bedroom/ACcom", System.Text.Encoding.UTF8.GetBytes(ACONF), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        Debug.Log("sent");
    }
    public void AC_TempU_Click()
    {
        Debug.Log("sending...");
        client.Publish("home/bedroom/ACcom", System.Text.Encoding.UTF8.GetBytes(TEMPU), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        Debug.Log("sent");
    }
    public void AC_TEMPD_Click()
    {
        Debug.Log("sending...");
        client.Publish("home/bedroom/ACcom", System.Text.Encoding.UTF8.GetBytes(TEMPD), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        Debug.Log("sent");
    }
    public void AC_MOD_Click()
    {
        Debug.Log("sending...");
        client.Publish("home/bedroom/ACcom", System.Text.Encoding.UTF8.GetBytes(MOD), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        Debug.Log("sent");
    }



    public void TV_ON_Click()
    {
        Debug.Log("sending...");
        client.Publish("home/livingroom/TVcom", System.Text.Encoding.UTF8.GetBytes(ONF), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        Debug.Log("sent");
    }
    public void TV_VolU_Click()
    {
        Debug.Log("sending...");
        client.Publish("home/livingroom/TVcom", System.Text.Encoding.UTF8.GetBytes(VolU), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        Debug.Log("sent");
    }
    public void TV_VOLD_Click()
    {
        Debug.Log("sending...");
        client.Publish("home/livingroom/TVcom", System.Text.Encoding.UTF8.GetBytes(VolD), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        Debug.Log("sent");
    }
    public void TV_CHU_Click()
    {
        Debug.Log("sending...");
        client.Publish("home/livingroom/TVcom", System.Text.Encoding.UTF8.GetBytes(ChU), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        Debug.Log("sent");
    }
    public void TV_CHD_Click()
    {
        Debug.Log("sending...");
        client.Publish("home/livingroom/TVcom", System.Text.Encoding.UTF8.GetBytes(ChD), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        Debug.Log("sent");
    }

    public void Online_Click()
    {

        Debug.Log("sending...");
        client.Publish("Whoisthere", System.Text.Encoding.UTF8.GetBytes("?"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        Debug.Log("sent");
    }
    private float nextActionTime = 0.0f;
    public float period = 1f;
    private int packets = 0;
    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += period;
            if (startsend){
                if (packets <=999)
                {
                    TV_ON_Click();
                    packets++;
                }
                if (packets >=1000 && packets < 2000)
                {
                    AC_TEMPD_Click();
                    packets++;
                }

            }

            
        }
        updateStatus();


    }

    private void GetIRCodes()
    {
        DataBTV.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            { }

            else if (task.IsCompleted)
            {
                Dictionary<String, object> results = (Dictionary<String, object>)task.Result.Value;
                ONF = (String)results["ON-OFF"];
                VolU = (String)results["VOL+"];
                VolD = (String)results["VOL-"];
                ChU = (String)results["CH+"];
                ChD = (String)results["CH-"];

            }

        });

        DataBAC.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            { }

            else if (task.IsCompleted)
            {
                Dictionary<String, object> ACresults = (Dictionary<String, object>)task.Result.Value;
                ACONF = (String)ACresults["ON-OFF"];
                TEMPU = (String)ACresults["TEMPU"];
                TEMPD = (String)ACresults["TEMPD"];
                MOD = (String)ACresults["MOD"];

            }

        });

    }


}
