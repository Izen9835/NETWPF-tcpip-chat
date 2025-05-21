# TCP/IP Chat Server
In this readme, I basically document the different .WPF app specific things that were used in developing this application,  
as well as the TCP/IP related libraries used

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
not sure if the other way round is possible/makes sense
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

## 2. Custom IO Library

## 3. Lambda Functions

![image](https://github.com/user-attachments/assets/097df821-75f4-4308-ae8b-b62c2b336e45)
![image](https://github.com/user-attachments/assets/01c79c21-e6ea-4cea-8cb5-f6756eccd349)
![image](https://github.com/user-attachments/assets/f0ef87a1-7ca8-4e79-9b9b-f5ac5680f2ca)
![image](https://github.com/user-attachments/assets/b0623bbf-fb7a-4b0b-a3a1-aa37d8ddb86b)





