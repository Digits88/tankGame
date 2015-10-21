using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace TankGame
{
    class Client
    {
        static void Main(string[] args)
        {
            StartConnection();
            Console.ReadLine();
        }

        public static void StartConnection()
        {
            NetworkStream net = null;
            TcpClient conn = null;
            String msg = null;
            try
            {
                Console.WriteLine(" StartCommunication() method is calling.........");
                String command = "JOIN#";
                conn = new TcpClient();
                // Connects the client to a remote TCP host using the specified host name and port number
                conn.Connect("localhost", 6000);
                //Returns the NetworkStream used to send and receive data
                net = conn.GetStream();
                ASCIIEncoding encode = new ASCIIEncoding();
                byte[] bytes = encode.GetBytes(command);
                net.Write(bytes, 0, bytes.Length);
                Console.WriteLine("\n Join Command Sent To The Server");

            }
            catch (Exception error)
            {
                Console.WriteLine(" Error Message: " + error.StackTrace);
            }
            finally
            {
                if (net != null)
                {
                    net.Close();
                    //handleMessages
                    TcpListener server_listner = new TcpListener(IPAddress.Any, 7000);
                    server_listner.Start();
                    Byte[] bytes = new Byte[100];


                    while (true)
                    {
                        TcpClient Server = server_listner.AcceptTcpClient();
                        NetworkStream stream = Server.GetStream();
                        msg = System.Text.Encoding.ASCII.GetString(bytes, 0, stream.Read(bytes, 0, bytes.Length));
                        Console.WriteLine(msg + "\n");
                        EncodeMsg(msg);
                        Server.Close();
                        stream.Close();
                    }
                } conn.Close();
            }
        }

        public static void EncodeMsg(String msg)
        {
            //Remove # from msg
            msg = msg.Remove(msg.Length - 1);
            char index = msg[0];
            // Console.WriteLine(index);
            if (index.Equals('I'))
            {
                Console.WriteLine("*******************************************************************\n");
                Console.WriteLine("Game Instance Received.......\n");
                String[] parts = msg.Split(':');
                Console.WriteLine("Player Is: " + parts[1] + "\n");
                //Get brick co-ordinates
                String type = null;
                for (int l = 1; l < 4; l++)
                {
                    if (l == 1) { type = "Brick"; }
                    else if (l == 2) { type = "Stone"; }
                    else if (l == 3) { type = "Water"; }
                    Console.WriteLine(type + " Co-ordinates are........." + "\n");
                    parts[l + 1] = parts[l + 1].Replace(',', ';');
                    String[] bricks = parts[l + 1].Split(';');
                    String[] BrickX = new String[bricks.Length / 2];
                    String[] BrickY = new String[bricks.Length / 2];

                    int j = 0, k = 0;

                    for (int i = 0; i < bricks.Length; i = i + 2)
                    {

                        BrickX[j] = bricks[i];
                        j++;
                    }
                    for (int i = 1; i < bricks.Length; i = i + 2)
                    {
                        if (bricks[i] != null)
                        {
                            BrickY[k] = bricks[i];
                            k++;
                        }
                    }
                    for (int i = 0; i < bricks.Length / 2; i++)
                    {
                        Console.WriteLine(type + " " + (i + 1) + ": " + "X--> " + BrickX[i] + "   Y--> " + BrickY[i]);
                    }
                    Console.WriteLine("\n");
                }
                Console.WriteLine("*******************************************************************\n");
            }
            else if (index.Equals('S'))
            {
                Console.WriteLine("*******************************************************************\n");
                Console.WriteLine("Your Details Received.......\n");
                String[] parts = msg.Split(';');
                //parts[0] = parts[0].Remove(0);
                //Console.WriteLine("Player Number Is: " + parts[0]+"\n");
                String[] cor = parts[1].Split(',');
                Console.WriteLine("Your Position Cordinates Are: " + "X--> " + cor[0] + " Y--> " + cor[1] + "\n");
                String dir = null;
                if (parts[2].Equals("0")) { dir = "North"; }
                else if (parts[2].Equals("1")) { dir = "East"; }
                else if (parts[2].Equals("2")) { dir = "South"; }
                else if (parts[2].Equals("3")) { dir = "West"; }
                Console.WriteLine("Your Direction Is: " + dir + "\n");
                Console.WriteLine("*******************************************************************\n");
            }
            else if (index.Equals('C'))
            {
                Console.WriteLine("*******************************************************************\n");
                Console.WriteLine("Guys...Coins Appeared.......\n");
                String[] parts = msg.Split(':');
                String[] cor = parts[1].Split(',');
                Console.WriteLine("Coin Cordinates Are: " + "X--> " + cor[0] + " Y--> " + cor[1] + "\n");
                Console.WriteLine("Time For The Coins to Disappear: " + parts[2] + "\n");
                Console.WriteLine("Value Of The Coins: " + parts[3] + "\n");
                Console.WriteLine("*******************************************************************\n");
            }
            else if (index.Equals('L'))
            {
                Console.WriteLine("*******************************************************************\n");
                Console.WriteLine("Guys...Life Pack Appeared.........\n");
                String[] parts = msg.Split(':');
                String[] cor = parts[1].Split(',');
                Console.WriteLine("Life Pack Cordinates Are: " + "X--> " + cor[0] + " Y--> " + cor[1] + "\n");
                Console.WriteLine("Time For The Life Pack to Disappear: " + parts[2] + "\n");
                Console.WriteLine("*******************************************************************\n");
            }
            else if (index.Equals('G'))
            {
                Console.WriteLine("*******************************************************************\n");
                Console.WriteLine("Global updates received.........\n");
                String[] parts = msg.Split(':');
                String[][] Updates = new String[parts.Length - 2][];
                for (int i = 1; i < parts.Length - 1; i++)
                {
                    parts[i] = parts[i].Replace(';', ',');

                }
                for (int i = 0; i < parts.Length - 2; i++)
                {
                    String[] cor = parts[i + 1].Split(',');
                    Updates[i] = cor;
                }
                String dir = null;
                for (int i = 0; i < Updates.Length; i++)
                {
                    Console.WriteLine("Player: " + Updates[i][0]);
                    Console.WriteLine("X Coordinate--> " + Updates[i][1]);
                    Console.WriteLine("Y Coordinate--> " + Updates[i][2]);
                    if (Updates[i][3].Equals("0")) { dir = "North"; }
                    else if (Updates[i][3].Equals("1")) { dir = "East"; }
                    else if (Updates[i][3].Equals("2")) { dir = "South"; }
                    else if (Updates[i][3].Equals("3")) { dir = "West"; }
                    Console.WriteLine("Direction: " + dir);
                    Console.WriteLine("Whether Shot: " + Updates[i][4]);
                    Console.WriteLine("Health: " + Updates[i][5]);
                    Console.WriteLine("Coins: " + Updates[i][6]);
                    Console.WriteLine("Point: " + Updates[i][7] + "\n");
                }
            }
        }
    }
}
