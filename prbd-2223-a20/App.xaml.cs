using System.Windows;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using MyPoll.Model;
using MyPoll.ViewModel;
using PRBD_Framework;

namespace MyPoll; 

public partial class App : ApplicationBase<User, MyPollContext>  {

    public enum Messages {
        MSG_LOGIN,
        MSG_LOGOUT,
        MSG_REFRESH_DATA,
        MSG_EDIT_POLL,
        MSG_EDITMODE,
        MSG_VOTE_CHANGE,
        MSG_SIGNUP,
        MSG_NEW_POLL,
        MSG_NAME_CHANGED,
        MSG_POLL_TYPE_CHANGED,
        MSG_POLL_CHANGED,
        MSG_DELETE_POLL,
        MSG_SAVE_POLL,
        MSG_CLOSE_TAB,
        MSG_DISPLAY_POLL
    }

    protected override void OnStartup(StartupEventArgs e) {
        PrepareDatabase();

        Register<User>(this, Messages.MSG_LOGIN, user => {
            Login(user);
            NavigateTo<MainViewModel, User, MyPollContext>();
        });

        Register(this, Messages.MSG_LOGOUT, () => {
            Logout();
            NavigateTo<LoginViewModel, User, MyPollContext>();
        });

        Register<User>(this, Messages.MSG_SIGNUP, user => {
            Login(user);
            NavigateTo<MainViewModel, User, MyPollContext>();
        });

    }

    protected override void OnRefreshData() {
        if (CurrentUser?.FullName != null)
            CurrentUser = User.GetByFullName(CurrentUser.FullName);
    }

    public static MyPollContext cntxt { get; private set; } = new MyPollContext();

    private static void PrepareDatabase() {
        // Clear database and seed data
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
        
        // Cold start
        Console.Write("Cold starting database... ");
        Console.WriteLine("FINAL done");

    }


}
