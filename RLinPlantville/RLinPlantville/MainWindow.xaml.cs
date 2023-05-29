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
using Newtonsoft.Json;


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
        
        private static List<SeedRecord> garden = new List<SeedRecord>();
        private static List<SeedRecord> inventory = new List<SeedRecord>();
        private static int money = 100;
        private static int land = 15;

        private static FarmRecord farmRecord = new FarmRecord(garden, inventory, money);


        public MainWindow()
        {
            InitializeComponent();

            // load player stats 
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                farmRecord = JsonConvert.DeserializeObject<FarmRecord>(json);

                garden = farmRecord.Garden;
                inventory = farmRecord.Inventory;
                money = farmRecord.Money;
                land = 15 - garden.Count;
            }

            load_init_data();
            
        }

        private void load_init_data()
        {
            // seeds
            foreach (Seed seed in seed_list)
            {
                seedDict.Add(seed.Name, seed);

                seed_lb.Items.Add($"{seed.Name} ${seed.SeedPrice}");
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

        private void garden_btn_Click(object sender, RoutedEventArgs e)
        {
            init_title("Garden", "What you would like to harvest?");
            garden_harvest_btn.Visibility = Visibility.Visible;
            inventory_sell_btn.Visibility = Visibility.Collapsed;
            garden_lb.Visibility = Visibility.Visible;
            inventory_lb.Visibility = Visibility.Collapsed;
            seed_lb.Visibility = Visibility.Collapsed;

            load_garden_data();
        }

        private void load_garden_data()
        {

            garden_lb.Items.Clear();
            if (garden.Count == 0) empty_garden_lb();
            else
            {
                foreach (SeedRecord record in garden)
                {
                    garden_lb.Items.Add($"{record.Seed.Name} ({check_seed_status(record, false)})");
                }
            }
        }

        private string check_seed_status(SeedRecord record, Boolean isHarvested)
        {
            string status = "harvest";

            if (!isHarvested)
            {
                DateTime harvestEndTime = record.HarvestTime.Add(TimeSpan.Parse(record.Seed.HarvertDuration));
                DateTime harvestSpoiledTime = harvestEndTime.Add(TimeSpan.Parse("00:05:00"));

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
            inventory_sell_btn.Visibility = Visibility.Visible;
            garden_harvest_btn.Visibility = Visibility.Collapsed;
            garden_lb.Visibility = Visibility.Collapsed;
            inventory_lb.Visibility = Visibility.Visible;
            seed_lb.Visibility = Visibility.Collapsed;

            // clearsss
            inventory_lb.Items.Clear();
            if (inventory.Count == 0) empty_inventory_lb();
            else
            {
                foreach (SeedRecord seedRecord in inventory)
                {
                    inventory_lb.Items.Add($"{seedRecord.Seed.Name} ${seedRecord.Seed.SeedPrice}");
                }
            }
        }

        private void seed_btn_Click(object sender, RoutedEventArgs e)
        {
            init_title("Seeds Emporium", "What you would like to purchase?");
            inventory_sell_btn.Visibility = Visibility.Collapsed;
            garden_harvest_btn.Visibility = Visibility.Collapsed;

            garden_lb.Visibility = Visibility.Collapsed;
            inventory_lb.Visibility = Visibility.Collapsed;
            seed_lb.Visibility = Visibility.Visible;
            
        }

        private void seed_lb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            // check if not null
            if (seed_lb.SelectedItem != null && money > 0 && land > 0)
            {
                string selected = seed_lb.SelectedItem.ToString().Split()[0];
                Seed selectedSeed = seedDict[selected];

                // add into garden
                SeedRecord record = new SeedRecord(selectedSeed, DateTime.Now, false);
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

                foreach (SeedRecord record in inventory)
                {
                    if (!record.IsSpoiled)
                        earn += record.Seed.HarvertPrice;    
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

            List<SeedRecord> itemsRemove = new List<SeedRecord>();

            foreach (SeedRecord record in garden)
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

            MessageBox.Show($"Harvested {itemsRemove.Count} plants.");
        }

        private void garden_lb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (garden_lb.SelectedIndex != -1 && garden.Count > 0)
            {
                int index = garden_lb.SelectedIndex;

                SeedRecord selected = garden[index];

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

        private void harvest_seed(SeedRecord record, Boolean isAll)
        {
             
            string status = check_seed_status(record, false);
            if (status.Equals("harvest") || status.Equals("spoiled"))
            {
                // add into inventory
                inventory.Add(record);

                // add into inventory_lb
                inventory_lb.Items.Add($"{record.Seed.Name} ${record.Seed.SeedPrice}");

                // update land
                land++;
                land_tb.Text = $"Land: {land}";

                // show message 
                if (!isAll)
                    MessageBox.Show($"{record.Seed.Name} harvested.", null, MessageBoxButton.OK);
            } else
            {
                if (!isAll) MessageBox.Show("Nothing to harvest.");
            }
        }

        private void window_closing(object sender, CancelEventArgs e)
        {
            string playerStats = save_player_stats();

            File.WriteAllText(filePath, playerStats);
        }

        private string save_player_stats()
        {
            farmRecord.Garden = garden;
            farmRecord.Inventory = inventory;
            farmRecord.Money = money;

            return JsonConvert.SerializeObject(farmRecord); ;
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
