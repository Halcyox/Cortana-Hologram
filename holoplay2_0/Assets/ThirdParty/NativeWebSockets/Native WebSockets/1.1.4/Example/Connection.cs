using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

public class Connection : MonoBehaviour
{
    public static Connection Instance { get; protected set; }

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
        // websocket = new WebSocket("ws://echo.websocket.org");
        websocket = new WebSocket("ws://localhost:3000");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // Reading a plain text message
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received OnMessage! (" + bytes.Length + " bytes) " + message);

            if (message.ToString() == "yes" || message.ToString() == "no")
            {
                answer = message.ToString();
            }
        };

        // Keep sending messages at every 0.3s
        //InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);
        Invoke("SendWebSocketMessage", 0.1f);

        await websocket.Connect();
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
        #endif

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    websocket.DispatchMessageQueue();
        //}
    }

    async void SendWebSocketMessage()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Sending bytes
            await websocket.Send(new byte[] { 10, 20, 30 });

            // Sending plain text
            //await websocket.SendText("plain text message");
            await websocket.SendText("I am Unity");
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
