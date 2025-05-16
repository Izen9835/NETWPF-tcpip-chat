using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public RelayCommand ConnectToServerCommand { get; set; }

        public string Username { get; set; }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message)); // triggers the UI update
                }
            }
        }
        public RelayCommand SendMessageCommand { get; set; }

        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<string> Messages { get; set; }

        private Server _server;

        // event handler to clear message bar when send message
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ConnectionHandler ConnectionHandler { get; set; }


        // Constructor
        public MainViewModel()
        {
            // a WPF collection that notifies the UI when items are added or removed
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<string>();

            _server = new Server();

            /* Event Handlers */
            // essentially subscribes to these events as defined in Server.cs
            // e.g. runs UserConnected() when connectedEvent is invoked
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectedEvent += RemoveUser;


            ConnectionHandler = new ConnectionHandler();
            ConnectionHandler.OnConnect += () => _server.ConnectToServer(Username);
            ConnectionHandler.OnDisconnect += () =>
            {
                _server.SendDisconnectMessageToServer();
                Application.Current.Dispatcher.Invoke(() => Users.Remove(getUserFromUsername(Username)));
            };



            // in RelayCommand, the 1st parameter is the execute command
            // the 2nd parameter is the CanExecute condition

            // command is allowed to run IF the username is NOT null or empty
            ConnectToServerCommand = new RelayCommand(
                o =>
                {
                    ConnectionHandler.ToggleConnection();
                },
                o => !string.IsNullOrEmpty(Username));

            // greys out the send button if the textbox is empty
            SendMessageCommand = new RelayCommand(
                o =>
                {
                    _server.SendMessageToServer(Message);
                    Message = string.Empty;
                },
                o =>
                {
                    return !string.IsNullOrEmpty(Message) && ConnectionHandler.IsConnected;
                }
            );

        }

        private UserModel getUserFromUsername(string username)
        {
            var gotUser = Users.FirstOrDefault(o => o.Username == username);
            // assume that username MUST be in the Users list
            return gotUser;
        }

        private void RemoveUser()
        {
            var uid = _server.PacketReader.ReadMessage();
            var user = Users.Where(x => x.UID == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived()
        {
            var msg = _server.PacketReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
        }

        private void UserConnected()
        {
            var user = new UserModel
            {
                Username = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage()
            };

            // if the user is not in the list yet then add them
            if (!Users.Any(x => x.UID == user.UID))
            {
                // UI thread will always listen for changes in ObservableCollections
                // So if you make a change in ObservableCollection, then you must do it via 
                // Dispatcher.Invoke() to run it synchronously on the UI thread
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }
    }
}
