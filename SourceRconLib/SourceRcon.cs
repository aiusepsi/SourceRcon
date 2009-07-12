using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;

namespace SourceRconLib
{
    /// <summary>
    /// Encapsulates RCON communication with a Source server.
    /// </summary>
    public class Rcon
    {
        public event RconOutput ServerOutput;
        public event RconOutput Errors;
        public event BoolInfo ConnectionSuccess;

        Socket S;

        int RequestIDCounter;

        void Reset()
        {
            S = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            PacketCount = 0;
            RequestIDCounter = 0;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Rcon()
        {

#if DEBUG
			    TempPackets = new ArrayList();
#endif
        }

        /// <summary>
        /// Attempts to connect to server.
        /// </summary>
        /// <param name="Server">The IPEndpoint of the server to contact.</param>
        /// <param name="password">RCON password.</param>
        /// <returns>True if connection successful, false otherwise.</returns>
        public void Connect(IPEndPoint Server, string password)
        {
            try
            {
                Reset();
                S.Connect(Server);
            }
            catch (SocketException)
            {
                OnError(MessageCode.ConnectionFailed, null);
                OnConnectionSuccess(false);
                return;
            }

            RCONPacket ServerAuthPacket = new RCONPacket();

            ++RequestIDCounter;
            ServerAuthPacket.RequestId = RequestIDCounter;

            ServerAuthPacket.String1 = password;
            ServerAuthPacket.ServerDataSent = RCONPacket.SERVERDATA_sent.SERVERDATA_AUTH;

            SendRCONPacket(ServerAuthPacket);

            //Start the listening loop, now that we've sent auth packet, we should be expecting a reply.
            GetPacketFromServer();
        }

        public bool ConnectBlocking(IPEndPoint Server, string password)
        {
            bool connected = false;
            AutoResetEvent e = new AutoResetEvent(false);

            this.ConnectionSuccess += (IsConnected) => { connected = IsConnected; e.Set(); };

            this.Connect(Server, password);
            e.WaitOne();

            return connected;
        }

        /// <summary>
        /// Sends a command to the server. Result is returned asynchronously via callbacks
        /// so wire those up before using this.
        /// </summary>
        /// <param name="command">Command to send.</param>
        public void ServerCommand(string command)
        {
            if (connected)
            {
                RCONPacket PacketToSend = new RCONPacket();
                ++RequestIDCounter;
                PacketToSend.RequestId = RequestIDCounter;
                PacketToSend.ServerDataSent = RCONPacket.SERVERDATA_sent.SERVERDATA_EXECCOMMAND;
                PacketToSend.String1 = command;
                SendRCONPacket(PacketToSend);
            }
        }

        public string ServerCommandBlocking(string command)
        {
            string s = null;
            AutoResetEvent e = new AutoResetEvent(false);

            RconOutput output = (MessageCode code, string stringout) => { s = stringout; e.Set();  };

            this.ServerOutput += output;

            ServerCommand(command);
            e.WaitOne();

            ServerOutput -= output;
            return s;
        }

        void SendRCONPacket(RCONPacket p)
        {
            byte[] Packet = p.OutputAsBytes();
            S.BeginSend(Packet, 0, Packet.Length, SocketFlags.None, new AsyncCallback(SendCallback), this);
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

        void GetPacketFromServer()
        {
            PacketState state = new PacketState();
            state.IsPacketLength = true;
            state.Data = new byte[4];
            state.PacketCount = PacketCount;
            PacketCount++;

#if DEBUG
			    TempPackets.Add(state);
#endif

            try
            {
                S.BeginReceive(state.Data, 0, 4, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            catch (SocketException se)
            {
                OnError(MessageCode.ConnectionFailed, se.Message);
            }
        }

#if DEBUG
		public ArrayList TempPackets;
#endif

        void ReceiveCallback(IAsyncResult ar)
        {
            bool recsuccess = false;
            PacketState state = null;

            try
            {
                int bytesgotten = S.EndReceive(ar);
                state = (PacketState)ar.AsyncState;
                state.BytesSoFar += bytesgotten;
                recsuccess = true;

#if DEBUG
			        Console.WriteLine("Receive Callback. Packet: {0} First packet: {1}, Bytes so far: {2}",
                                        state.PacketCount,state.IsPacketLength,state.BytesSoFar);
#endif

            }
            catch (SocketException)
            {
                OnError(MessageCode.ConnectionClosed, null);
            }

            if (recsuccess)
                ProcessIncomingData(state);
        }

        void ProcessIncomingData(PacketState state)
        {
            if (state.IsPacketLength)
            {
                // First 4 bytes of a new packet.
                state.PacketLength = BitConverter.ToInt32(state.Data, 0);

                state.IsPacketLength = false;
                state.BytesSoFar = 0;
                state.Data = new byte[state.PacketLength];
                S.BeginReceive(state.Data, 0, state.PacketLength, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                // Do something with data...

                if (state.BytesSoFar < state.PacketLength)
                {
                    // Missing data.
                    S.BeginReceive(state.Data, state.BytesSoFar, state.PacketLength - state.BytesSoFar, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // Process data.
#if DEBUG
					    Console.WriteLine("Complete packet.");
#endif

                    RCONPacket RetPack = new RCONPacket();
                    RetPack.ParseFromBytes(state.Data, this);

                    ProcessResponse(RetPack);

                    // Wait for new packet.
                    GetPacketFromServer();
                }
            }
        }

        void ProcessResponse(RCONPacket P)
        {
            switch (P.ServerDataReceived)
            {
                case RCONPacket.SERVERDATA_rec.SERVERDATA_AUTH_RESPONSE:
                    if (P.RequestId != -1)
                    {
                        // Connected.
                        connected = true;
                        OnError(MessageCode.ConnectionSuccess, null);
                        OnConnectionSuccess(true);
                    }
                    else
                    {
                        // Failed!
                        OnError(MessageCode.ConnectionFailed, null);
                        OnConnectionSuccess(false);
                    }
                    break;
                case RCONPacket.SERVERDATA_rec.SERVERDATA_RESPONSE_VALUE:
                    if (hadjunkpacket)
                    {
                        // Real packet!
                        OnServerOutput(MessageCode.ConsoleOutput, P.String1);
                    }
                    else
                    {
                        hadjunkpacket = true;
                        OnError(MessageCode.JunkPacket, null);
                    }
                    break;
                default:
                    OnError(MessageCode.UnknownResponse, null);
                    break;
            }
        }

        bool hadjunkpacket;

        internal void OnServerOutput(MessageCode code, string data)
        {
            if (ServerOutput != null)
            {
                ServerOutput(code, data);
            }
        }

        internal void OnError(MessageCode code, string data)
        {
            if (Errors != null)
            {
                Errors(code, data);
            }
        }

        internal void OnConnectionSuccess(bool info)
        {
            if (ConnectionSuccess != null)
            {
                ConnectionSuccess(info);
            }
        }
    }
}