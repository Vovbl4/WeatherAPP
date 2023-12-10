using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWPF_App.Models
{
    internal class User : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public User() { }

        public User(string login, string email, string password)
        {
            Login = login;
            Email = email;
            Password = password;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
