using MyWPF_App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace MyWPF_App
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private AppDbContext _db;
        public RegisterWindow()
        {
            InitializeComponent();
            _db = new AppDbContext();
        }

        private void UserRegister_Click(object sender, RoutedEventArgs e)
        {
            string login = UserLoginField.Text.Trim();
            string email = UserEmailField.Text.Trim();
            string password = UserPasswordField.Password.Trim();
            if(login.Equals("") || !email.Contains("@") || password.Length < 3)
            {
                MessageBox.Show("Somethingn is wrong");
                return;
            }

            User authUser = _db.Users.Where(el => el.Login == login).FirstOrDefault();
            if(authUser != null)
            {
                MessageBox.Show("User with this login already exists");
                return;
            }

            User user = new User(login, email, Hash(password));
            _db.Users.Add(user);
            _db.SaveChanges();

            UserLoginField.Text = "";
            UserEmailField.Text = "";
            UserPasswordField.Password = "";

            MessageBox.Show("Succesfuly Registered");
        }

        private string Hash(string input)
        {
            byte[] tmp = Encoding.UTF8.GetBytes(input);
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(tmp);
                return Convert.ToBase64String(hash);
            }
        }

        private void LoginWindowbtn_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            AuthWindow window = new AuthWindow();
            window.Show();
            Close();
        }
    }
}
