using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using Microsoft.Extensions.Configuration; 

namespace Gaze_Point.Connection
{

    /// <summary>
    /// Manages TCP/IP communication with the Gazepoint eye-tracker.
    /// Handles the connestion lifecycle, command transiìmission and buffered data reconstruction using a line-terminated protocol.
    /// </summary>
    /// <remarks>
    /// This implementation follows the Gazepoint API protocol (TCP/IP XML/String).
    /// </remarks>
    /// <author>Agnese Pinto</author>
    

    public class GPClient
    {

        private readonly string IpAddress;
        private readonly int IpPort;
        private readonly byte[] _buffer;

        private TcpClient _client;
        private NetworkStream _stream;
        private readonly StringBuilder _dataAccumulator = new StringBuilder();


        public GPClient()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings/Connection.json", optional: false, reloadOnChange: true)
                    .Build();

                IpAddress = configuration["IpAddress"];
                IpPort = int.Parse(configuration["IpPort"]);
                int _bufferSize = int.Parse(configuration["BufferSize"]);
                _buffer = new byte[_bufferSize];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Network configuration error: " + ex.Message);
                
                // Fallback
                IpAddress = "127.0.0.1";
                IpPort = 4242;
                _buffer = new byte[4096];
            }
        }


        public bool IsConnected
        {
            get
            {
                if (_client != null)        
                {
                    return _client.Connected;       
                }
                return false;           
            }
        }


        public void Connect()
        {
            try
            {
                if (!IsConnected)            
                {
                    _client = new TcpClient(IpAddress, IpPort);         
                    _stream = _client.GetStream();                      

                    Console.WriteLine("Successfully connected to the server.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection failed: " + ex.Message);
            }

        }


        /// <summary>
        /// Sends a command to the Gazepoint server, automatically appending the required line terminator and encoding it as ASCII.
        /// </summary>
        /// <param name="command">The XML command string to be sent (without line terminators).</param>

        public void SendCommand(string command)
        {
            if (IsConnected)
            {
                string formattedCommand = command + "\r\n";     
                byte[] data = Encoding.ASCII.GetBytes(formattedCommand);    
                _stream.Write(data, 0, data.Length);         
            }
        }


        /// <summary>
        /// Retrieves avaible data from the network stream and extracts complete messages using an internal buffer.
        /// </summary>
        /// <returns>A list of complete XML strings, separated by line terminators</returns>
        /// <remarks>
        /// Handles partial message reception by accumulating fragments until a full terminator (\r|n) is detected.
        /// </remarks>
        
        public List<string> ReadData()
        {
            List<string> packets = new List<string>();

            if (IsConnected && _stream.DataAvailable)
            {                
                int bytesRead = _stream.Read(_buffer, 0, _buffer.Length);               
                string rawChunk = Encoding.ASCII.GetString(_buffer, 0, bytesRead);      
                _dataAccumulator.Append(rawChunk);                                                      
                string currentContent = _dataAccumulator.ToString();         
                int terminatorIndex;

                while ((terminatorIndex = currentContent.IndexOf("\r\n")) != -1)      
                {
                    string packet = currentContent.Substring(0, terminatorIndex);
                    if (!string.IsNullOrEmpty(packet))
                    {
                        packets.Add(packet);
                    }
                    _dataAccumulator.Remove(0, terminatorIndex + 2);
                    currentContent = _dataAccumulator.ToString();
                }
            }
            return packets;
        }


        public void Disconnect()
        {
            if (_stream != null)
            {
                _stream.Close();
            }

            if (_client != null)
            {
                _client.Close();
            }
            Console.WriteLine("Connection closed successfully.");
        }
    }
}
