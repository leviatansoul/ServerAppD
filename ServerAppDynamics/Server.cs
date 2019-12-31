using System;
using System.Collections.Generic;
using System.Text;

//For obtaining directories and files
using System.IO;
using System.Text.RegularExpressions;


// A C# Program for Server 
using System.Net;
using System.Net.Sockets;

namespace ServerAppDynamics
{
    public sealed class Server
    {
        private static Server instance = null;
        private static readonly object padlock = new object();

        Server()
        {
        }

        public static Server Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Server();                 

                    }
                    Console.WriteLine("ID: " + instance.GetHashCode());
                    return instance;
                }
            }
        }

        // Main Method 
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    
                    Console.WriteLine("Server starting");
                    Server.Instance.ExecuteServer();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Main Excepction: "+e.ToString());
                    continue;
                }
            }
            
            

        }

        private bool isValidPath(String path)
        {
            if (path.Equals("/") || path.Equals("\\")) return true;

            return Regex.IsMatch(path, @"^\/([A-z0-9-_+]+\/)*([A-z0-9]+)$"); //Check if path are in correct format. Example: /home/dir

        }

        private void ExecuteServer()
        {
            // Establish the local endpoint 
            // for the socket. Dns.GetHostName 
            // returns the name of the host 
            // running the application. 
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

            // Creation TCP/IP Socket using 
            // Socket Class Costructor 
            Socket listener = new Socket(ipAddr.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

            Socket clientSocket = null;

            try
            {

                // Using Bind() method we associate a 
                // network address to the Server Socket 
                // All client that will connect to this 
                // Server Socket must know this network 
                // Address 
                listener.Bind(localEndPoint);

                // Using Listen() method we create 
                // the Client list that will want 
                // to connect to Server 
                listener.Listen(10);

               
                while (true)
                {

                    Console.WriteLine("Waiting connection ... ");

                    // Suspend while waiting for 
                    // incoming connection Using 
                    // Accept() method the server 
                    // will accept connection of client 
                    clientSocket = listener.Accept();

                    // Data buffer 
                    byte[] bytes = new Byte[1024];
                    string data = null;
                    string[] args = null;
                    byte[] message = null;

                    while (true)
                    {

                        int numByte = clientSocket.Receive(bytes);

                        data += Encoding.ASCII.GetString(bytes,
                                                0, numByte);

                        if (data.IndexOf("<EOF>") > -1)
                        {
                            data = data.Substring(0, data.Length - 5);
                            args = data.Split(' ');
                            break;
                        }

                    }

                    Console.WriteLine("Text received -> {0} ", data);
                    Console.WriteLine("Args: " + string.Join(",", args));


                    switch (args[0])
                    {
                        case "HELLO":
                            message = Encoding.ASCII.GetBytes("Hi");
                            break;

                        case "TIME":
                            DateTime utctime = DateTime.UtcNow;
                            utctime.ToString();
                            message = Encoding.ASCII.GetBytes(utctime.ToString());
                            break;

                        case "DIR":
                            string path = args.Length == 1 ? "\\" : args[1];
                            if (isValidPath(path))
                            {
                                if (Directory.Exists(Directory.GetCurrentDirectory() + path))
                                {
                                    message = Encoding.ASCII.GetBytes("DIR.");
                                    Console.WriteLine(Directory.GetCurrentDirectory() + path);
                                    String[] s = Directory.GetFiles(Directory.GetCurrentDirectory() + path);
                                    String[] p = Directory.GetDirectories(Directory.GetCurrentDirectory() + path);
                                    Console.WriteLine(String.Join(",", s));
                                    Console.WriteLine(String.Join(",", p));
                                }
                                else
                                {
                                    message = Encoding.ASCII.GetBytes("Path directory does not exit. Try a new one.");
                                }

                            }
                            else
                            {
                                message = Encoding.ASCII.GetBytes("Path error");
                            }

                            break;
                        default:
                            message = Encoding.ASCII.GetBytes("Command not valid. Try with HELLO, TIME or DIR.");
                            break;
                    }


                    // Send a message to Client 
                    // using Send() method 
                    clientSocket.Send(message);

                    // Close client Socket using the 
                    // Close() method. After closing, 
                    // we can use the closed Socket 
                    // for a new Client Connection 
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("ExecutionServer Exception: "+e.ToString());

                // Close all sockect connections
                if(listener != null)
                {
                    listener.Close();
                }
                
                if(clientSocket != null)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
                
                
            }
        }
    }
}
