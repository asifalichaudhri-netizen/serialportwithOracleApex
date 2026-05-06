using System;
using System.IO.Ports;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;

namespace SerialPortReader
{
    internal class Program
    {
        private static TcpListener _tcpListener;
        private static SerialPort _serialPort;
        private static string _connectionString = "User Id=cust_kl;Password=custklatkamal;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=172.16.1.9)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=prod)));";

        static void Main(string[] args)
        {
            StartTcpListener();
            // Keep the application running in the background
            Thread.Sleep(Timeout.Infinite);
        }

        private static void StartTcpListener()
        {
            _tcpListener = new TcpListener(IPAddress.Any, 12345);
            _tcpListener.Start();

            Thread listenerThread = new Thread(ListenForClients);
            listenerThread.Start();
        }

        private static void ListenForClients()
        {
            while (true)
            {
                TcpClient client = _tcpListener.AcceptTcpClient();
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }

        private static void HandleClient(object clientObj)
        {
            try
            {
                TcpClient client = (TcpClient)clientObj;
                LogEvent("Client connected...");

                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                

                if (bytesRead > 0)
                {
                    // Convert received data to string
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    LogEvent($"Raw request received: {receivedData}");

                    // Extract POST data from HTTP request
                    if (receivedData.StartsWith("POST"))
                    {
                        string[] requestParts = receivedData.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None);
                        if (requestParts.Length > 1)
                        {
                            string postData = requestParts[1].Trim(); // POST data is after the headers
                            LogEvent($"POST data received: {postData}");

                            // Read serial data from the port
                            string serialData = ReadSerialPortData();

                            // Insert serial data into Oracle if received
                            if (!string.IsNullOrEmpty(serialData))
                            {
                                InsertDataIntoOracle(postData, serialData);
                                LogEvent($"Serial data inserted into Oracle table: {serialData}");
                            }
                            else
                            {
                                LogEvent("No serial data received.");
                            }

                            // Respond to the client after processing
                            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nData received and processed.";
                            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                            stream.Write(responseBytes, 0, responseBytes.Length);
                        }
                        else
                        {
                            LogEvent("No POST data found in request.");
                        }
                    }
                    else
                    {
                        LogEvent("Invalid HTTP request received.");
                    }
                }
                else
                {
                    LogEvent("No data received from the client.");
                }

                // Close the client connection
                client.Close();
                LogEvent("Client connection closed.");
            }
            catch (Exception ex)
            {
                LogEvent($"Error handling client: {ex.Message}");
            }
        }




        private static string ReadSerialPortData()
        {
            string serialData = string.Empty;
            int attempts = 0;
            const int maxAttempts = 5; // Retry up to 5 times

            try
            {
                LogEvent("Initializing Serial Port COM1...");

                _serialPort = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One)
                {
                    NewLine = "\r\n",
                    Encoding = Encoding.ASCII
                };

                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    LogEvent("Serial port opened...");
                }

                while (attempts < maxAttempts)
                {
                    LogEvent($"Reading serial data, attempt {attempts + 1}...");
                    serialData = _serialPort.ReadExisting().Trim(); // Read data from serial port

                    if (!string.IsNullOrEmpty(serialData))
                    {
                        LogEvent($"Raw data received: {serialData}");

                        // Extract first valid numeric value using regex
                        Match match = Regex.Match(serialData, @"\d+\.\d+");
                        if (match.Success)
                        {
                            serialData = match.Value; // Get first valid number
                            LogEvent($"Cleaned serial data: {serialData}");
                            _serialPort.Close();
                            return serialData;
                        }
                        else
                        {
                            LogEvent("No valid numeric data found.");
                        }
                    }

                    LogEvent("No data received, retrying...");
                    Thread.Sleep(1000);
                    attempts++;
                }

                LogEvent("Failed to receive serial data after maximum retries.");
            }
            catch (Exception ex)
            {
                LogEvent($"Error in ReadSerialPortData: {ex.Message}");
            }
            finally
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                    LogEvent("Serial port closed.");
                }
            }

            return serialData; // Return empty if no valid data received
        }




        private static void InsertDataIntoOracle(string data1,string data)
        {
            try
            {
                using (OracleConnection connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO cust_kl.kl_serial_data_log (barcode_no,s_data, creation_date,terminal_ip) VALUES (:data1,:data, sysdate,:data2)";
                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("data1", data1));
                        command.Parameters.Add(new OracleParameter("data", data));
                        command.Parameters.Add(new OracleParameter("data2", Environment.MachineName));
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception) { /* Handle exception or log */ }
        }

        private static void LogEvent(string message)
        {
            try
            {
                string logFilePath = @"C:\Logs\SerialPortReader.log"; // Ensure this directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)); // Ensure directory exists

                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // If logging fails, write to Windows Event Log (optional)
                System.Diagnostics.EventLog.WriteEntry("SerialPortReader", $"Logging failed: {ex.Message}", System.Diagnostics.EventLogEntryType.Error);
            }
        }

    }
}
