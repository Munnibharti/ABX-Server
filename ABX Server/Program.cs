using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        const string server = "127.0.0.1";
        const int port = 3000;

        try
        {
            // Connect to the server
            using (TcpClient client = new TcpClient(server, port))
            using (NetworkStream stream = client.GetStream())
            {
                // Send "Stream All Packets" request
                byte[] request = new byte[] { 1, 0 }; // callType = 1, resendSeq = 0
                stream.Write(request, 0, request.Length);

                // Receive response
                List<Packet> packets = new List<Packet>();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Parse packets from the buffer
                    packets.AddRange(ParsePackets(buffer, bytesRead));
                }

                // Log the number of packets received
                Console.WriteLine($"Received {packets.Count} packets.");

                // Handle missing sequences after the server stops sending data
                packets = HandleMissingSequences(packets, server, port);

                // Write packets to JSON file
                File.WriteAllText("C:\\output.json", JsonConvert.SerializeObject(packets, Formatting.Indented));
                Console.WriteLine("Data saved to output.json");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static List<Packet> ParsePackets(byte[] buffer, int bytesRead)
    {
        List<Packet> packets = new List<Packet>();
        int offset = 0;

        while (offset + 17 <= bytesRead) // Each packet is 17 bytes
        {
            string symbol = Encoding.ASCII.GetString(buffer, offset, 4);
            offset += 4;

            string buySellIndicator = Encoding.ASCII.GetString(buffer, offset, 1);
            offset += 1;

            int quantity = BitConverter.ToInt32(buffer.Skip(offset).Take(4).Reverse().ToArray(), 0);
            offset += 4;

            int price = BitConverter.ToInt32(buffer.Skip(offset).Take(4).Reverse().ToArray(), 0);
            offset += 4;

            int packetSequence = BitConverter.ToInt32(buffer.Skip(offset).Take(4).Reverse().ToArray(), 0);
            offset += 4;

            var packet = new Packet
            {
                Symbol = symbol,
                BuySellIndicator = buySellIndicator,
                Quantity = quantity,
                Price = price,
                PacketSequence = packetSequence
            };
            packets.Add(packet);

            // Log the parsed packet
            Console.WriteLine($"Packet Received: {JsonConvert.SerializeObject(packet)}");
        }

        return packets;
    }

    static List<Packet> HandleMissingSequences(List<Packet> packets, string server, int port)
    {
        packets.Sort((a, b) => a.PacketSequence.CompareTo(b.PacketSequence));
        List<int> missingSequences = new List<int>();

        // Identify missing sequences
        int lastSequence = packets[^1].PacketSequence; // Last packet is never missed
        for (int i = 1; i <= lastSequence; i++)
        {
            if (!packets.Exists(p => p.PacketSequence == i))
            {
                missingSequences.Add(i);
            }
        }

        // Log the missing sequences
        Console.WriteLine($"Missing Sequences: {string.Join(", ", missingSequences)}");

        // Request missing packets
        foreach (int seq in missingSequences)
        {
            bool success = RequestMissingPacket(seq, server, port, packets);
            if (!success)
            {
                Console.WriteLine($"Failed to retrieve packet {seq} after retries.");
            }
        }

        return packets;
    }

    static bool RequestMissingPacket(int sequence, string server, int port, List<Packet> packets)
    {
        const int maxRetries = 3;
        int retries = 0;

        while (retries < maxRetries)
        {
            try
            {
                using (TcpClient client = new TcpClient(server, port))
                using (NetworkStream stream = client.GetStream())
                {
                    // Send "Resend Packet" request
                    byte[] resendRequest = new byte[] { 2, (byte)sequence };
                    stream.Write(resendRequest, 0, resendRequest.Length);

                    // Receive the response
                    byte[] buffer = new byte[17]; // Each packet is 17 bytes
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        var receivedPackets = ParsePackets(buffer, bytesRead);
                        packets.AddRange(receivedPackets);
                        return true; // Successfully received the packet
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error requesting packet {sequence}: {ex.Message}");
            }

            retries++;
            Console.WriteLine($"Retrying for packet {sequence} ({retries}/{maxRetries})...");
        }

        return false; // Failed to retrieve the packet after retries
    }

    class Packet
    {
        public string Symbol { get; set; }
        public string BuySellIndicator { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int PacketSequence { get; set; }
    }
}
