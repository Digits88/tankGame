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
        //public static void Main(String[] args)
        //{
        //    StartCommunication();
        //    Console.ReadLine();
        //}


        public static void StartCommunication()
        {

            try
            {
                TcpClient serv = new TcpClient();
                serv.Connect("localhost", 6000);
                String str = "JOIN#";
                NetworkStream stm = serv.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(str);
                stm.Write(ba, 0, ba.Length);
                Console.WriteLine("\nSent JOIN#");

                stm.Close();
                handleMessage();
                serv.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }

        }

        private static void handleMessage()
        {
            TcpListener serverListner = new TcpListener(IPAddress.Any, 7000);
            serverListner.Start();
            Byte[] bytes = new Byte[1024];
            String data;

            //listening loop;
            while (true)
            {
                TcpClient gameServer = serverListner.AcceptTcpClient();
                data = null;
                NetworkStream stream = gameServer.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);  //Encode Byte into a String
                    //String[] spltted = data.Split(new Char[] { ':' });


                    formatMsg(data);
                    //  System.Console.WriteLine(data);
                    // engine.ProcessData(spltted);
                }
                gameServer.Close();
                stream.Close();
            }

        }

        public static void formatMsg(String msg)
        {
            String[] firstpart = msg.Split('#');
            String[] parts = firstpart[0].Split(':');

            try
            {
                String msg_format = parts[0];
                if (msg_format.Equals("I")) // game instant received
                {
                    Console.WriteLine("================================================================================\n");
                    Console.WriteLine("New Game Instant received");
                    String player = parts[1];
                    Console.WriteLine("Player: " + player);

                    // reading brick position details
                    Console.WriteLine("Bricks -------------------------------------------");
                    String brick_map = parts[2];
                    String[] bricks = brick_map.Split(';');
                    int brick_no = 1;
                    foreach (String brick in bricks)
                    {
                        String[] brick_location = brick.Split(',');
                        Console.WriteLine("Brick No. " + brick_no + " location ==>  X = " + brick_location[0] + " Y = " + brick_location[1]);
                        brick_no++;
                    }
                    // end reading brick posiotion details


                    // reading stone position details

                    Console.WriteLine("Stones --------------------------------------------");
                    String stone_map = parts[3];
                    String[] stones = stone_map.Split(';');
                    int stone_no = 1;
                    foreach (String stone in stones)
                    {
                        String[] stone_location = stone.Split(',');
                        Console.WriteLine("Stone No. " + stone_no + " location ==>  X = " + stone_location[0] + " Y = " + stone_location[1]);
                        stone_no++;
                    }

                    // end of reading stone position details


                    // reading water position details

                    Console.WriteLine("Water --------------------------------------------");
                    String water_map = parts[4];
                    String[] waters = water_map.Split(';');
                    int water_no = 1;
                    foreach (String water in waters)
                    {
                        String[] water_location = water.Split(',');
                        Console.WriteLine("Water No. " + water_no + " location ==>  X = " + water_location[0] + " Y = " + water_location[1]);
                        water_no++;
                    }

                    // end of reading water position details

                }
                else if (msg_format.Equals("G")) // global update received
                {
                    Console.WriteLine("================================================================================\n");
                    Console.WriteLine("New global Update received");
                    int player_no = 1;
                    for (player_no = 1; player_no <= 5; player_no++)
                    {
                        String player_code = parts[player_no];
                        if (player_code.Substring(0, 1).Equals("P")) // this is a player sub string
                        {
                            String[] player_details = player_code.Split(';');
                            Console.WriteLine("Player Details -----------------------------");
                            Console.WriteLine("Player name: " + player_details[0]);
                            String[] player_log = player_details[1].Split(',');
                            Console.WriteLine("X ==> " + player_log[0] + " y ==> " + player_log[1]);
                            Console.WriteLine("Direction ==> " + player_details[2]);
                            Console.WriteLine("Whehter shot ==> " + player_details[3]);
                            Console.WriteLine("Health ==> " + player_details[4]);
                            Console.WriteLine("Coins ==> " + player_details[5]);
                            Console.WriteLine("Point ==> " + player_details[6]);
                        }
                        else
                            break;
                    }
                    Console.WriteLine("================================================================================\n");
                    Console.WriteLine("Moving shot details");
                    //  Console.WriteLine(parts[player_no]);
                    String[] shots = parts[player_no].Split(';');
                    foreach (String shot in shots)
                    {
                        String[] shot_details = shot.Split(',');
                        Console.WriteLine("Shot details ####  x==> " + shot_details[0] + " y ==> " + shot_details[1] + " damage level ==> " + shot_details[2]);
                    }




                }
                else if (msg_format.Equals("C")) // coin detail received
                {
                    Console.WriteLine("================================================================================\n");
                    Console.WriteLine("Coin resource received");
                    String[] location = parts[1].Split(',');
                    Console.WriteLine("Location x ==> " + location[0] + " y ==> " + location[1]);
                    Console.WriteLine("Time to disappear ==> " + parts[2]);
                    Console.WriteLine("Value of coin ==> " + parts[3]);
                }
                else if (msg_format.Equals("L")) // coin detail received
                {
                    Console.WriteLine("================================================================================\n");
                    Console.WriteLine("Life Pack received");
                    String[] location = parts[1].Split(',');
                    Console.WriteLine("Location x ==> " + location[0] + " y ==> " + location[1]);
                    Console.WriteLine("Time to disappear ==> " + parts[2]);

                }
            }
            catch (Exception e)
            {

            }

            /*foreach (String part in parts)
            {
                Console.WriteLine(part);
            }*/
        }

    }
}
