using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NativeWebSocket;
using Newtonsoft.Json.Linq;

public class ConnectionWebSocket : MonoBehaviour
{
    public static ConnectionWebSocket Instance { get; protected set; }
    public UnityEvent<string> AnswerEvent;
    public UnityEvent<bool> BciEdge;
    public bool BciConnected; // <-- caches the level of the above edge
    [SerializeField]
    string websocketName = "ws://35.234.253.134:3001";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    WebSocket websocket;
    public string answer;

    // Start is called before the first frame update
    async void Start()
    {
        //websocket = new WebSocket("ws://backend.cortanahologram.com:3001");
        websocket = new WebSocket(websocketName);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            BciConnected = true;
            BciEdge.Invoke(BciConnected);
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
            BciConnected = false;
            BciEdge.Invoke(BciConnected);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            BciConnected = false;
            BciEdge.Invoke(BciConnected);
        };

        websocket.OnMessage += (bytes) =>
        {
            // Parse out the yes/no responses from the BCI.
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received OnMessage! (" + bytes.Length + " bytes) " + message);

            try
            {
                JSONHandler(message);
            }
            catch (Exception ex)
            {
                string strMsg = message.ToString().Replace("\"", "\'");
                Debug.Log("Had to use String Replace to get the answers");
                JSONHandler(strMsg);
            }
        };
        await websocket.Connect();
        await websocket.SendText("ARE_YOU_THERE_BCI");
    }

    void Update()
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
    #endif

        if (Input.GetKeyDown(KeyCode.Space))    // Debug for Editor
        {
            Invoke("SendStart", 0.1f);
            BciConnected = true;
            BciEdge.Invoke(BciConnected);
        }
    }

    public async void SendAreYouThere()
    {
        if (websocket.State == WebSocketState.Open) {
            // Sending plain text
            await websocket.SendText("ARE_YOU_THERE_BCI");
            Debug.Log("Sent the ARE_YOU_THERE_BCI command!");
            BciConnected = true;
            BciEdge.Invoke(BciConnected);
        } else {
            Debug.Log("Websocket is not open :(");
            BciConnected = false;
            BciEdge.Invoke(BciConnected);
        }
    }

    public async void SendStart()
    {
        await websocket.SendText("START_COMMAND");
        Debug.Log("Sent the start command!");
        BciConnected = true;
        BciEdge.Invoke(BciConnected);
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
        BciConnected = false;
        BciEdge.Invoke(BciConnected);
    }

    void JSONHandler(string message)
    {
        JToken msg_json = JToken.Parse(message);
        switch (msg_json.Type)
        {
            case JTokenType.String:
                JValue msg_string = (JValue)msg_json;
                if (msg_string.Value<String>() == "foundBci")
                {
                    BciConnected = true;
                    BciEdge.Invoke(BciConnected);
                    break;
                }//assumed postcondition: is "yes" or "no"
                AnswerEvent.Invoke(msg_string.Value<String>());
                break;
            case JTokenType.Object:
                string type_field = msg_json.Value<String>("type");
                switch (type_field)
                {
                    case "BciConnectedStatus":
                        // observe a state transition
                        if (BciConnected != msg_json.Value<bool>("data"))
                        {
                            // toggle the "latch"
                            BciConnected = !BciConnected;
                            // ... this is called an edge
                            BciEdge.Invoke(BciConnected);
                        }
                        break;
                    default:
                        Debug.LogFormat("who gave me this unknoweable type_field??? {0}", type_field);
                        break;
                }
                break;
            default:
                Debug.LogError("who gave me this bizarro json object?? surely not the server!");
                break;
        }
    }
}