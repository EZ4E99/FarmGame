using Godot;
using System;


public partial class Phone : Panel
{
    public enum PageType
    {
        Home,
        TodoList,
        Store,
    }
    public PageType CurrentPage = PageType.Home;
    public Panel HomePage;
    public Panel TodoListPage;
    public Panel StorePage;
    public Label DayCount;
    public Label MoneyCount;
    public Label CurrentStatus;
    public TextureButton TodoListButton;
    public Button TodoListBackButton;
    public TextureButton StoreButton;
    public Button StoreBackButton;

    public Player player;

    public override void _Ready()
    {
        HomePage = GetNode<Panel>("Pages/Home");
        TodoListPage = GetNode<Panel>("Pages/TodoList");
        StorePage = GetNode<Panel>("Pages/Store");
        DayCount = GetNode<Label>("UI/DayCount");
        MoneyCount = GetNode<Label>("Pages/Store/Panel/VBoxContainer/MoneyCount");
        CurrentStatus = GetNode<Label>("Pages/Home/Status");
        TodoListButton = GetNode<TextureButton>("Pages/Home/HBoxContainer/TodoList/TodoListIcon");
        TodoListBackButton = GetNode<Button>("Pages/TodoList/BackButton");
        StoreButton = GetNode<TextureButton>("Pages/Home/HBoxContainer/Store/StoreIcon");
        StoreBackButton = GetNode<Button>("Pages/Store/BackButton");

        player = (Player)GetTree().GetFirstNodeInGroup("player");

        Callable OnTodoListButtonPressedCallable = new(this, MethodName.ToTodoList);
        TodoListButton.Connect("pressed", OnTodoListButtonPressedCallable, 0);
        Callable OnStoreButtonPressedCallable = new(this, MethodName.ToStore);
        StoreButton.Connect("pressed", OnStoreButtonPressedCallable, 0);
        Callable OnBackButtonPressedCallable = new(this, MethodName.ToHome);
        TodoListBackButton.Connect("pressed", OnBackButtonPressedCallable, 0);
        StoreBackButton.Connect("pressed", OnBackButtonPressedCallable, 0);

        UpdateDayCount();
        UpdateMoneyCount();
        UpdateCurrentStatus();
    }

    public void ToTodoList()
    {
        CurrentPage = PageType.TodoList;
        HomePage.Hide();
        StorePage.Hide();
        TodoListPage.Show();
    }

    public void ToStore()
    {
        CurrentPage = PageType.Store;
        HomePage.Hide();
        StorePage.Show();
        TodoListPage.Hide();
        UpdateMoneyCount();
    }

    public void ToHome()
    {
        CurrentPage = PageType.Home;
        HomePage.Show();
        StorePage.Hide();
        TodoListPage.Hide();
        UpdateCurrentStatus();
    }

    public void UpdateDayCount()
    {
        DayCount.Text = "DAY - " + player._currentDay;
    }

    public void UpdateMoneyCount()
    {
        MoneyCount.Text = "资金存余：￥" + player._currentMoney;
    }

    public void UpdateCurrentStatus()
    {
        CurrentStatus.Text = "资金存余：￥" + player._currentMoney + "\n"
        + "碳汇贡献：0" + "\n"
        + "社会价值：0";
    }
}
