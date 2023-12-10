using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using MyWPF_App.Models;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace MyWPF_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string API_KEY = "201bb3db814fd5ef026d58560813059a";
        private AppDbContext _db;
        private ObservableCollection<User> _users;


        public MainWindow()
        {
            InitializeComponent();
            MainScreen.IsChecked = true;
            _db = new AppDbContext();
            LoadUsers();
            UsersListBox.ItemsSource = _users;

            if (!File.Exists("user.xml"))
                ShowAuthWindow();

            XmlSerializer xml = new XmlSerializer(typeof(AuthUser));
            using (FileStream file = new FileStream("user.xml", FileMode.Open))
            {
                AuthUser auth = (AuthUser)xml.Deserialize(file);
                UserNameLabel.Content = auth.Login;
            }

            SetDefaultSize.IsSelected = true;
        }



        private void LoadUsers()
        {
            _users = new ObservableCollection<User>(_db.Users.ToList());
            UsersListBox.ItemsSource = _users;
        }

        private async void GetWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            string city = UserCityTextBox.Text.Trim();
            if(city.Length <2 )
            {
                MessageBox.Show("Set your City");
                return;
            }

            string data = "";

            try 
            {
                data = await GetWeather(city);
                var json = JObject.Parse(data);
                string temp = json["main"]["temp"].ToString();
                WeatherResults.Content = $"In city {city}: {temp} °C";
            }
            catch(HttpRequestException ex) 
            { 
                MessageBox.Show("Introduce a valid city");
                WeatherResults.Content = "";
            }
        }

        private async Task<string> GetWeather(string city)
        {
            HttpClient client = new HttpClient();
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={API_KEY}&units=metric";
            return await client.GetStringAsync(url);
            
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string objName = ((RadioButton)sender).Name;

            StackPanel[] panels = { MainScreenPanel, PeopleScreenPanel, AccountScreenPanel, NotesScreenPanel};
            foreach(var pan in panels)
            {
                pan.Visibility = Visibility.Hidden;
            }

            switch(objName)
            {
                case "MainScreen": MainScreenPanel.Visibility = Visibility.Visible; break;
                case "PeopleScreen": PeopleScreenPanel.Visibility = Visibility.Visible; break;
                case "AccountScreen": AccountScreenPanel.Visibility = Visibility.Visible; break;
                case "NotesScreen": NotesScreenPanel.Visibility = Visibility.Visible; break;
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            var userToDelete = _db.Users.SingleOrDefault(u => u.Login == UserLoginTextBox.Text);
            if (userToDelete != null)
            {
                _db.Users.Remove(userToDelete);
                _db.SaveChanges();

                LoadUsers();
                MessageBox.Show("Deleted :)");
                if(userToDelete.Login == UserNameLabel.Content.ToString())
                {
                    ExitButton_Click(sender, e);
                }
            }
            else
                MessageBox.Show("This user does not exists");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            File.Delete("user.xml");
            ShowAuthWindow();
        }  
        
        private void ShowAuthWindow()
        {
            Hide();
            AuthWindow window = new AuthWindow();
            window.Show();
            Close();
        }

        private void UserChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = UserLogin.Text.Trim();
            string email = UserEmail.Text.Trim();
            if (login.Equals("") || !email.Contains("@"))
            {
                MessageBox.Show("Somethingn is wrong");
                return;
            }

            AppDbContext db = new AppDbContext();
            int countUsers = Convert.ToInt32(db.Users.Count(el => el.Login == login));
            if(countUsers != 0 && login.Equals(UserNameLabel.Content))
            {
                MessageBox.Show("User with this login already exists");
                return;
            }

            User user = db.Users.FirstOrDefault(el => el.Login == UserNameLabel.Content.ToString());
            user.Email = email; 
            user.Login = login;
            db.SaveChanges();

            UserNameLabel.Content = login;
            MessageBox.Show("Changed :)");

            User authUser = db.Users.Where(_user => _user.Login == login).FirstOrDefault();

            AuthUser auth = new AuthUser(login, authUser.Email);
            XmlSerializer xml = new XmlSerializer(typeof(AuthUser));
            File.Delete("user.xml");
            using (FileStream file = new FileStream("user.xml", FileMode.CreateNew))
            {
                xml.Serialize(file, auth);
            }
        }

        private void MenuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            bool isFolder = (bool)openFileDialog.ShowDialog();

            if(isFolder)
            {
                using(Stream stream = File.Open(openFileDialog.FileName, FileMode.Open))
                {
                    using (StreamReader writer = new StreamReader(stream))
                    {
                        UserNotesTextBox.Text = writer.ReadToEnd();
                    }
                }
            }
        }

        private void MenuSaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveTextToFile();
        }

        private void TimesNewRomanSetText_Click(object sender, RoutedEventArgs e)
        {
            UserNotesTextBox.FontFamily = new System.Windows.Media.FontFamily("Times New Roman");
            VerdanaSetText.IsChecked = false;
        }

        private void VerdanaSetText_Click(object sender, RoutedEventArgs e)
        {
            UserNotesTextBox.FontFamily = new System.Windows.Media.FontFamily("Verdana");
            TimesNewRomanSetText.IsChecked = false;
        }


        private void SelectFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem comboBoxItem = (ComboBoxItem)SelectFontSize.SelectedItem;
            int fontSize = Convert.ToInt32(comboBoxItem.Tag);
            UserNotesTextBox.FontSize = fontSize;
        }

        private void MenuNewFile_Click(object sender, RoutedEventArgs e)
        {
            if (UserNotesTextBox.Text.Trim().Equals(""))
                return;
            SaveTextToFile();
            UserNotesTextBox.Text = "";
        }

        private void SaveTextToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            bool isFolder = (bool)saveFileDialog.ShowDialog();
            if (isFolder)
            {
                using (Stream file = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate))
                {
                    using (StreamWriter writer = new StreamWriter(file))
                    {
                        writer.Write(UserNotesTextBox.Text);
                    }
                }
            }
        }
    }
}
