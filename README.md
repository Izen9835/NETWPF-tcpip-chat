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
```xaml
<!--MainWindow.xaml-->
<TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}" />
```
```cs
// MainViewMode.cs
public string Username { get; set; }
```

### Single Variable Binding (UI <==> Property)

### ObservableCollection

### RelayCommand

## 2. Custom IO Library

## 3. Lambda Functions

![image](https://github.com/user-attachments/assets/097df821-75f4-4308-ae8b-b62c2b336e45)
![image](https://github.com/user-attachments/assets/01c79c21-e6ea-4cea-8cb5-f6756eccd349)
![image](https://github.com/user-attachments/assets/f0ef87a1-7ca8-4e79-9b9b-f5ac5680f2ca)
![image](https://github.com/user-attachments/assets/b0623bbf-fb7a-4b0b-a3a1-aa37d8ddb86b)





