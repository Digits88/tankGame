using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Timers;
using System.Text;

namespace TankGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    public class ServerMessageArgs : EventArgs
    {
        public String msg;
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {

        #region variables
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // global game details
        int block_pixel_size = 40;
        int grid_size = 20;


        #region texture objects
        // declaring game texture objects
        SpriteFont title_f;
        Vector2 title_position;
        String title_text;
        Color title_color;

        // score board title
        SpriteFont score_title;
        Vector2 score_title_position;
        String score_title_text;
        Color score_title_color;

        // player details
        SpriteFont player_1;
        Vector2 player_1_position;
        String player_1_text;
        Color player_1_color;

        SpriteFont player_2;
        Vector2 player_2_position;
        String player_2_text;
        Color player_2_color;

        SpriteFont player_3;
        Vector2 player_3_position;
        String player_3_text;
        Color player_3_color;

        SpriteFont player_4;
        Vector2 player_4_position;
        String player_4_text;
        Color player_4_color;

        #endregion
        // decalring empty block 
        Block[,] blocks;

        // server
        TcpClient serv;
        TcpListener serverListner;
        Byte[] bytes;
        String data;
        String server_ip = "localhost";
        IPAddress client_ip = IPAddress.Any;

        //instants for updating purposes
        private int state = 0;
        System.Timers.Timer _timer;

        // details about our tank
        string player_id;


        // AI related instants
        AI ai;
        System.Timers.Timer _timer_ai;
        int tmpstd = 0;
        public EventHandler<ServerMessageArgs> newMessage;


        #endregion

        public Game1()
        {
            // changing the windows location
            // var form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);
            //form.Location = new System.Drawing.Point(1000, 1000);


            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = grid_size * block_pixel_size + 30;
            graphics.PreferredBackBufferWidth = grid_size * block_pixel_size + 200;
            Content.RootDirectory = "Content";



        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>



        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            #region text objects initializing
            // creating the title
            title_position = new Vector2(0, 0);
            title_text = "                                                                Black Panther ";
            title_color = Color.Black;

            // scoreboard title
            int title_left = 820;
            score_title_color = Color.Black;
            score_title_position = new Vector2(title_left, 100);
            score_title_text = "Score Board";

            // player details

            player_1_color = Color.Black;
            player_1_position = new Vector2(title_left, 240);
            player_1_text = "";

            player_2_color = Color.Black;
            player_2_position = new Vector2(title_left, 280);
            player_2_text = "";

            player_3_color = Color.Black;
            player_3_position = new Vector2(title_left, 320);
            player_3_text = "";

            player_4_color = Color.Black;
            player_4_position = new Vector2(title_left, 360);
            player_4_text = "";




            #endregion


            #region grid

            blocks = new Block[grid_size, grid_size];
            // creating the block
            for (int i = 0; i < grid_size; i++)
            {
                for (int j = 0; j < grid_size; j++)
                {
                    blocks[i, j] = new Block(new Vector2(i * block_pixel_size, j * block_pixel_size + 30));
                }
            }

            #endregion

            #region event handling
            ai = new AI(blocks, grid_size, player_id);
            ai.listenToMessages(this);



            _timer = new System.Timers.Timer(500);
            _timer.Elapsed += new ElapsedEventHandler(run_game);
            _timer.Enabled = true;

            #endregion
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            #region text objects
            // loading the title
            title_f = Content.Load<SpriteFont>("Fonts/title_font");
            score_title = Content.Load<SpriteFont>("Fonts/score_board_title");


            // player strings

            player_1 = Content.Load<SpriteFont>("Fonts/player_1");
            player_2 = Content.Load<SpriteFont>("Fonts/player_2");
            player_3 = Content.Load<SpriteFont>("Fonts/player_3");
            player_4 = Content.Load<SpriteFont>("Fonts/player_4");

            #endregion

            // loading the image of block
            for (int i = 0; i < grid_size; i++)
            {
                for (int j = 0; j < grid_size; j++)
                {
                    blocks[i, j].loadContent(Content);
                }
            }
            startCommunication();


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        public void run_game(object sender, ElapsedEventArgs e)
        {
            handleMessage();
        }
        //deleting previous tank instances
        public void remove_redundant_tanks(String id)
        {
            for (int i = 0; i < grid_size; i++)
            {
                for (int j = 0; j < grid_size; j++)
                {
                    if (blocks[i, j].get_type() == 6) // if it is a tank
                    {
                        if (String.Compare(blocks[i, j].get_tank_id(), id) == 0) // if this block contains the tank we need
                        {
                            blocks[i, j].change_type(0, Content, -100); // change it to an empty block
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here


            base.Update(gameTime);
        }
        // updating the ttls of relevent objects

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(143, 143, 143));

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            #region text objects
            spriteBatch.DrawString(title_f, title_text, title_position, title_color);
            spriteBatch.DrawString(score_title, score_title_text, score_title_position, score_title_color);
            spriteBatch.DrawString(player_1, player_1_text, player_1_position, player_1_color);
            spriteBatch.DrawString(player_2, player_2_text, player_2_position, player_2_color);
            spriteBatch.DrawString(player_3, player_3_text, player_3_position, player_3_color);
            spriteBatch.DrawString(player_4, player_4_text, player_4_position, player_4_color);
            #endregion

            #region grid
            // drawing the grid of blocks
            for (int i = 0; i < grid_size; i++)
            {
                for (int j = 0; j < grid_size; j++)
                {
                    blocks[i, j].draw(spriteBatch);
                }
            }
            #endregion
            spriteBatch.End();
            base.Draw(gameTime);
        }



        #region communication
        // communicating with server
        public void startCommunication()
        {

            try
            {
                serv = new TcpClient();
                serv.Connect(server_ip, 6000);
                String str = "JOIN#";
                NetworkStream stm = serv.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(str);
                stm.Write(ba, 0, ba.Length);
                Console.WriteLine("\nSent JOIN#");

                stm.Close();
                serverListner = new TcpListener(client_ip, 7000);
                serverListner.Start();
                bytes = new Byte[1024];

                //serv.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }

        public void handleMessage()
        {



            //listening loop;
            //while (true)
            //{
            TcpClient gameServer = serverListner.AcceptTcpClient();
            data = null;
            NetworkStream stream = gameServer.GetStream();

            int i;

            // Loop to receive all the data sent by the client.
            try
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);  //Encode Byte into a String
                                                                               //String[] spltted = data.Split(new Char[] { ':' });

                    encodeMsg(data);

                    //  System.Console.WriteLine(data);
                    // engine.ProcessData(spltted);
                }

            }
            catch (Exception e)
            {

            }
            finally
            {
                gameServer.Close();
                stream.Close();
            }


            //}
        }
        #endregion

        #region message encoding
        private void encodeMsg(String msg)
        {
            String[] firstpart = msg.Split('#');
            String[] parts = firstpart[0].Split(':');
            try
            {
                String msg_format = parts[0];
                if (msg_format.Equals("I")) // game instant received
                {
                    game_instant(parts);
                }
                else if (msg_format.Equals("G")) // global update received
                {
                    global_update(parts);
                }
                else if (msg_format.Equals("C")) // coin detail received
                {
                    coin_detail(parts);
                }
                else if (msg_format.Equals("L")) // Life Pack detail received
                {
                    lifePack_detail(parts);
                }
                else if (msg_format.Equals("S")) // acceptance received
                {
                    acceptance(parts);
                }
                else // mooving or shooting state received
                {
                    mooving_shooting(parts);
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }
        private void mooving_shooting(String[] parts)
        {
            // send server msg to the ai
            ServerMessageArgs serverMessageArgs = new ServerMessageArgs();
            serverMessageArgs.msg = parts[0];
            Console.WriteLine(parts[0]);
            newMessage(this, serverMessageArgs);
        }
        private void global_update(String[] parts)
        {

            int player_no = 1;
            for (player_no = 1; player_no <= 5; player_no++)
            {
                String player_code = parts[player_no];
                if (player_code.Substring(0, 1).Equals("P")) // this is a player sub string
                {
                    String[] player_details = player_code.Split(';');
                    String player_id = player_details[0];
                    String[] player_log = player_details[1].Split(',');
                    int direction = int.Parse(player_details[2]);
                    int shotted = int.Parse(player_details[3]);
                    int health = int.Parse(player_details[4]);
                    int coins = int.Parse(player_details[5]);
                    int points = int.Parse(player_details[6]);
                    remove_redundant_tanks(player_id);

                    bool our = false;
                    if (String.Compare(player_id, this.player_id) == 0) // if this is our tank
                    {
                        our = true;
                        player_2_text = "My Coins: " + coins;
                        player_3_text = "My Points: " + points;
                        player_4_text = "My Health: " + health;
                    }

                    blocks[int.Parse(player_log[0]), int.Parse(player_log[1])].change_type(player_id, Content, direction, shotted, health, coins, points, our);

                }
                else
                    break;
            }
            check_life(parts[player_no]);
            //     Console.WriteLine("##### " + parts[player_no]);
        }

        private void check_life(String msg)
        {
            String[] details = msg.Split(';');
            for (int i = 0; i < details.Length; i++) // iterating through bricks
            {
                String[] brick = details[i].Split(',');
                int x = int.Parse(brick[0]);
                int y = int.Parse(brick[1]);
                int life = int.Parse(brick[2]);
                if (life == 4) // if the brick is destroid
                {
                    blocks[x, y].change_type(0, Content, -100); // change it to an empty cell
                }
            }
        }
        private void coin_detail(String[] parts)
        {


            String[] location_str = parts[1].Split(',');
            blocks[int.Parse(location_str[0]), int.Parse(location_str[1])].change_type(4, Content, int.Parse(parts[2]));

            //   int val = int.Parse(parts[3]);
        }
        private void lifePack_detail(String[] parts)
        {


            String[] location_str = parts[1].Split(',');
            blocks[int.Parse(location_str[0]), int.Parse(location_str[1])].change_type(5, Content, int.Parse(parts[2]));


        }
        private void acceptance(String[] parts)
        {
            // initializing player id
            String[] details = parts[1].Split(';');
            String player_id = details[0];
            String[] location_str = details[1].Split(',');
            int[] location = new int[] { int.Parse(location_str[0]), int.Parse(location_str[1]) };
            int direction = int.Parse(details[2]);
            //  this.player_id = player_id;
            ai.set_player_id(this.player_id);
            bool our = false;
            if (String.Compare(player_id, this.player_id) == 0) // if this is our tank
            {
                our = true;
            }
            blocks[int.Parse(location_str[0]), int.Parse(location_str[1])].change_type(player_id, Content, direction, -100, -100, -100, -100, our);

            ai.start_ai();

            //  Tank new_tank = new Tank(0, direction, player_id, 0, 0, 0, 0);
            //  boad.edit_grid(new_tank, location);


        }
        private void game_instant(String[] parts)
        {

            this.player_id = parts[1];
            player_1_text = "My player: " + this.player_id;
            // reading bricks
            String brick_map = parts[2];
            String[] bricks = brick_map.Split(';');
            foreach (String brick in bricks)
            {
                String[] brick_location = brick.Split(',');
                blocks[int.Parse(brick_location[0]), int.Parse(brick_location[1])].change_type(1, Content, -100);


            }

            // reading stones
            String stone_map = parts[3];
            String[] stones = stone_map.Split(';');
            foreach (String stone in stones)
            {
                String[] stone_location = stone.Split(',');
                blocks[int.Parse(stone_location[0]), int.Parse(stone_location[1])].change_type(2, Content, -100);

            }

            // reading water
            String water_map = parts[4];
            String[] waters = water_map.Split(';');
            foreach (String water in waters)
            {
                String[] water_location = water.Split(',');
                blocks[int.Parse(water_location[0]), int.Parse(water_location[1])].change_type(3, Content, -100);

            }

        }

        #endregion

    }
}
