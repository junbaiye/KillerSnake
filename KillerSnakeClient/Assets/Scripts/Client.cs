﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using scMessage;

public class Client : MonoBehaviour
{

	private int
		sPort = 3000, // server port
		pfrPort = 2999; // policy file request port
	
	private Socket
		cSock; // client socket
	
	private string
		ipAddress = "127.0.0.1"; // server ip address
	
	public bool
		connectedToServer = false;
	
	private List<message>
		incMessages = new List<message> ();
	
	public static Client Instance { get; private set; }
	
	void Awake ()
	{
		Instance = this;
		DontDestroyOnLoad (this);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (incMessages.Count > 0) {
			doMessages ();
		}
	}
	
	public void connect ()
	{
		if (!connectedToServer) {
			try {
				// get policy if we are on the web or in editor
				if ((Application.platform == RuntimePlatform.WindowsWebPlayer) || (Application.platform == RuntimePlatform.WindowsEditor)) {
					Security.PrefetchSocketPolicy (ipAddress, pfrPort);
				}
			
				cSock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				cSock.Connect (new IPEndPoint (IPAddress.Parse (ipAddress), sPort));
				Connection gsCon = new Connection (cSock);
			} catch {
				Debug.Log ("Unable to connect to server.");
			}
		}
	}
	
	public void onConnect ()
	{
		connectedToServer = true;
		
		// test the connection
		message testMessage = new message ("LOGIN");
		SendServerMessage (testMessage);
	}
	
	private void OnApplicationQuit ()
	{
		try {
			cSock.Close ();
		} catch {
		}
	}
	
	public void addServerMessageToQue (message msg)
	{
		incMessages.Add (msg);
	}
	
	private void doMessages ()
	{
		// do messages
		List<message> completedMessages = new List<message> ();
		for (int i = 0; i < incMessages.Count; i++) {
			try {
				handleData (incMessages [i]);
				completedMessages.Add (incMessages [i]);
			} catch {
			}
		}
		
		// delete completed messages
		for (int i = 0; i < completedMessages.Count; i++) {
			try {
				incMessages.Remove (completedMessages [i]);
			} catch {
			}
		}
	}
	
	private void handleData (message mess)
	{
		Debug.Log ("The server sent a message: " + mess.messageText);
	}
	
	public void SendServerMessage (message mes)
	{
		if (connectedToServer) {
			try {
				// convert message into a byte array, wrap the message with framing
				byte[] messageObject = conversionTools.convertObjectToBytes (mes);
				byte[] readyMessage = conversionTools.wrapMessage (messageObject);
				
				// send completed message
				cSock.Send (readyMessage);
			} catch {
				Debug.Log ("There was an error sending server message " + mes.messageText);
			}
		}
	}
}