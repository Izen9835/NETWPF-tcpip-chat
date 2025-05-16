using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.MVVM.ViewModel
{
    class ConnectionHandler : INotifyPropertyChanged
    {
        private bool _isConnected;
        private string _buttonText;

        public event Action OnConnect;
        public event Action OnDisconnect;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsConnected
        {
            get { return _isConnected; }
            private set // prevent mainviewmodel from directly setting this property
            {
                if (value != _isConnected)
                {
                    _isConnected = value;                   
                    OnPropertyChanged(nameof(IsConnected));
                    UpdateButtonText();
                }
            }
        }

        public string ButtonText
        {
            get { return _buttonText; }
            private set
            {
                if (value != _buttonText)
                {
                    _buttonText = value;
                    OnPropertyChanged(nameof(ButtonText));
                    UpdateButtonText();
                }
            }
        }


        // constructor
        public ConnectionHandler()
        {
            _isConnected = false;
            UpdateButtonText();
        }

        public void ToggleConnection()
        {
            if (_isConnected)
            {
                OnDisconnect?.Invoke(); // invoke the disconnect routine (run by MainViewModel)
                IsConnected = false; // then set to false. which will trigger INotifyPropertyChanged, changing the UI.
            } else
            {
                OnConnect?.Invoke();
                IsConnected = true;
            }
        }

        private void UpdateButtonText()
        {
            // directly change the ButtonText property as we are using INotifyPropertyChanged to update the UI
            // if connected, show disconnect button, vice versa.
            ButtonText = IsConnected ? "Disconnect" : "Connect";
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }
}
