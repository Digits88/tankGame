using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace TankGame
{
    class AI
    {


        #region variables
        TcpClient serv;
        NetworkStream stm;
        Timer _timer;
        String server_ip = "localhost";


        // grid of the game
        Block[,] blocks;
        int grid_size;
        String player_id;

        // mssages form the server
        string server_msg = "";




        // the target cell
        int[] target_tersure = { 11, 12 };
        int[] tmp_target_tresure = new int[2];

        // our tank location
        int[] our_tank_location = { 0, 0 };

        #endregion

        #region a star components

        // needed for a star algo

        private int path_size;
        private int[,] optimum_path = new int[1000, 2]; // array to store the optimum path
        private int[,] optimum_path_final = new int[1000, 2]; // to store the final path
        private Node last_node;
        private bool locked = false; // to lock the path finder
        private int watch_dog_counter = 0; // to stop unlimited iterations
        private bool wathc_dog_status = false;
        private int watch_dog_limit = 50; // limit of the watch dog
        private int max_h_to_tresure = 30;
        // class for nodes of the grid
        public class Node
        {
            int g;
            int h;
            int f;
            int[] parent = new int[2];
            bool restricted;
            bool opened;
            bool non;
            public Node()
            {
                this.non = true;
                this.opened = false;
                this.restricted = false;

            }
            public bool is_opened()
            {
                return this.opened;
            }

            public bool is_restricted()
            {
                return this.restricted;
            }

            public bool is_non()
            {
                return this.non;
            }

            public void close()
            {
                this.restricted = true;
                this.opened = false;
                this.non = false;
            }

            public void init()
            {
                this.opened = false;
                this.restricted = false;
                this.non = true;
                this.f = 0;
                this.g = 0;
                this.h = 0;

            }
            public void open()
            {
                this.opened = true;
                this.restricted = false;
                this.non = false;
            }

            public void restrict()
            {
                close();
            }

            public void set_all(int g, int h, int f)
            {
                this.h = h;
                this.f = f;
                this.g = g;
            }

            public void set_g(int g)
            {
                this.g = g;
            }

            public void set_h(int h)
            {
                this.h = h;
            }

            public void set_f(int f)
            {
                this.f = f;
            }
            public void set_g_h(int g, int h)
            {
                this.g = g;
                this.h = h;
                this.f = g + h;
            }
            public void set_parent(int x, int y)
            {
                this.parent[0] = x;
                this.parent[1] = y;
            }

            public int get_g()
            {
                return this.g;
            }

            public int get_h()
            {
                return this.h;
            }
            public int get_f()
            {
                return this.f;
            }
            public int[] get_parent()
            {
                return this.parent;
            }
        }

        // node grid

        Node[,] grid;

        #endregion

        #region constructro
        public AI(Block[,] blocks, int size, String player_id)
        {
            this.blocks = blocks;
            this.grid_size = size;
            this.player_id = player_id;
            grid = new Node[size, size];
            // initializing grid for the  a strat algorithm
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    grid[i, j] = new Node();
                }
            }
        }

        #endregion

        #region main running parts of ai
        // method to start ai
        public void start_ai()
        {
            System.Threading.Thread.Sleep(1000);


            //  _timer = new Timer(5000);
            //  _timer.Elapsed += new ElapsedEventHandler(kill_them);
            // _timer.Enabled = true;
            // System.Threading.Thread.Sleep(3000);


            //  find_the_tresure();
            while (true)
            {
                if (!locked)
                {
                    print_tresure();
                    System.Threading.Thread.Sleep(1000);
                }


            }


        }

        // all the ai logic goes here
        public void run_ai(object sender, ElapsedEventArgs e)
        {


            this.server_msg = "";
        }

        #endregion

        #region message passing with server
        // listner to the server messages from main class
        public void listenToMessages(Game1 game)
        {
            //  game.newMessage += new_sever_msg;

        }

        // accepting the server messages
        public void new_sever_msg(Object sender, ServerMessageArgs e)
        {
            this.server_msg = e.msg;
        }

        public void set_player_id(String id)
        {
            this.player_id = id;
        }

        public void send_message_to_server(String str)
        {

            try
            {
                serv = new TcpClient();
                serv.Connect(server_ip, 6000);
                stm = serv.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(str);
                stm.Write(ba, 0, ba.Length);
                stm.Close();
                serv.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }
        #endregion

        #region tank_moves

        // move our tank one time left
        public void move_left()
        {
            String msg = "LEFT#";
            send_message_to_server(msg);
        }
        //move our tank one time right
        public void move_right()
        {
            String msg = "RIGHT#";
            send_message_to_server(msg);
        }
        // move our message one time dowm
        public void move_down()
        {
            String msg = "DOWN#";
            send_message_to_server(msg);
        }
        //move our tank one time up
        public void move_up()
        {
            String msg = "UP#";
            send_message_to_server(msg);
        }

        #endregion

        #region finding the best tresure location

        public void print_tresure()
        {
            int[] best = get_best_tresure();
            //   Console.WriteLine(best[0] + "   " + best[1]);
            if (best != null)
            {
                locked = true;
                this.tmp_target_tresure[0] = best[0];
                this.tmp_target_tresure[1] = best[1];
                find_the_tresure();
            }
        }

        public int[] get_best_tresure()
        {
            get_our_tank_location();
            int tank_x = our_tank_location[0];
            int tank_y = our_tank_location[1]; // getting the tank location

            int close = int.MaxValue;
            int x = 0;
            int y = 0;
            for (int i = 0; i < grid_size; i++)
            {
                for (int j = 0; j < grid_size; j++)
                {
                    Block block = blocks[i, j];
                    if (block.get_type() == 4 || block.get_type() == 5)
                    {
                        int distance = find_h(i, j);
                        if (distance < close && distance != 0)
                        {
                            close = distance;
                            x = i;
                            y = j;
                        }
                    }

                }
            }
            if (close != int.MaxValue && (close + 2) < blocks[x, y].get_ttl() && close < max_h_to_tresure) // if this is suitable for a tresure
            {
                int[] fin = { x, y };
                return fin;
            }
            else
                return null;






        }



        #endregion

        #region tresure finding

        // initializing the grid

        public void init_grid()
        {
            for (int i = 0; i < this.grid_size; i++)
            {
                for (int j = 0; j < this.grid_size; j++)
                {
                    if (blocks[i, j].get_type() == 1 || blocks[i, j].get_type() == 2 || blocks[i, j].get_type() == 3) // if it is an obstacle
                    {
                        grid[i, j].close(); // close the cell
                    }
                    else
                    {
                        grid[i, j].init();
                    }
                }
            }

        }

        // a star algorithm goes here
        public void find_the_tresure()
        {
            //  if (locked)
            //      return;
            //  locked = true; // locking the method
            this.target_tersure[0] = this.tmp_target_tresure[0];
            this.target_tersure[1] = this.tmp_target_tresure[1]; // initializing the tresure

            Console.WriteLine(target_tersure[0] + "  " + target_tersure[1]);

            init_grid();
            get_our_tank_location(); // updating the tank locaion
            int tank_x = our_tank_location[0];
            int tank_y = our_tank_location[1];
            Node starting_node = grid[our_tank_location[0], our_tank_location[1]]; // the staarting node of the grid
            starting_node.set_g_h(0, 0); // initializing the starting node
            wathc_dog_status = false; // off watch bog
            watch_dog_counter = 0; // initializing the timer
            a_star(our_tank_location[0], our_tank_location[1], 0); // starting the a star algorithm
            if (wathc_dog_status)
            {
                Console.WriteLine("??????????????????????????????????");
                locked = false;
                return;
            }
            // storing final path
            path_size = 1;
            optimum_path[0, 0] = target_tersure[0];
            optimum_path[0, 1] = target_tersure[1];
            while (true)
            {
                try
                {
                    optimum_path[path_size, 0] = last_node.get_parent()[0];
                    optimum_path[path_size, 1] = last_node.get_parent()[1];
                    //Console.WriteLine(last_node.get_parent()[0] + "   " + last_node.get_parent()[1]);
                    last_node = grid[last_node.get_parent()[0], last_node.get_parent()[1]];
                    path_size++;
                    if (last_node.get_parent()[0] == tank_x && last_node.get_parent()[1] == tank_y)
                        break;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    locked = false;
                    return;
                }


            }

            Console.WriteLine("########################");
            for (int i = 1; i <= path_size; i++)
            {
                optimum_path_final[i, 0] = optimum_path[path_size - i, 0];
                optimum_path_final[i, 1] = optimum_path[path_size - i, 1];
                Console.WriteLine(optimum_path_final[i, 0] + "   " + optimum_path_final[i, 1]);
            }

            Console.WriteLine("########################");
            drive_tank();

        }


        // finding the h value of a given cell
        public int find_h(int x, int y)
        {
            int value_x = Math.Abs(x - target_tersure[0]);
            int value_y = Math.Abs(y - target_tersure[1]);
            return value_x + value_y;
        }

        // recursive function of the a star
        public void a_star(int x, int y, int number)
        {
            watch_dog_counter++; // increasing the watch dow counter
            if (watch_dog_counter >= watch_dog_limit) // error in the code
            {
                wathc_dog_status = true; // on the watch dog
                return;
            }
            if (wathc_dog_status)
            {
                return;
            }

            if (x == target_tersure[0] && y == target_tersure[1]) // target reached
                return;

            #region variables for the algorithm

            grid[x, y].close(); // closing the current grid
            int[] next_cell = new int[2]; // to store the next index of the search algorithm
            int smallest_f = int.MaxValue; // to store the optimem f value
            Node this_node = grid[x, y]; // the current node

            #endregion

            #region going through cells

            try
            {
                int current_x = x;
                int current_y = y - 1;
                Node current_node = grid[current_x, current_y]; // getting the node
                if (current_node.is_non()) // if it not opened yet (no parent)
                {
                    // current_node.set_parent(this_node); // setting parent of the node
                    current_node.open(); // add that node to the open list
                    int current_h = find_h(current_x, current_y); // calculating the h value of the child node
                    int current_g = this_node.get_g() + 1; // set the g value of the child to infinity
                    current_node.set_g_h(current_g, current_h); // setting the gh values of the clild node
                    current_node.set_parent(x, y); // setting the parent node
                    if (current_node.get_f() < smallest_f) // smallest f values cell
                    {
                        next_cell[0] = current_x;
                        next_cell[1] = current_y; // setting the coordiantes of the next cell
                        smallest_f = current_node.get_f(); // updating the smallest f
                    }
                }
                else if (current_node.is_opened()) // if it is an opened cell
                {
                    if (current_node.get_f() < smallest_f) // if this is the smallest f valued cell
                    {
                        next_cell[0] = current_x;
                        next_cell[1] = current_y; // setting the next optimum cell
                        smallest_f = current_node.get_f(); // updating the smallest f
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                int current_x = x + 1;
                int current_y = y;
                Node current_node = grid[current_x, current_y]; // getting the node
                if (current_node.is_non()) // if it not opened yet (no parent)
                {
                    current_node.set_parent(x, y); // setting the parent node
                    current_node.open(); // add that node to the open list
                    int current_h = find_h(current_x, current_y); // calculating the h value of the child node
                    int current_g = this_node.get_g() + 1; // set the g value of the child to infinity
                    current_node.set_g_h(current_g, current_h); // setting the gh values of the clild node
                    if (current_node.get_f() < smallest_f) // smallest f values cell
                    {
                        next_cell[0] = current_x;
                        next_cell[1] = current_y; // setting the next optimum cell
                        smallest_f = current_node.get_f(); // updating the smallest f
                    }
                }
                else if (current_node.is_opened()) // if it is an opened cell
                {
                    if (current_node.get_f() < smallest_f) // if this is the smallest f valued cell
                    {
                        next_cell[0] = current_x;
                        next_cell[1] = current_y; // setting the next optimum cell
                        smallest_f = current_node.get_f(); // updating the smallest f
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            try
            {
                int current_x = x;
                int current_y = y + 1;
                Node current_node = grid[current_x, current_y]; // getting the node
                if (current_node.is_non()) // if it not opened yet (no parent)
                {
                    current_node.set_parent(x, y); // setting the parent node
                    current_node.open(); // add that node to the open list
                    int current_h = find_h(current_x, current_y); // calculating the h value of the child node
                    int current_g = this_node.get_g() + 1; // set the g value of the child to infinity
                    current_node.set_g_h(current_g, current_h); // setting the gh values of the clild node
                    if (current_node.get_f() < smallest_f) // smallest f values cell
                    {
                        next_cell[0] = current_x;
                        next_cell[1] = current_y; // setting the next optimum cell
                        smallest_f = current_node.get_f(); // updating the smallest f
                    }
                }
                else if (current_node.is_opened()) // if it is an opened cell
                {
                    if (current_node.get_f() < smallest_f) // if this is the smallest f valued cell
                    {
                        next_cell[0] = current_x;
                        next_cell[1] = current_y; // setting the next optimum cell
                        smallest_f = current_node.get_f(); // updating the smallest f
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            try
            {
                int current_x = x - 1;
                int current_y = y;
                Node current_node = grid[current_x, current_y]; // getting the node
                if (current_node.is_non()) // if it not opened yet (no parent)
                {
                    current_node.set_parent(x, y); // setting the parent node
                    current_node.open(); // add that node to the open list
                    int current_h = find_h(current_x, current_y); // calculating the h value of the child node
                    int current_g = this_node.get_g() + 1; // set the g value of the child to infinity
                    current_node.set_g_h(current_g, current_h); // setting the gh values of the clild node
                    if (current_node.get_f() < smallest_f) // smallest f values cell
                    {
                        next_cell[0] = current_x;
                        next_cell[1] = current_y; // setting the next optimum cell
                        smallest_f = current_node.get_f(); // updating the smallest f
                    }
                }
                else if (current_node.is_opened()) // if it is an opened cell
                {
                    if (current_node.get_f() < smallest_f) // if this is the smallest f valued cell
                    {
                        next_cell[0] = current_x;
                        next_cell[1] = current_y; // setting the next optimum cell
                        smallest_f = current_node.get_f(); // updating the smallest f
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            #endregion

            #region iterating
            int estimate_g = this_node.get_g() + 1; // estimated g value for the child node if it goes through this node
            Node selected = grid[next_cell[0], next_cell[1]]; // selected node as the optimum path
            if (estimate_g > selected.get_g())
            {
                this.last_node = selected;
            }
            else
            {
                selected.set_parent(x, y);
                this.last_node = selected;

            }
            a_star(next_cell[0], next_cell[1], number + 1);

            #endregion
        }

        // get current tank locaion
        public void get_our_tank_location()
        {
            for (int i = 0; i < grid_size; i++)
            {
                for (int j = 0; j < grid_size; j++)
                {
                    Block block = blocks[i, j];
                    if (block.get_type() == 6) // if it is a tank
                    {
                        if (String.Compare(this.player_id, block.get_tank_id()) == 0)   // if this is our tank
                        {
                            this.our_tank_location[0] = i;
                            this.our_tank_location[1] = j;
                            return; // exit

                        }
                    }
                }
            }


        }


        // check for tresure existance
        private bool is_tresure_exist()
        {
            Block tresure = blocks[target_tersure[0], target_tersure[1]];
            int type = tresure.get_type();
            if (!(type == 4 || type == 5))
            {
                return false;
            }
            return true;
        }



        // driving the tank according to the root 
        public void drive_tank()
        {
            int watch_dog_path = 0;
            int wathc_dog_path_limit = 10;
            get_our_tank_location();
            Console.WriteLine("..................." + our_tank_location[0] + "   " + our_tank_location[1]);
            for (int i = 1; i <= path_size;) // iterating through the path 
            {
                watch_dog_path++; // watch dog
                if (watch_dog_path > wathc_dog_path_limit)
                {
                    locked = false;
                    return;
                }

                // check for excistance of the tresure
                if (!is_tresure_exist())
                {
                    Console.WriteLine("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                    locked = false;
                    return;
                }
                get_our_tank_location();
                int tank_x = our_tank_location[0];
                int tank_y = our_tank_location[1];
                //   shoot();// shooting
                if (optimum_path_final[i, 0] == tank_x)
                {
                    if (optimum_path_final[i, 1] > tank_y)
                    {
                        move_down();
                    }
                    else if (optimum_path_final[i, 1] == tank_y)
                    {

                    }
                    else
                    {
                        move_up();//move_up();
                    }
                }
                else
                {
                    if (optimum_path_final[i, 0] > tank_x)
                        move_right();
                    else if (optimum_path_final[i, 0] == tank_x)
                    {

                    }
                    else
                        move_left();
                }

                System.Threading.Thread.Sleep(1100);
                get_our_tank_location();
                if (tank_x == our_tank_location[0] && tank_y == our_tank_location[1])
                    continue;


                i++;
                watch_dog_path = 0;

            }
            Console.WriteLine("done****************************");
            locked = false;
            shoot();
        }

        #endregion 

        #region shooting


        public void kill_them(object sender, ElapsedEventArgs e)
        {
            try
            {
                shoot();
            }
            catch (Exception ex)
            {

            }
        }
        private bool check_for_enemy()
        {
            get_our_tank_location();
            int x = our_tank_location[0];
            int y = our_tank_location[1];
            Block our_tank = blocks[x, y]; // our tank
            int direction = our_tank.get_direction();
            if (direction == 0) // up
            {
                for (int i = y - 1; i > -1; i--)
                {
                    Block enemy = blocks[x, i];
                    if (enemy.get_type() == 6 || enemy.get_type() == 1) // if it is a tank or brick
                    {
                        return true;
                    }
                }
            }
            else if (direction == 2) // down
            {
                for (int i = y + 1; i < grid_size; i++)
                {
                    Block enemy = blocks[x, i];
                    if (enemy.get_type() == 6 || enemy.get_type() == 1) // if it is a tank or brick
                    {
                        return true;
                    }
                }
            }
            else if (direction == 1) // right
            {
                for (int i = x + 1; i < grid_size; i++)
                {
                    Block enemy = blocks[i, y];
                    if (enemy.get_type() == 6 || enemy.get_type() == 1) // if it is a tank or brick
                    {
                        return true;
                    }
                }
            }
            else // left
            {
                for (int i = x - 1; i >= 0; i--)
                {
                    Block enemy = blocks[i, y];
                    if (enemy.get_type() == 6 || enemy.get_type() == 1) // if it is a tank or brick
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void shoot()
        {
            if (check_for_enemy())
            {
                send_message_to_server("SHOOT#");
                System.Threading.Thread.Sleep(1000);
                send_message_to_server("SHOOT#");
                System.Threading.Thread.Sleep(1000);
                send_message_to_server("SHOOT#");
                System.Threading.Thread.Sleep(1000);
                send_message_to_server("SHOOT#");
                Console.WriteLine("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                System.Threading.Thread.Sleep(1000);
            }
        }
        #endregion
    }
}
