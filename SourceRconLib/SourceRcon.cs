using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace SourceRcon
{
	/// <summary>
	/// Summary description for SourceRcon.
	/// </summary>
	public class SourceRcon
	{
		public SourceRcon()
		{
			S = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
			PacketCount = 0;

#if DEBUG
			TempPackets = new ArrayList();
#endif
		}

		public bool Connect(IPEndPoint Server, string password)
		{
			try
			{
				S.Connect(Server);
			}
			catch(SocketException)
			{
				OnError(ConnectionFailedString);
				OnConnectionSuccess(false);
				return false;
			}

			RCONPacket SA = new RCONPacket();
			SA.RequestId = 1;
			SA.String1 = password;
			SA.ServerDataSent = RCONPacket.SERVERDATA_sent.SERVERDATA_AUTH;

			SendRCONPacket(SA);
			
			// This is the first time we've sent, so we can start listening now!
			StartGetNewPacket();

			return true;
		}

		public void ServerCommand(string command)
		{
			if(connected)
			{
				RCONPacket PacketToSend = new RCONPacket();
				PacketToSend.RequestId = 2;
				PacketToSend.ServerDataSent = RCONPacket.SERVERDATA_sent.SERVERDATA_EXECCOMMAND;
				PacketToSend.String1 = command;
				SendRCONPacket(PacketToSend);
			}
		}
	
		void SendRCONPacket(RCONPacket p)
		{
			byte[] Packet = p.OutputAsBytes();
			S.BeginSend(Packet,0,Packet.Length,SocketFlags.None,new AsyncCallback(SendCallback),this);
		}

		bool connected;
		public bool Connected
		{
			get { return connected; }
		}

		void SendCallback(IAsyncResult ar)
		{
			S.EndSend(ar);
		}

		int PacketCount;

		void StartGetNewPacket()
		{
			RecState state = new RecState();
			state.IsPacketLength = true;
			state.Data = new byte[4];
			state.PacketCount = PacketCount;
			PacketCount++;
#if DEBUG
			TempPackets.Add(state);
#endif
			S.BeginReceive(state.Data,0,4,SocketFlags.None,new AsyncCallback(ReceiveCallback),state);
		}

#if DEBUG
		public ArrayList TempPackets;
#endif

		void ReceiveCallback(IAsyncResult ar)
		{
			bool recsuccess = false;
			RecState state = null;

			try
			{
				int	bytesgotten = S.EndReceive(ar);
				state = (RecState)ar.AsyncState;
				state.BytesSoFar += bytesgotten;
				recsuccess = true;

#if DEBUG
			Console.WriteLine("Receive Callback. Packet: {0} First packet: {1}, Bytes so far: {2}",state.PacketCount,state.IsPacketLength,state.BytesSoFar);
#endif

			}
			catch(SocketException)
			{
				OnError(ConnectionClosed);
			}

			if(recsuccess)
			ProcessIncomingData(state);
		}

		void ProcessIncomingData(RecState state)
		{
			if(state.IsPacketLength)
			{
				// First 4 bytes of a new packet.
				state.PacketLength = BitConverter.ToInt32(state.Data,0);

				state.IsPacketLength = false;
				state.BytesSoFar = 0;
				state.Data = new byte[state.PacketLength];
				S.BeginReceive(state.Data,0,state.PacketLength,SocketFlags.None,new AsyncCallback(ReceiveCallback),state);
			}
			else
			{
				// Do something with data...

				if(state.BytesSoFar < state.PacketLength)
				{
					// Missing data.
					S.BeginReceive(state.Data,state.BytesSoFar,state.PacketLength - state.BytesSoFar,SocketFlags.None,new AsyncCallback(ReceiveCallback),state);
				}
				else
				{
					// Process data.
#if DEBUG
					Console.WriteLine("Complete packet.");
#endif

					RCONPacket RetPack = new RCONPacket();
					RetPack.ParseFromBytes(state.Data,this);

					ProcessResponse(RetPack);

					// Wait for new packet.
					StartGetNewPacket();
				}
			}
		}

		void ProcessResponse(RCONPacket P)
		{
			switch(P.ServerDataReceived)
			{
				case RCONPacket.SERVERDATA_rec.SERVERDATA_AUTH_RESPONSE:
					if(P.RequestId != -1)
					{
						// Connected.
						connected = true;
						OnError(ConnectionSuccessString);
						OnConnectionSuccess(true);
					}
					else
					{
						// Failed!
						OnError(ConnectionFailedString);
						OnConnectionSuccess(false);
					}
					break;
				case RCONPacket.SERVERDATA_rec.SERVERDATA_RESPONSE_VALUE:
					if(hadjunkpacket)
					{
						// Real packet!
						OnServerOutput(P.String1);
					}
					else
					{
						hadjunkpacket = true;
						OnError(GotJunkPacket);
					}
					break;
				default:
					OnError(UnknownResponseType);
					break;
			}
		}

		bool hadjunkpacket;

		internal void OnServerOutput(string output)
		{
			if(ServerOutput != null)
			{
				ServerOutput(output);
			}
		}

		internal void OnError(string error)
		{
			if(Errors != null)
			{
				Errors(error);
			}
		}

		internal void OnConnectionSuccess(bool info)
		{
			if(ConnectionSuccess != null)
			{
				ConnectionSuccess(info);
			}
		}

		public event StringOutput ServerOutput;
		public event StringOutput Errors;
		public event BoolInfo ConnectionSuccess;

		public static string ConnectionClosed = "Connection closed by remote host";
		public static string ConnectionSuccessString = "Connection Succeeded!";
		public static string ConnectionFailedString = "Connection Failed!";
		public static string UnknownResponseType = "Unknown response";
		public static string GotJunkPacket = "Had junk packet. This is normal.";

		Socket S;
	}

	public delegate void StringOutput(string output);
	public delegate void BoolInfo(bool info);

	internal class RecState
	{
		internal RecState()
		{
			PacketLength = -1;
			BytesSoFar = 0;
			IsPacketLength = false;
		}

		public int PacketCount;
		public int PacketLength;
		public int BytesSoFar;
		public bool IsPacketLength;
		public byte[] Data;
	}

	

	internal class RCONPacket
	{
		internal RCONPacket()
		{
			RequestId = 0;
			String1 = "blah";
			String2 = String.Empty;
			ServerDataSent = SERVERDATA_sent.None;
			ServerDataReceived = SERVERDATA_rec.None;
		}

		internal byte[] OutputAsBytes()
		{
			byte[] packetsize;
			byte[] reqid;
			byte[] serverdata;
			byte[] bstring1;
			byte[] bstring2;

			ASCIIEncoding Ascii = new ASCIIEncoding();
			
			bstring1 = Ascii.GetBytes(String1);
			bstring2 = Ascii.GetBytes(String2);

			serverdata = BitConverter.GetBytes((int)ServerDataSent);
			reqid = BitConverter.GetBytes(RequestId);

			// Compose into one packet.
			byte[] FinalPacket = new byte[4 + 4 + 4 + bstring1.Length + 1 + bstring2.Length + 1];
			packetsize = BitConverter.GetBytes(FinalPacket.Length - 4);

			int BPtr = 0;
			packetsize.CopyTo(FinalPacket,BPtr);
			BPtr += 4;

			reqid.CopyTo(FinalPacket,BPtr);
			BPtr += 4;

			serverdata.CopyTo(FinalPacket,BPtr);
			BPtr += 4;

			bstring1.CopyTo(FinalPacket,BPtr);
			BPtr += bstring1.Length;

			FinalPacket[BPtr] = (byte)0;
			BPtr++;

			bstring2.CopyTo(FinalPacket,BPtr);
			BPtr += bstring2.Length;

			FinalPacket[BPtr] = (byte)0;
			BPtr++;

			return FinalPacket;
		}

		internal void ParseFromBytes(byte[] bytes, SourceRcon parent)
		{
			int BPtr = 0;
			ArrayList stringcache;
			ASCIIEncoding Ascii = new ASCIIEncoding();

			// First 4 bytes are ReqId.
			RequestId = BitConverter.ToInt32(bytes,BPtr);
			BPtr += 4;
			// Next 4 are server data.
			ServerDataReceived = (SERVERDATA_rec)BitConverter.ToInt32(bytes,BPtr);
			BPtr += 4;
			// string1 till /0
			stringcache = new ArrayList();
			while(bytes[BPtr] != 0)
			{
				stringcache.Add(bytes[BPtr]);
				BPtr++;
			}
			String1 = Ascii.GetString((byte[])stringcache.ToArray(typeof(byte)));
			BPtr++;

			// string2 till /0

			stringcache = new ArrayList();
			while(bytes[BPtr] != 0)
			{
				stringcache.Add(bytes[BPtr]);
				BPtr++;
			}
			String2 = Ascii.GetString((byte[])stringcache.ToArray(typeof(byte)));
			BPtr++;

			// Repeat if there's more data?

			if(BPtr != bytes.Length)
			{
				parent.OnError("Urk, extra data!");
			}
		}

		public enum SERVERDATA_sent : int
		{
			SERVERDATA_AUTH = 3,
			SERVERDATA_EXECCOMMAND = 2,
			None = 255
		}

		public enum SERVERDATA_rec : int
		{
			SERVERDATA_RESPONSE_VALUE = 0,
			SERVERDATA_AUTH_RESPONSE = 2,
			None = 255
		}

		internal int RequestId;
		internal string String1;
		internal string String2;
		internal RCONPacket.SERVERDATA_sent ServerDataSent;
		internal RCONPacket.SERVERDATA_rec ServerDataReceived;
	}
}
