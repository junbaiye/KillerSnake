﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using scMessage;

public class Client : MonoBehaviour
{

	private const int PORT = 3000;// server port
	
	private Socket socket; // server socket
	
	private string
		ipAddress = "127.0.0.1"; // server ip address
	
	public bool connectedToServer = false;
	
	private Queue<message> incMessages = new Queue<message> ();
		
	public static Client Instance { get; private set; }
	
	void Awake ()
	{
		Instance = this;
		DontDestroyOnLoad (this);
	}
	
	// Update is called once per frame
	void Update ()
	{
		for (int i = 0; i < incMessages.Count; i++) {
			handleData (incMessages.Dequeue ());
		}
	}
	
	void OnApplicationQuit ()
	{		
		socket.Close ();
	}
	
	public void connect ()
	{
		if (!connectedToServer) {
			try {
				socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect (new IPEndPoint (IPAddress.Parse (ipAddress), PORT));
				new Connection (socket);
				
				connectedToServer = true;
				
			} catch {
				Debug.Log ("Unable to connect to server.");
			}
		}
	}
	
	public void addServerMessageToQueue (message msg)
	{
		incMessages.Enqueue (msg);
	}
	
	private void handleData (message msg)
	{
		Debug.Log (msg.messageText);

		string command = msg.getSCObject ("head").getString ("command");
		if (command.Equals ("login")) {
			GameObject.Find ("Login").GetComponent<Login> ().loginResponse (msg);
		} else if (command.Equals ("register")) {
			GameObject.Find ("Register").GetComponent<Register> ().registerResponse (msg);
		}
	}
	
	public void SendServerMessage (message mes)
	{
		if (connectedToServer) {
			try {
				// convert message into a byte array, wrap the message with framing
				byte[] messageObject = conversionTools.convertObjectToBytes (mes);
				byte[] readyMessage = conversionTools.wrapMessage (messageObject);
				
				// send completed message
				socket.Send (readyMessage);
			} catch {
				Debug.Log ("There was an error sending server message " + mes.messageText);
			}
		}
	}
}
