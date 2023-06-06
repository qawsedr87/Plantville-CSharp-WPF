using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace RLinPlantville
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static List<Seed> seed_list = new List<Seed>() {
            new Seed("strawberries", 2, 8, "00:00:30"),
            new Seed("spinach", 5, 21, "00:01:00"),
            new Seed("pears", 2, 20, "00:03:00")
        };

        private static string filePath = "player_stats.txt";

        Dictionary<string, Seed> seedDict = new Dictionary<string, Seed>();
        
        private static List<TradeRecord> garden = new List<TradeRecord>();
        private static List<TradeRecord> inventory = new List<TradeRecord>();
        private static int money = 100;
        private static int land = 15;

        // private static FarmRecord farmRecord = new FarmRecord(garden, inventory, money);
        private static TradeFarmRecord tradeFarmRecord = new TradeFarmRecord(garden, inventory, money);

        private string g_username = "";
        public MainWindow()
        {
            InitializeComponent();

            // chat
            UpdateChatListBox();

            // username 
            username_tb.Text = $"Hello, {g_username}";

            // load player_stats 
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                tradeFarmRecord = JsonConvert.DeserializeObject<TradeFarmRecord>(json);

                garden = tradeFarmRecord.Garden;
                inventory = tradeFarmRecord.Inventory;
                money = tradeFarmRecord.Money;
                land = 15 - garden.Count;
            }

            load_init_data();
            
        }

        private void signin_tb_keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && signin_tb.Text != "")
            {
                load_g_username(signin_tb.Text);
            }
            else if (e.Key == Key.Enter && signin_tb.Text == "")
            {
                MessageBox.Show("Please Enter Message", null);
            }
        }

        private void signin_btn_Click(object sender, RoutedEventArgs e)
        {
            if (signin_tb.Text == null)
            {
                MessageBox.Show("Please Enter Message", null);
            }
            else
            {
                load_g_username(signin_tb.Text);
            }
        }

        private void load_g_username(string name)
        {
            g_username = name;
            username_tb.Text = $"Hello, {g_username}";

            signin_grid.Visibility = Visibility.Collapsed;
            menu_grid.Visibility = Visibility.Visible;
        }

        private void load_init_data()
        {
            // seeds
            propose_seed_cb.Items.Clear();

            foreach (Seed seed in seed_list)
            {
                seedDict.Add(seed.Name, seed);

                seed_lb.Items.Add($"{seed.Name} ${seed.SeedPrice}");

                // combo box 
                propose_seed_cb.Items.Add(seed.Name);
            }

            // money and land info
            money_tb.Text = $"Money: ${money.ToString()}";
            land_tb.Text = $"Land: {land.ToString()}";

            // main
            load_garden_data();
        }

        private void empty_inventory_lb()
        {
            inventory_lb.Items.Clear();
            inventory_lb.Items.Add("No fruits or vegetables harvested.");
        }

        private void empty_garden_lb()
        {
            garden_lb.Items.Clear();
            garden_lb.Items.Add("No plants in garden.");
        }

        public void set_menu_visibility(List<UIElement> menusToShow, List<UIElement> menusToHide)
        {
            foreach (var menu in menusToShow)
            {
                menu.Visibility = Visibility.Visible;
            }

            foreach (var menu in menusToHide)
            {
                menu.Visibility = Visibility.Collapsed;
            }
        }

        private void garden_btn_Click(object sender, RoutedEventArgs e)
        {
            init_title("Garden", "What you would like to harvest?");
            List<UIElement> menusToShow = new List<UIElement>
            {
                garden_harvest_btn,
                garden_lb
            };

            List<UIElement> menusToHide = new List<UIElement>
            {
                inventory_sell_btn,
                propose_accept_btn,
                chat_send_btn,
                chat_input_tb,
                inventory_lb,
                seed_lb,
                chat_lb,
                trade_lb,
                propose_lb,
                propose_summit_btn
            };

            set_menu_visibility(menusToShow, menusToHide);

            load_garden_data();
        }

        private void load_garden_data()
        {

            garden_lb.Items.Clear();
            if (garden.Count == 0) empty_garden_lb();
            else
            {
                foreach (TradeRecord record in garden)
                {
                    garden_lb.Items.Add($"{record.Seed.Name} ({check_seed_status(record, false)})");
                }
            }
        }

        private string check_seed_status(TradeRecord record, Boolean isHarvested)
        {
            string status = "harvest";

            if (!isHarvested)
            {
                DateTime harvestEndTime = record.HarvestTime.Add(TimeSpan.Parse(record.Seed.HarvertDuration));
                DateTime harvestSpoiledTime = harvestEndTime.Add(TimeSpan.Parse("00:15:00"));

                TimeSpan timeLeft = harvestEndTime - DateTime.Now;
                int left = Convert.ToInt32(timeLeft.TotalMinutes.ToString("F0"));

                if (record.IsSpoiled) status = "spoiled";
                else if (DateTime.Now >= harvestSpoiledTime)
                {
                    record.IsSpoiled = true;
                    status = "spoiled";
                }
                // harvestTime <= now < harvestEndTime
                else if (left >= 1)
                {
                    status = $"{left} minutes left";
                }
            }
            

            return status;
        }

        private void inventory_btn_Click(object sender, RoutedEventArgs e)
        {
            init_title("Inventory", "$10 each trip to farmer's market.");
            List<UIElement> menusToShow = new List<UIElement>
            {
                inventory_sell_btn,
                inventory_lb
            };

            List<UIElement> menusToHide = new List<UIElement>
            {
                garden_harvest_btn,
                garden_lb,
                propose_accept_btn,
                chat_send_btn,
                chat_input_tb,
                seed_lb,
                chat_lb,
                trade_lb,
                propose_lb,
                propose_summit_btn
            };

            set_menu_visibility(menusToShow, menusToHide);

            // clear
            inventory_lb.Items.Clear();
            if (inventory.Count == 0) empty_inventory_lb();
            else
            {
                var groupInventory = GetGroupInventoryList(inventory);

                foreach (TradeRecord seedRecord in groupInventory)
                {
                    inventory_lb.Items.Add($"{seedRecord.Seed.Name} [{seedRecord.Quantity}] ${seedRecord.Seed.SeedPrice}");
                }

                // update new inventory list 
                inventory = groupInventory;
            }
        }


        private void seed_btn_Click(object sender, RoutedEventArgs e)
        {
            init_title("Seeds Emporium", "What you would like to purchase?");
            List<UIElement> menusToShow = new List<UIElement>
            {
                seed_lb
            };

            List<UIElement> menusToHide = new List<UIElement>
            {
                garden_harvest_btn,
                garden_lb,
                propose_accept_btn,
                chat_send_btn,
                chat_input_tb,
                inventory_sell_btn,
                inventory_lb,
                chat_lb,
                trade_lb,
                propose_lb,
                propose_summit_btn
            };

            set_menu_visibility(menusToShow, menusToHide);

        }

        private void trade_btn_Click(object sender, RoutedEventArgs e)
        {
            init_title("Trade Marketplace", "");
            List<UIElement> menusToShow = new List<UIElement>
            {
                trade_lb,
                propose_accept_btn
            };

            List<UIElement> menusToHide = new List<UIElement>
            {
                garden_harvest_btn,
                garden_lb,
                chat_send_btn,
                chat_input_tb,
                inventory_sell_btn,
                inventory_lb,
                chat_lb,
                seed_lb,
                propose_lb,
                propose_summit_btn
            };

            set_menu_visibility(menusToShow, menusToHide);

            UpdateTradeListBox();

        }

        private void propose_btn_Click(object sender, RoutedEventArgs e)
        {
            init_title("Propose Trade", "");
            List<UIElement> menusToShow = new List<UIElement>
            {

                propose_lb,
                propose_summit_btn
            };

            List<UIElement> menusToHide = new List<UIElement>
            {
                garden_harvest_btn,
                garden_lb,
                chat_send_btn,
                chat_input_tb,
                inventory_sell_btn,
                inventory_lb,
                chat_lb,
                seed_lb,
                trade_lb,
                propose_accept_btn
            };

            set_menu_visibility(menusToShow, menusToHide);

        }

        private void seed_lb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            // check if not null
            if (seed_lb.SelectedItem != null && money > 0 && land > 0)
            {
                string selected = seed_lb.SelectedItem.ToString().Split()[0];
                Seed selectedSeed = seedDict[selected];

                // add into garden
                TradeRecord record = new TradeRecord(selectedSeed, DateTime.Now, 1, false);
                garden.Add(record);

                // subtract money and land
                money -= selectedSeed.SeedPrice;
                money_tb.Text = $"Money: ${money}";
                land--;
                land_tb.Text = $"Land: {land}";

                // show message
                MessageBox.Show($"You purchased {selectedSeed.Name}");

            } else
            {
                MessageBox.Show("You don't have enough money or land.");
            }
   
        }

        private void sell_inventory_btn_Click(object sender, RoutedEventArgs e) 
        {   
             if (inventory.Count == 0)
             {

                if (MessageBox.Show("Are you sure you want to go to the farmer's market without any inventory?",
                       "Lose money for no reason?",
                       MessageBoxButton.YesNo,
                       MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    money -= 10;
                    money_tb.Text = $"Money: ${money}";

                    inventory_result_message_show(-10);
                }
            } else
            {
                int earn = 0;

                foreach (TradeRecord record in inventory)
                {
                    if (!record.IsSpoiled)
                    {
                        int quantity = record.Quantity;
                        int price = record.Seed.HarvertPrice;

                        earn += quantity * price;
                    }
                }

                // fee 
                earn -= 10;
                inventory_result_message_show(earn);

                money += earn;
                money_tb.Text = $"Money: ${money}";

                // clear 
                inventory.Clear();
                empty_inventory_lb();
            }
        }

        private void inventory_result_message_show(int money)
        {
            MessageBox.Show($"Cleared inventory. Made ${money}");
        }

       
        private void init_title(string title, string subtitle)
        {
            title_tb.Text = title;
            subtitle_tb.Text = subtitle;
        }

        private void garden_harvest_btn_Click(object sender, RoutedEventArgs e)
        {
            
            if (garden.Count == 0)
            {
                MessageBox.Show("Nothing to harvest.");
                return;
            } 

            List<TradeRecord> itemsRemove = new List<TradeRecord>();

            foreach (TradeRecord record in garden)
            {
                harvest_seed(record, true);

                string status = check_seed_status(record, false);
                if (status.Equals("harvest") || status.Equals("spoiled"))
                {
                    itemsRemove.Add(record);
                }

            }

            garden.RemoveAll(record => itemsRemove.Contains(record));
            load_garden_data();

            if (itemsRemove.Count == 0) MessageBox.Show("Nothing to harvest.");
            else MessageBox.Show($"Harvested {itemsRemove.Count} plants.");
        }

        private void garden_lb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (garden_lb.SelectedIndex != -1 && garden.Count > 0)
            {
                int index = garden_lb.SelectedIndex;

                TradeRecord selected = garden[index];

                harvest_seed(selected, false);
                
                string status = check_seed_status(selected, false);

                if (status.Equals("harvest") || status.Equals("spoiled"))
                {
                    // update garden record
                    garden.Remove(selected);

                    // remove the selectedItem
                    if (selected != null)
                    {

                        garden_lb.Items.Remove(garden_lb.SelectedItem);
                    }

                    if (garden_lb.Items.Count == 0) empty_garden_lb();
                }

                
            } else
            {
                MessageBox.Show("Nothing to harvest.");
            }
        }

        private void harvest_seed(TradeRecord record, Boolean isAll)
        {
             
            string status = check_seed_status(record, false);
            if (status.Equals("harvest"))
            {
                inventory.Add(record);

                // update land
                add_land();

                // show message 
                if (!isAll)
                    MessageBox.Show($"{record.Seed.Name} harvested.");
            } else if (status.Equals("spoiled"))
            {
                // spoiled couldn't store into inventory
                // update land 
                add_land();

                // show message 
                if (!isAll)
                    MessageBox.Show($"{record.Seed.Name} spoiled.");
            }
            else
            {
                if (!isAll) MessageBox.Show("Nothing to harvest.");
            }
        }

        private void add_land()
        {
            land++;
            land_tb.Text = $"Land: {land}";
        }

        private void window_closing(object sender, CancelEventArgs e)
        {
            string playerStats = save_player_stats();

            File.WriteAllText(filePath, playerStats);
        }

        private string save_player_stats()
        {
            tradeFarmRecord.Garden = garden;
            tradeFarmRecord.Inventory = inventory;
            tradeFarmRecord.Money = money;

            return JsonConvert.SerializeObject(tradeFarmRecord); ;
        }

        private void chat_send_btn_Click(object sender, RoutedEventArgs e)
        {
            if (chat_input_tb.Text == null)
            {
                MessageBox.Show("Please Enter Message", null);
            }
            else
            {
                string input = chat_input_tb.Text;
                PostChatMessage(new ChatInput(g_username, input));

                // update chat 
                UpdateChatListBox();

                // clear
                chat_input_tb.Clear();
            }
        }

        private void chat_input_tb_keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && chat_input_tb.Text != "")
            {
                string input = chat_input_tb.Text;
                PostChatMessage(new ChatInput(g_username, input));

                // update chat 
                UpdateChatListBox();
                // clear
                chat_input_tb.Clear();
            } else if (e.Key == Key.Enter && chat_input_tb.Text == "")
            {
                MessageBox.Show("Please Enter Message", null);
            }
        }

        private void propose_accept_btn_Click(object sender, RoutedEventArgs e)
        {
            // selected
            if(trade_lb.SelectedItem == null)
            {
                MessageBox.Show("Error: Please select a trade to accept.", null);
                return;
            }

            // inventory none 
            if (inventory.Count == 0)
            {
                MessageBox.Show("Inventory Empty!", null);
                return;
            }

            // Trade
            int index = trade_lb.SelectedIndex;
            List<Trade> trades = GetTrades();
 
            Trade selectedTrade = trades[index];
            if (!selectedTrade.Fields.State.Equals("open"))
            {
                MessageBox.Show("Trade is not available, please try again!", null);
                return;
            }

            // check inventory and trade deal 
            if (!IsInStock(selectedTrade))
            {
                MessageBox.Show($"Error: You do not have enough {selectedTrade.Fields.Plant} in your inventory to make trade.", null);
                return;
            }

            // api
            AcceptTradeInput accept = new AcceptTradeInput(selectedTrade.Pk, g_username);
            List<Trade> newTrades = PostAcceptTrade(accept);
            if (newTrades.Count == trades.Count)
            {
                // earn money
                money += selectedTrade.Fields.Price;

                MessageBox.Show($"Trade accepted! You bought {selectedTrade.Fields.Quantity} {selectedTrade.Fields.Plant} for ${selectedTrade.Fields.Price} from {selectedTrade.Fields.Author}");

                money_tb.Text = $"Money: ${money}";

                // update inventory 
                var matchingRecord = inventory.FirstOrDefault(record =>
                    record.Seed.Name.Equals(selectedTrade.Fields.Plant) &&
                    !record.IsSpoiled &&
                    record.Quantity >= selectedTrade.Fields.Quantity
                );

                if (matchingRecord != null)
                {
                    int remainingQuantity = matchingRecord.Quantity - selectedTrade.Fields.Quantity;
                    if (remainingQuantity > 0)
                    {
                        matchingRecord.Quantity = remainingQuantity;
                    }
                    else
                    {
                        inventory.Remove(matchingRecord);
                    }
                }

                // update trade 
                UpdateTradeListBox();
            }
        }

        private bool IsInStock(Trade trade)
        {
            List<TradeRecord> updatedInventory = GetGroupInventoryList(inventory);
            inventory = updatedInventory;

            // check if updatedInventory has enough trade quantity 
            var checkStock = inventory.Where(record =>
                record.Seed.Name.Equals(trade.Fields.Plant) &&
                !record.IsSpoiled &&
                record.Quantity >= trade.Fields.Quantity
            );

            return checkStock.Any();

        }

        private void propose_summit_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int quantity = int.Parse(propose_quantity_tb.Text);
                int price = int.Parse(propose_price_tb.Text);

                string plant = propose_seed_cb.SelectedItem.ToString();

                TradeInput trade = new TradeInput(g_username, plant, quantity, price);
                PostTrade(trade);

                // clear 
                propose_seed_cb.Items.Clear();
                propose_quantity_tb.Clear();
                propose_price_tb.Clear();


            } catch (Exception ex)
            {
                MessageBox.Show("Input string was not in a correct format in Quantity and Price", null);
                Console.WriteLine(ex.ToString());
            }

        }

        private void chat_btn_Click(object sender, RoutedEventArgs e)
        {
            init_title("Chatroom", "");
            List<UIElement> menusToShow = new List<UIElement>
            {

                chat_send_btn,
                chat_input_tb,
                chat_lb
            };

            List<UIElement> menusToHide = new List<UIElement>
            {
                garden_harvest_btn,
                garden_lb,
                propose_lb,
                propose_summit_btn,
                inventory_sell_btn,
                inventory_lb,
                seed_lb,
                trade_lb,
                propose_accept_btn
            };

            set_menu_visibility(menusToShow, menusToHide);

            UpdateChatListBox();
        }

        public static List<TradeRecord> GetGroupInventoryList(List<TradeRecord> list)
        {
            // LINQ for quantity by "group by Seed" and "IsSpoiled is false"
            List<TradeRecord> inventoryList = inventory.Where(record => !record.IsSpoiled)
                .GroupBy(record => record.Seed.ToString())
                .Select(group =>
                {
                    Seed seed = group.FirstOrDefault()?.Seed;
                    DateTime harvestTime = group.FirstOrDefault()?.HarvestTime ?? DateTime.Now;
                    int quantity = group.Sum(record => record.Quantity);

                    return new TradeRecord(seed, harvestTime, quantity, false);
                })
                .ToList();

            return inventoryList;
        }

        public void UpdateTradeListBox()
        {
            List<Trade> list = GetTrades();
            trade_lb.Items.Clear();

            foreach (Trade t in list)
            {
                string state = t.Fields.State;
                string message = "";
                
                if (state.Equals("open"))
                {
                    message = $"{t.Fields.Author} wants to buy {t.Fields.Quantity} {t.Fields.Plant} for ${t.Fields.Price}";
                } else // state = close, pending 
                {
                    message = $"{t.Fields.Author} bought {t.Fields.Quantity} {t.Fields.Plant} for ${t.Fields.Price} from {t.Fields.AcceptedBy}";
                } 
                trade_lb.Items.Add($"[{state}] {message}");
            }
        }

        public void UpdateChatListBox() {
            List<Chat> list = GetChatMessages();
            chat_lb.Items.Clear();

            foreach (Chat c in list)
            {
                chat_lb.Items.Add($"{c.Fields.Username}: {c.Fields.Message}");
            }
        }
        public static List<Trade> PostAcceptTrade(AcceptTradeInput accept)
        {
            var url = new Uri("https://plantville.herokuapp.com/accept_trade");
            var formData = new Dictionary<string, string> {
                { "trade_id", accept.TradeId.ToString() },
                { "accepted_by", accept.AcceptedBy }
            };

            List<Trade> list = new List<Trade>();

            try
            {

                var arrayTask = PostJsonArrayAsync(url, formData);

                if (arrayTask.Equals("FAIL"))
                {
                    MessageBox.Show($"Post Accepted Trade Error: couldn't accept trade_id '{accept.TradeId}' because trade state is not open.", null);
                    return list;
                }

                var jsonArray = JArray.Parse(arrayTask);
                foreach (JObject obj in jsonArray)
                {
                    Trade item = obj.ToObject<Trade>();

                    // System.Console.WriteLine(item.ToString());
                    list.Add(item);
                }
            }
            catch (AggregateException e)
            {
                MessageBox.Show($"Post Accepted Trade Error: trade_id '{accept.TradeId}' doesn't exist.", null);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Post Accepted Trade Error: {e.ToString()}", null);
            }

            return list;
        }

        public static List<Trade> PostTrade(TradeInput trade)
        {
            var url = new Uri("http://plantville.herokuapp.com/trades");
            var formData = new Dictionary<string, string> {
                { "author", trade.Author },
                { "plant", trade.Plant },
                { "quantity", trade.Quantity.ToString() },
                { "price", trade.Price.ToString() }
            };

            try
            {
                var task = PostJsonArrayAsync(url, formData);

                System.Console.WriteLine($"New Trade Open: {task}");
                MessageBox.Show("Successfully Added!");
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"Post Trade Error: {e.ToString()}");
            }

            return GetTrades();
        }

        public static List<Chat> PostChatMessage(ChatInput chat)
        {
            var url = new Uri("http://plantville.herokuapp.com");
            var formData = new Dictionary<string, string> {
                { "username", chat.Username },
                { "message", chat.Message }
            };

            List<Chat> list = new List<Chat>();

            try
            {
                var arrayTask = PostJsonArrayAsync(url, formData);

                var jsonArray = JArray.Parse(arrayTask);
                foreach (JObject obj in jsonArray)
                {
                    Chat item = obj.ToObject<Chat>();

                    // System.Console.WriteLine(item.ToString());
                    list.Add(item);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"Post Chat Message Error: {e.ToString()}");
            }

            return list;
        }

        public static string PostJsonArrayAsync(Uri uri, Dictionary<string, string> formData)
        {
            using (var client = new HttpClient())
            {
                var response = client.PostAsync(uri, new FormUrlEncodedContent(formData)).Result;
                response.EnsureSuccessStatusCode();

                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public static JArray GetJsonArrayAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                string jsonString = client.GetStringAsync(uri).Result;

                return JArray.Parse(jsonString);
            }
        }

        public static List<Trade> GetTrades()
        {
            var url = new Uri("http://plantville.herokuapp.com/trades");
            return GetObjects<Trade>(url);
        }
        public static List<Chat> GetChatMessages()
        {
            var url = new Uri("http://plantville.herokuapp.com");
            return GetObjects<Chat>(url);
        }

        // Get 
        public static List<T> GetObjects<T>(Uri url)
        {

            var jsonArray = GetJsonArrayAsync(url);

            List<T> list = new List<T>();
            foreach (JObject obj in jsonArray)
            {
                T item = obj.ToObject<T>();

                list.Add(item);
            }

            return list;
        }

    }
    public class TradeFarmRecord
    {

        public List<TradeRecord> Garden { get; set; }
        public List<TradeRecord> Inventory { get; set; }
        public int Money { get; set; }

        public TradeFarmRecord(List<TradeRecord> garden, List<TradeRecord> inventory, int money)
        {
            Garden = garden;
            Inventory = inventory;
            Money = money;
        }
    }

    public class TradeRecord
    {
        public Seed Seed { get; set; }
        public DateTime HarvestTime { get; set; } // default: Now
        public int Quantity { get; set; } // default: 1

        public bool IsSpoiled { get; set; } // default: false

        public TradeRecord(Seed seed, DateTime harvestTime, int quantity, bool isSpoiled)
        {
            Seed = seed;
            HarvestTime = harvestTime;
            Quantity = quantity;
            IsSpoiled = isSpoiled;
        }

        public override string ToString()
        {
            string seedInfo = Seed.ToString();
            string harvestTime = HarvestTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
            bool isSpoiled = IsSpoiled;
            int quantity = Quantity;

            return $"- Trade Seed[{quantity}]: {seedInfo}, Harvest Time: {harvestTime}, Is Spoiled: {isSpoiled}\n";
        }
    }

    public class AcceptTradeInput
    {
        public int TradeId { get; set; }
        public string AcceptedBy { get; set; }

        public AcceptTradeInput(int id, string acceptdBy)
        {
            TradeId = id;
            AcceptedBy = acceptdBy;
        }
    }

    public class Trade
    {
        public string Model { get; set; }
        public int Pk { get; set; }

        public TradeMeta Fields { get; set; }

        public class TradeMeta
        {
            public string Author { get; set; }

            [JsonProperty("accepted_by")]
            public string AcceptedBy { get; set; } // default: null
            public int Price { get; set; }
            public string State { get; set; } // pending, open, close. default: open
            public string Plant { get; set; } // seed_list
            public int Quantity { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public DateTime UpdatedAt { get; set; }
        }

        public override string ToString()
        {
            return $"Model {Model}, Pk {Pk}, Fields: Author {Fields.Author}, AcceptedBy {Fields.AcceptedBy}, Price {Fields.Price}, State {Fields.State}, Plant {Fields.Plant}, Quantity {Fields.Quantity}, CreatedAt {Fields.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}";
        }
    }

    public class TradeInput
    {
        public string Author { get; set; }
        public string Plant { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }

        public TradeInput(string author, string plant, int quantity, int price)
        {
            Author = author;
            Plant = plant;
            Quantity = quantity;
            Price = price;
        }
    }
    public class ChatInput
    {
        public string Username { get; set; }
        public string Message { get; set; }

        public ChatInput(string username, string message)
        {
            Username = username;
            Message = message;
        }
    }

    public class Chat
    {
        public string Model { get; set; }
        public int Pk { get; set; }
        public ChatMeta Fields { get; set; }

        public class ChatMeta
        {
            public string Username { get; set; }
            public string Message { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("updated_at")]
            public DateTime UpdatedAt { get; set; }
        }

        public override string ToString()
        {
            return $"Model {Model}, Pk {Pk}, Fields: {Fields.Username}, {Fields.Message} @{Fields.CreatedAt.ToString("yyyy-mm-ddTHH:mm:ss.fffZ")}";
        }
    }

    public class Seed
    {
        public string Name { get; set; }
        public int SeedPrice { get; set; }
        public int HarvertPrice { get; set; }
        public string HarvertDuration { get; set; } // "hh:mm:ss"

        public Seed(string name, int sPrice, int hPrice, string hDuration)
        {
            Name = name;
            SeedPrice = sPrice;
            HarvertPrice = hPrice;
            // HarvertDuration = TimeSpan.Parse(hDuration);
            HarvertDuration = hDuration;
        }

        public override string ToString()
        {
            string seedName = Name;
            // string duration = HarvertDuration.ToString(@"hh\:mm\:ss");

            return $"{seedName}, seed price: {SeedPrice}, harvest price: {HarvertPrice}, time: {HarvertDuration}";
        }
    }

    public class SeedRecord
    {
        public Seed Seed { get; set; }
        public DateTime HarvestTime { get; set; } // default: Now
        public bool IsSpoiled { get; set; } // default: false

        public SeedRecord(Seed seed, DateTime harvestTime, bool isSpoiled)
        {
            Seed = seed;
            HarvestTime = harvestTime;
            IsSpoiled = isSpoiled;
        }

        public override string ToString()
        {
            string seedInfo = Seed.ToString();
            string harvestTime = HarvestTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
            bool isSpoiled = IsSpoiled;

            return $"- Seed: {seedInfo}, Harvest Time: {harvestTime}, Is Spoiled: {isSpoiled}\n";
        }
    }

    public class FarmRecord
    {

        public List<SeedRecord> Garden { get; set; }
        public List<SeedRecord> Inventory { get; set; }
        public int Money { get; set; }

        public FarmRecord(List<SeedRecord> garden, List<SeedRecord> inventory, int money)
        {
            Garden = garden;
            Inventory = inventory;
            Money = money;
        }

        
    }
}
