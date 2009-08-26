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
    public class Rcon : IDisposable
    {
        public event RconOutput ServerOutput;
        public event RconOutput Errors;
        public event BoolInfo ConnectionSuccess;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Rcon()
        {
            rconSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Reset();

            #if DEBUG
                TempPackets = new ArrayList();
            #endif
        }

        #region IDisposable Members

        bool Disposed;
        public void Dispose()
        {
            if (!Disposed)
            {
                rconSocket.Close();
            }

            Disposed = true;
            disconnected = true;
            connected = false;
        }

        #endregion

        bool disconnected;
        public void Disconnect()
        {
            if (connected)
            {
                connected = false;
                disconnected = true;

                rconSocket.Disconnect(false);
            }
            else
            {
                OnError(MessageCode.CantDisconnectIfNotConnected, null);
            }
        }

        /// <summary>
        /// Attempts to connect to server.
        /// </summary>
        /// <param name="Server">The IPEndpoint of the server to contact.</param>
        /// <param name="password">RCON password.</param>
        public void Connect(IPEndPoint Server, string password)
        {
            if(Disposed)
            {
                OnError(MessageCode.ConnectionFailed, "Already disposed");
                return;
            }

            if (disconnected)
            {
                OnError(MessageCode.ConnectionFailed, "Previously disconnected");
                return;
            }

            try
            {
                rconSocket.Connect(Server);
            }
            catch (SocketException)
            {
                OnError(MessageCode.ConnectionFailed, null);
                OnConnectionSuccess(false);
                return;
            }

            Reset();

            RCONPacket ServerAuthPacket = new RCONPacket();

            ++RequestIDCounter;
            ServerAuthPacket.RequestId = RequestIDCounter;

            ServerAuthPacket.String1 = password;
            ServerAuthPacket.ServerDataSent = RCONPacket.SERVERDATA_sent.SERVERDATA_AUTH;

            SendRCONPacket(ServerAuthPacket);

            //Start the listening loop, now that we've sent auth packet, we should be expecting a reply.
            GetNewPacketFromServer();
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
            else
            {
                OnError(MessageCode.SendCommandsWhenConnected, null);
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

        // End of public interface.
        // The guts start here.

        Socket rconSocket;
        int RequestIDCounter;
        int PacketCount;

        #if DEBUG
            public ArrayList TempPackets;
        #endif

        void Reset()
        {
            PacketCount = 0;
            RequestIDCounter = 0;
        }

        void SendRCONPacket(RCONPacket p)
        {
            byte[] Packet = p.OutputAsBytes();
            try
            {
                rconSocket.BeginSend(Packet, 0, Packet.Length, SocketFlags.None, new AsyncCallback(SendCallback), this);
            }
            catch(SocketException se)
            {
                OnError(MessageCode.ConnectionClosed, se.Message);
                Disconnect();
            }
        }

        bool connected;
        public bool Connected
        {
            get { return connected; }
        }

        void SendCallback(IAsyncResult ar)
        {
            try
            {
                rconSocket.EndSend(ar);
            }
            catch (SocketException se)
            {
                OnError(MessageCode.ConnectionClosed, se.Message);
                Disconnect();
            }
        }

        void GetNewPacketFromServer()
        {
            // Prepare the state information for a new packet.
            PacketState state = new PacketState();
            state.IsPacketLength = true;
            state.Data = new byte[4];
            state.PacketCount = PacketCount;
            PacketCount++;

            // If we're debugging, log the packetstate.
            // Can use this to trace the packets later.
            #if DEBUG
			    TempPackets.Add(state);
            #endif

            try
            {
                rconSocket.BeginReceive(state.Data, 0, 4, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            catch (SocketException se)
            {
                OnError(MessageCode.ConnectionFailed, se.Message);
                Disconnect();
            }
        }

        void ReceiveCallback(IAsyncResult ar)
        {
            PacketState state = null;

            try
            {
                int bytesreceived = rconSocket.EndReceive(ar);
                state = (PacketState)ar.AsyncState;
                state.BytesSoFar += bytesreceived;

#if DEBUG
                Console.WriteLine("Receive Callback. Packet: {0} First packet: {1}, Bytes so far: {2}",
                                    state.PacketCount, state.IsPacketLength, state.BytesSoFar);
#endif

                // Spin the processing of this data off into another thread.
                ThreadPool.QueueUserWorkItem((object pool_state) =>
                {
                    ProcessIncomingData(state);
                });

            }
            catch (SocketException se)
            {
                OnError(MessageCode.ConnectionClosed, se.Message);
                Disconnect();
            }
            catch (ObjectDisposedException ode)
            {
                OnError(MessageCode.AlreadyDisposed, ode.Message);
            }
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

                if (state.PacketLength > 0)
                {
                    StartToReceive(state);
                }
                else
                {
                    OnError(MessageCode.EmptyPacket, null);
                    // Treat as a fatal error?
                    Disconnect();
                }
            }
            else
            {
                // This is a fragment of a complete packet.
                if (state.BytesSoFar < state.PacketLength)
                {
                    // We don't have all the data, ask the network for the rest.
                    StartToReceive(state);
                }
                else
                {
                    // This is the whole packet, so we can go ahead and pack it up into a structure and then punt it upstairs.
                    #if DEBUG
					    Console.WriteLine("Complete packet.");
                    #endif

                    RCONPacket ReturnedPacket = new RCONPacket();
                    ReturnedPacket.ParseFromBytes(state.Data, this);

                    ThreadPool.QueueUserWorkItem((object pool_state) =>
                    {
                        ProcessResponse(ReturnedPacket);
                    });

                    // Wait for new packet.
                    GetNewPacketFromServer();
                }
            }
        }

        void StartToReceive(PacketState state)
        {
            try
            {
                rconSocket.BeginReceive(state.Data, state.BytesSoFar, state.PacketLength - state.BytesSoFar, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            catch (SocketException se)
            {
                OnError(MessageCode.ConnectionClosed, se.Message);
                Disconnect();
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