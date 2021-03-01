using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TicTacToeTah
{
    public enum CommunicationState
    {
        Connected,
        Disconnected,
        ErrorConnecting,
        MessageRecieved
    }

    public delegate void CommunicationResult(CommunicationState state, string additional);


    public class Communication
    {
        private readonly int port = 65500;


        private Socket listeningSocket; //the listening socket
        private Socket connectedSocket; //the socket once connected

        private readonly byte[] dataBuffer; //the byte array

        private bool isConnected;
        private readonly string myIPAddress;

        private bool isHost;

        private NetworkStream networkStream; //the network stream gotten by using the socket
        private StreamReader inputStream; //read from the network stream
        private StreamWriter outputStream; //write to the network stream


        public CommunicationResult NotifyCommunication;

        public Communication()
        {
            dataBuffer = new byte[4096];
            myIPAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
        }


        public string MyIP
        {
            get { return myIPAddress; }
        }

        public bool IsHost
        {
            get { return isHost; }
        }


        public bool IsConnected
        {
            get { return isConnected; }
        }

        public void ConnectToHost(string ip)
        {
            try
            {
                //IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                isHost = false;
                IPAddress ipAddress = Dns.GetHostEntry(ip).AddressList[0]; //get ipaddress object by resolving host
                //IPAddress ipAddress = Dns.Resolve(ip).AddressList[0];//get ipaddress object by resolving host
                IPEndPoint ipConnect = new IPEndPoint(ipAddress, port);
                connectedSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connectedSocket.BeginConnect(ipConnect, OnConnect, null);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }

        private void OnConnect(IAsyncResult asyncResult)
        {
            try
            {
                connectedSocket.EndConnect(asyncResult);
                networkStream = new NetworkStream(connectedSocket); //get the stream from the tcp connection
                inputStream = new StreamReader(networkStream); //use streamreader to read from network stream
                outputStream = new StreamWriter(networkStream); //use streamwriter to write to network stream
                isConnected = true;

                NotifyCommunication(CommunicationState.Connected, null);
                networkStream.BeginRead(dataBuffer, 0, dataBuffer.Length, OnRead, null);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }


        public void ListenForConnection()
        {
            try
            {
                isHost = true;

                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, port); //accept any ip on port 65500
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //make the socket
                listeningSocket.Bind(ipLocal); //bind ip address range to socket
                listeningSocket.Listen(4); //listen for connections
                listeningSocket.BeginAccept(OnHost, null);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }

        private void OnHost(IAsyncResult asyncResult)
        {
            try
            {
                connectedSocket = listeningSocket.EndAccept(asyncResult);
                networkStream = new NetworkStream(connectedSocket); //get the stream from the tcp connection
                inputStream = new StreamReader(networkStream); //use streamreader to read from network stream
                outputStream = new StreamWriter(networkStream); //use streamwriter to write to network stream
                listeningSocket.Close();
                isConnected = true;

                NotifyCommunication(CommunicationState.Connected, null);
                networkStream.BeginRead(dataBuffer, 0, dataBuffer.Length, OnRead, null);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }


        public void WriteMessage(string message)
        {
            try
            {
                outputStream.WriteLine(message);
                outputStream.Flush();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }


        private void OnRead(IAsyncResult asyncResult)
        {
            try
            {
                int numberBytes = networkStream.EndRead(asyncResult);

                if (numberBytes > 0)
                {
                    string message = Encoding.ASCII.GetString(dataBuffer, 0, numberBytes);
                    NotifyCommunication(CommunicationState.MessageRecieved, message);
                }

                networkStream.BeginRead(dataBuffer, 0, dataBuffer.Length, OnRead, null);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }

        public void Disconnect()
        {
            if (connectedSocket != null)
            {
                connectedSocket.Close();
                isConnected = false;
                NotifyCommunication(CommunicationState.Disconnected, null);
            }
        }

        ~Communication()
        {
            Dispose(true);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                //close all streams
                isConnected = false;
                try
                {
                    if (inputStream != null)
                        inputStream.Close();
                    if (outputStream != null)
                        outputStream.Close();
                    if (networkStream != null)
                        networkStream.Close();
                    if (connectedSocket != null)
                    {
                        connectedSocket.Close();
                    }
                    if (listeningSocket != null)
                    {
                        listeningSocket.Close();
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
            }
        }
    }
}