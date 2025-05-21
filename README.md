# TCP/IP Chat Server
# Highly recommend [this video](https://www.youtube.com/watch?v=I-Xmp-mulz4) by ```Payload``` on making a Chat Server in WPF C#
This program was developed based on the above video  

  
![image](https://github.com/user-attachments/assets/ffe9666e-5f3d-4974-8cac-dbe4cb1416a6)

In this readme, I basically document the different .WPF app specific things that were used in developing this application,  
as well as the TCP/IP related libraries used

# Overall Architecture
## TCP Server
```
TCP-Server/
├── Client.cs
├── Program.cs
├── Net
│   └── IO
│       ├── PacketBuilder.cs
│       └── PacketReader.cs
```
Program.cs mainly uses ```TcpListener``` to listen for incoming new connections
when a new connection is established, it initialises a ```TcpClient``` object using ```Client.cs``` which runs a ```Task.Run(()=>...)``` for handling Messages from that client


## TCP Client
```
TCP-Client/
├── Server.cs
├── MVVM
│   ├── Model
│   │   └── UserModel.cs
│   ├── View
│   │   └── MainWindow.xaml
│   └── ViewModel
│       ├── MainViewMode.cs
│       └── ConnectionHandler.cs
├── Net
│   └── IO
│       ├── PacketBuilder.cs
│       └── PacketReader.cs
```
Server.cs contains the ```Task.Run(()=>...)``` for handling Messages asynchronously,  
which needs to be asynchronous as the client still needs to handle other things such as user input
```MainViewModel.cs``` instantiates a new server object using ```Server.cs```


## 1. View - ViewModel
Learning to use the bindings is a key part of the MVVM model  
The UI part is specified in the ```.xaml (View)``` file while the Property part is specified in the ```.cs (ViewModel)``` part  
You can pretty much bind anything: 
```xaml
<TextBox Height="25"
         DockPanel.Dock="Top"
         Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}"
         IsReadOnly="{Binding ConnectionHandler.IsConnected}">
<Button Height="25"
        DockPanel.Dock="Top"
        Content="{Binding ConnectionHandler.ButtonText}"
        Command="{Binding ConnectToServerCommand}"/>
<ListView Height="380"
          ItemsSource="{Binding Messages}"/>
```
### Single Variable Binding (UI ==> Property)
Any changes on the UI will directly update the Property within the ViewModel
```xaml
<!--MainWindow.xaml-->
<TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}" />
```
```cs
// MainViewModel.cs
public string Username { get; set; }

// use the property directly, e.g.
ConnectionHandler.OnConnect += () => _server.ConnectToServer(Username);
```

### Single Variable Binding (UI <==> Property)
If you want changes in Property to affect the UI also, only the ViewModel needs to be updated, everything stays the same on the View side.
```cs
// MainViewModel.cs
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

public event PropertyChangedEventHandler PropertyChanged;
protected void OnPropertyChanged(string propertyName)
{
   PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

// directly update the Message property for changes to reflect on UI
Message = string.Empty;
```
For some reason, a lot of setup is needed to go from Property ==> UI, for single variables.  
In this project, this feature was used to clear the Message bar everytime a message was sent.

### ObservableCollection (list UI <== Property) 
```xaml
<!--MainWindow.xaml-->
   <ListView ItemsSource="{Binding Users}">
       <ListView.ItemTemplate>
           <DataTemplate>
               <TextBlock Text="{Binding Username}"/>
           </DataTemplate>
       </ListView.ItemTemplate>
   </ListView>
```
```cs
// MainViewMode.cs
// 'global' within the class
public ObservableCollection<UserModel> Users { get; set; }

// Constructor 
        public MainViewModel()
        {
            // a WPF collection that notifies the UI when items are added or removed
            Users = new ObservableCollection<UserModel>();
         ...
         }

// Adding to the users list
Application.Current.Dispatcher.Invoke(() => Users.Add(user));
```
By using an ObservableCollection, the UI thread will constantly listen for changes in the Property  
To change an ObservableCollection you must use Dispatcher.Invoke to synchronously alter the UI thread.


### RelayCommand
Used for triggering button press events
```xaml
<!--MainWindow.xaml-->
           <TextBox.InputBindings>
               <KeyBinding Key="Enter"
                           Command="{Binding SendMessageCommand}"/>
           </TextBox.InputBindings>
```
```cs
// MainViewMode.cs
// global property of class
public RelayCommand SendMessageCommand { get; set; }

// Constructor
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
```
First parameter (Execute) is the func to be executed, Second parameter (canExecute) is the condition for allowing execute.  
So you can grey out and disable a button from being pressed under certain conditions using Bindings

### Event Handlers
```MainViewModel.cs``` instantiates a new server object using ```Server.cs```
```cs
// Server.cs
public event Action connectedEvent;

...

Task.Run(() =>
{
            switch (opcode)
            {
                case 1:
                    connectedEvent?.Invoke();
                    break;
                  ...
            }

}, _cts.Token);
```
```cs
// MainViewModel.cs
class MainViewModel : INotifyPropertyChanged
{
    private Server _server;

    // Constructor
    public MainViewModel()
    {
        /* Event Handlers */
        // essentially subscribes to these events as defined in Server.cs
        // e.g. runs UserConnected() when connectedEvent is invoked
        _server.connectedEvent += UserConnected;
         ...
     }
```
Basically in the event that ```connectedEvent``` is invoked in ```Server.cs```, it will call the function ```UserConnected``` in ```MainViewModel.cs```  
```?.Invoke()``` does the null check ```?``` first before executing the function.  
It will be null if you do not subscribe to it via ```_server.connectedEvent +=```  
Effectively, these ```Delegates```/```Event Handlers``` are ways to call functions in the "parent" class (the one that instantiates an object)  
In this context it is useful for triggering events in the ViewModel when specific commands are recevied from the server

## 2. Custom IO Library
The packetbuilder and packet reader used by both TCP Server and TCP Client are identical.  (logical)  
The packet structure manually built starts with an OpCode, followed by Message Length (bytes), followed by the Message  
the streams used in both builder and reader basically have this buffer system, where if you do smth like ```MemoryStream.Read( )``` it will return data from the buffer while removing the read data from the buffer (FIFO)
### Packet Builder
Uses ```MemoryStream```  
Basically converts data to a byte[] for comms between server and client
```cs
// PacketBuilder.cs
public byte[] GetPacketBytes()
{
   return _ms.ToArray();
}
```
```cs
// Program.cs
TcpClient _client;
...
_client.Client.Send(messagePacket.GetPacketBytes());
```
```TcpClient.Client.Send( )``` expects a byte[], which is why we use this method  
Information is written to the ```MemoryStream``` using WriteByte and Write
```cs
// PacketBuilder.cs
public void WriteMessage(string msg)
{
   var msgLength = msg.Length;
   _ms.Write(BitConverter.GetBytes(msgLength));

   _ms.Write(Encoding.ASCII.GetBytes(msg));
}
```

### Packet Reader
Uses ```NetworkStream```  
```cs
// PacketReader.cs
public string ReadMessage()
{
   byte[] msgBuffer;
   var length = ReadInt32();
   msgBuffer = new byte[length];
   _ns.Read(msgBuffer, 0, length);

   var msg = Encoding.ASCII.GetString(msgBuffer);
   return msg;
}
```
Extends ```BinaryReader``` so that we can use ```ReadByte``` for the OpCode
```cs
// Program.cs
var opcode = _packetReader.ReadByte();
```
```cs
// PacketReader.cs
class PacketReader : BinaryReader
{
private NetworkStream _ns;

public PacketReader(NetworkStream ns) : base(ns)
{
   _ns = ns;
}
```
In this context, base(ns) calls the constructor of the inherited BinaryReader class so they share the same ```NetworkStream```


## 3. Lambda Functions
Not necessary but makes code A LOT cleaner  
Code is difficult to understand if you don't understand Lambda functions  
They are used in various places throughout the codebase  
```cs
// MainViewModel.cs
   ConnectToServerCommand = new RelayCommand(
       o =>
       {
           ConnectionHandler.ToggleConnection();
       },
       o => !string.IsNullOrEmpty(Username));
```
```cs
// MainViewModel.cs
var user = Users.Where(x => x.UID == uid).FirstOrDefault();
```
```cs
// Client.cs
 Task.Run(() => Process(_cts.Token));
```


## 4. CancellationTokenSource
### ```Task.Run(() => ...)```
Both TCP client and server utilise Task.Run() to run specific functions (threads) asynchronously with other processes  
Both essentially use it for processing incoming packets  
In the TCP Client: 
```cs
// Server.cs (in TCP Client)

// In Constructor
ReadPackets();

// Method definition
 private void ReadPackets()
 {
     Task.Run(() =>
     {
         ...
                            case 11:
                                // the ACK signal from server that we can safely disconnect
                                DisconnectFromServer();
                                break;
         ...
     }, _cts.Token);
 }

public void DisconnectFromServer()
{
   if (_client.Connected)
   {
       _cts.Cancel(); // to stop the while loop in ReadPackets
         ...
   }
}
```
Or alternatively in the TCP Server:
```cs
// Client.cs (in TCP Server)

// In Constructor
Task.Run(() => Process(_cts.Token));

// Function Definition
void Process(CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        try
        {
            var opcode = _packetReader.ReadByte();
            switch (opcode)
            {
                case 5: // receive message
                  ...
                  break;
                case 10: // receive disconnect request
                  ...
                  break;
                default:
                    break;
            }

        } 
        catch (Exception ex)
        {
            _cts.Cancel();
        }
    }
}
```
Both client and server utilised CTS in different manners, but for the same purpose of ensuring graceful termination of threads upon disconnection from the server.  



![image](https://github.com/user-attachments/assets/097df821-75f4-4308-ae8b-b62c2b336e45)
![image](https://github.com/user-attachments/assets/01c79c21-e6ea-4cea-8cb5-f6756eccd349)
![image](https://github.com/user-attachments/assets/f0ef87a1-7ca8-4e79-9b9b-f5ac5680f2ca)
![image](https://github.com/user-attachments/assets/b0623bbf-fb7a-4b0b-a3a1-aa37d8ddb86b)





