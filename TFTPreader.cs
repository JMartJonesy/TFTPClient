using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;

public class TFTPreader()
{
	private const int TFTP_Port = 69;
	private const int fullBlock = 516;

	private string mode;
	private string host;
	private string requestFile;

	private IPAddress hostIP;

	private UdpClient client;

	private IPEndPoint receiveLoc;

	private Dictionary<string, byte> opCodes;

	public TFTPreader(string mode, string host, string fileName)
		:this()
	{
		this.mode = mode;
		this.host = host;
		this.requestFile = fileName;
		hostIP = null;
		client = new UdpClient();
		receiveLoc = null;
		testHostName();
		fillDictionary();
	}

	public bool testHostName()
	{
		IPAddress[] addresses = Dns.GetHostEntry(host).AddressList;
		if(addresses.Length == 0)
		{
			Console.WriteLine("Host :" + host + " not found");
			return false;
		}
		hostIP = addresses[0];
		return true;
	}

	public void fillDictionary()
	{
		opCodes = new Dictionary<string, byte>()
		{
			{"read", 1},
			{"write", 2},
			{"data", 3},
			{"ack", 4},
			{"error", 5}
		};
	}

	public void retreiveFile(byte[] block)
	{
		FileStream fileStream = new FileStream(Directory.GetCurrentDirectory() + "/" + requestFile, FileMode.Create, FileAccess.ReadWrite);
		Stream fileWriter = fileStream;
		
		int blockNum = 1;
		
		do
		{
			fileWriter.Write(block, 4, block.Length);
			blockNum += 1;
			block = sendAck(blockNum);
		}
		while(block.Length == fullBlock);
		
		fileWriter.Write(block, 4, block.Length);
		
		Console.WriteLine("File :" + requestFile + " successfully downloaded");
	}

	public byte[] sendAck(int blockNum)
	{
		byte[] packet = new byte[4];
		packet[0] = 0;
		packet[1] = opCodes["ack"];
		
		//gonna have to split blockNum if over 8 bytes long

		packet[2] = 0;
		packet[3] = (byte)blockNum;

		return packet;
	}

	public void sendRequest()
	{
		byte[] modeBytes = Encoding.ASCII.GetBytes(mode);
		byte[] fileBytes = Encoding.ASCII.GetBytes(requestFile);
		byte[] requestPacket = new byte[4 + fileBytes.Length  + modeBytes.Length];

		requestPacket[0] = 0;
		requestPacket[1] = opCodes["read"];
		for( int i = 0; i < fileBytes.Length; i++ )
			requestPacket[i + 2] = fileBytes[i];
		requestPacket[fileBytes.Length + 2] = 0;
		for( int i = 0; i < modeBytes.Length; i++ )
			requestPacket[fileBytes.Length + 3 + i] = modeBytes[i];
		requestPacket[requestPacket.Length - 1] = 0;

		IPEndPoint destination = new IPEndPoint(hostIP, TFTP_Port);
		byte[] firstBlock = sendReceivePacket(requestPacket, destination);
		if(firstBlock != null)
		{
			retreiveFile(firstBlock);
		}
	
		closeClient();
	}

	public byte[] sendReceivePacket(byte[] packet, IPEndPoint destination)
	{
		client.Send(packet, packet.Length, destination);
		int port = destination.Port;
		receiveLoc = new IPEndPoint(hostIP, port);
		byte[] receivePacket = client.Receive(ref receiveLoc);
		if(!checkError(receivePacket))
			return null;
		return receivePacket;
	}

	public bool checkError(byte[] packet)
	{
		if(packet[1] == opCodes["error"])
		{
			byte[] error = new byte[packet.Length - 5];
			for(int i = 4; i < (packet.Length - 1); i++)
			{
				error[i-4] = packet[i];
			}
			Console.WriteLine("Error code " + packet[0] + packet[1] + ": " + Encoding.ASCII.GetString(error));
			return false;
		}
		return true;
	}

	public void closeClient()
	{
		client.Close();
	}

	static public void Main(string[] args)
	{
		if(args.Length == 3)
		{
			TFTPreader tftp = new TFTPreader(args[0], args[1], args[2]);
			tftp.sendRequest();
		}
		else
			Console.WriteLine("usage");
	}
}
