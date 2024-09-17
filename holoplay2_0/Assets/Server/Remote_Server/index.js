const { clear } = require('console');
const crypto = require('crypto');
const express = require('express');
const { createServer } = require('http');
const WebSocket = require('ws');

const app = express();
const unity_port = 3001;
const bci_port = 3000;

// Create a class that stores the trigger for the BCI connection from the Unity scene
class Trigger {
    constructor() {
        this.trigger = false;
    }
    getTrigger() {
        return this.trigger;
    }
    setTrigger(trigger) {
        this.trigger = !trigger;
    }
}

function bciServer(app, port, broadcast_cb) {
  const server = createServer(app);
  const wss = new WebSocket.Server({ server });
  wss.on('connection', function (bci_conn) {
    const alivePing = setInterval(() => bci_conn.send("ping"), 5000);
    // We should wait for a trigger message from the Unity scene to start the BCI connection based on after Cortana 
    // We need to send to the Unity Server the connection event with the BCI
    // Send message code here
    // Once we get connected, then we can send the START_COMMAND
    const startCmd = bci_conn.send("START_COMMAND");
    bci_conn.on('message', function (data) {
      if (typeof (data) === "string") {
        console.log("string received from BCI -> " + data);

        const obj = JSON.parse(data);
        console.log("BCI response -> " + obj.data.answer);
        broadcast_cb(obj.data.answer);
      }
    });

    bci_conn.on('close', function () {
      console.log("BCI disconnected.");
      clear(startCmd);
      clearInterval(alivePing);
    });
  });

  server.listen(port);
}

function unityServer(app,port) {
  const server = createServer(app);
  const wss = new WebSocket.Server({ server });
  const connections = [];

  wss.on('connection', function (ws) {
    console.info(`Unity client ${ws} joined.`);
    connections.push(ws);
  });

  wss.on('close', function() {
    console.info(`Unity client ${ws} lost.`)
    console.debug(connections);
    connections.splice(connections.indexOf(ws), 1);
  })

  // on message function
  wss.on('message', function (data) {
    console.log("message received from Unity -> " + data);
    // We need to call this in order to sta
  });
  
  server.listen(port);

  return function broadcast(data) {
    connections.forEach(client => client.send(data));
  };
}

const broadcast = unityServer(app, unity_port);
bciServer(app, bci_port, broadcast);