using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

public class TFTPreader()
{
	static public void Main(string[] args)
	{
		UdpClient client = new UdpClient("glados@cs.rit.edu", 69);
		client.Close();
	}
}
