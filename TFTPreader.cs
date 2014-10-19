using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;

public class TFTPreader()
{

	static public void Main(string[] args)
	{
		Dictionary<string, int> opCodes = new Dictionary<string, int>()
		{
			{"read", 1},
			{"write", 2},
			{"data", 3},
			{"ack", 4},
			{"error", 5}
		};
		
		string mode = args[0];
		string host = args[1];
		string file = args[2];

		byte[] modeBytes = Encoding.ASCII.GetBytes(mode);
		byte[] fileBytes = Encoding.ASCII.GetBytes(file);
		byte[] requestPacket = new byte[2 + fileBytes.Length + 1 + modeBytes.Length + 1];

		requestPacket[0] = (byte)0;
		requestPacket[1] = (byte)opCodes["read"];
		for( int i = 0; i < fileBytes.Length; i++ )
			requestPacket[i + 2] = fileBytes[i];
		requestPacket[fileBytes.Length + 2] = (byte)0;
		for( int i = 0; i < modeBytes.Length; i++ )
			requestPacket[fileBytes.Length + 3 + i] = modeBytes[i];
		requestPacket[requestPacket.Length - 1] = (byte)0;

		IPEndPoint srcPoint;
		IPEndPoint endPoint = new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], 69);

		UdpClient client = new UdpClient();
		
		client.Send(requestPacket, requestPacket.Length, endPoint);
		
		int sendPort = ((IPEndPoint)client.Client.LocalEndPoint).Port;
		string sendIP = ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString();

		Console.WriteLine(sendPort + " " + sendIP);

		srcPoint = new IPEndPoint(IPAddress.Any, sendPort);

		byte[] returnBytes = client.Receive(ref srcPoint);

		client.Close();
	
		Console.WriteLine(returnBytes[1]);
	}
}
