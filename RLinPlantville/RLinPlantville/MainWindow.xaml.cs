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

        Dictionary<string, Seed> seedDict = new Dictionary<string, Seed>();
        
        private static List<SeedRecord> garden = new List<SeedRecord>();
        private static List<SeedRecord> inventory = new List<SeedRecord>();
        private static int money = 100;
        private static int land = 15;

        private static FarmRecord farmRecord = new FarmRecord(garden, inventory, money);


        public MainWindow()
        {
            InitializeComponent();

            foreach (Seed seed in seed_list)
            {
                seedDict.Add(seed.Name, seed);

                seed_lb.Items.Add($"{seed.Name} ${seed.SeedPrice}");
            }

            money_tb.Text = $"Money: ${money.ToString()}";
            land_tb.Text = $"Land: {land.ToString()}";

            if (inventory.Count == 0) empty_inventory_lb();
            if (garden.Count == 0) empty_garden_lb();
            
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

            // clear 
            garden_lb.Items.Clear();
            if (garden.Count == 0) empty_garden_lb();
            else
            {
                foreach (SeedRecord record in garden)
                {
                    // FIXME: count down from DateTime.Now + record.Seed.HarvestDuration 
                    garden_lb.Items.Add($"{record.Seed.Name} at {record.HarvestTime} (harvest)");
                }
            }
        }

        private void inventory_btn_Click(object sender, RoutedEventArgs e)
        {
            init_title("Inventory", "$10 each trip to farmer's market.");
            inventory_sell_btn.Visibility = Visibility.Visible;
            garden_harvest_btn.Visibility = Visibility.Collapsed;
            garden_lb.Visibility = Visibility.Collapsed;
            inventory_lb.Visibility = Visibility.Visible;
            seed_lb.Visibility = Visibility.Collapsed;

            // clear
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
                money_tb.Text = $"Money: {money}";
                land--;
                land_tb.Text = $"Land: {land}";

                // show message
                MessageBox.Show($"You purchased {selectedSeed.Name}");

            } else
            {
                Console.WriteLine($"seed_lb.SelectedItem is null and money {money} with land {land}");
                MessageBox.Show("You don't have enough money.");
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
                    money_tb.Text = $"Money: {money}";

                    inventory_result_message_show(-10);
                }
            } else
            {
                int earn = 0;

                foreach (SeedRecord record in inventory)
                {
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

                // FIXME: check valid items which are all harvested status
                itemsRemove.Add(record);

            }

            garden.RemoveAll(record => itemsRemove.Contains(record));

            MessageBox.Show($"Harvested {itemsRemove.Count} plants.");

            // FIXME: remove garden_lb 
            empty_garden_lb();
        }

        private void garden_lb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (garden_lb.SelectedIndex != -1 && garden.Count > 0)
            {
                int index = garden_lb.SelectedIndex;

                SeedRecord selected = garden[index];
                harvest_seed(selected, false);

                // update garden record
                garden.Remove(selected);

                // remove the selectedItem
                if (selected != null)
                {

                    garden_lb.Items.Remove(garden_lb.SelectedItem);
                }

                if (garden_lb.Items.Count == 0) empty_garden_lb();
            } else
            {
                MessageBox.Show("Nothing to harvest.");
            }
        }

        private void harvest_seed(SeedRecord record, Boolean isAll)
        {
            // TODO: check status is harvest and is not spoiled 

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
            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            string playerStats = save_player_stats();

            // Save player stats to a log file
            string logFilePath = "player_stats.txt";
            File.WriteAllText(logFilePath, playerStats);
        }

        private string save_player_stats()
        {
            farmRecord.Garden = garden;
            farmRecord.Inventory = inventory;
            farmRecord.Money = money;

            string res = JsonConvert.SerializeObject(farmRecord);
            return res;
        }

    }

    public class Seed
    {
        public string Name { get; set; }
        public int SeedPrice { get; set; }
        public int HarvertPrice { get; set; }
        public TimeSpan HarvertDuration { get; set; } // "hh:mm:ss"

        public Seed(string name, int sPrice, int hPrice, string hDuration)
        {
            Name = name;
            SeedPrice = sPrice;
            HarvertPrice = hPrice;
            HarvertDuration = TimeSpan.Parse(hDuration);
        }

        public override string ToString()
        {
            string seedName = Name;
            string duration = HarvertDuration.ToString(@"hh\:mm\:ss");

            return $"{seedName}, seed price: {SeedPrice}, harvest price: {HarvertPrice}, time: {duration}";
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
