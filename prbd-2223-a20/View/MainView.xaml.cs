using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using MyPoll.Model;
using MyPoll.ViewModel;
using PRBD_Framework;
using static MyPoll.App;

namespace MyPoll.View;

public partial class MainView : WindowBase {
    public MainView() {
        InitializeComponent();

        Register<Poll>(App.Messages.MSG_NEW_POLL,
            poll => DoDisplayPoll(poll, true));

        Register<Poll>(App.Messages.MSG_EDIT_POLL,
         poll => DoDisplayPoll(poll, false));


        Register<Poll>(App.Messages.MSG_DISPLAY_POLL,
            poll => DoDisplayDetail(poll));

        Register<Poll>(App.Messages.MSG_EDIT_POLL,
           poll => DoDisplayPoll(poll, false));

        Register<Poll>(App.Messages.MSG_CLOSE_TAB,
            poll => DoCloseTab(poll));

        Register<Poll>(App.Messages.MSG_DELETE_POLL,
            poll => DoCloseTab(poll));

        Register<Poll>(App.Messages.MSG_SAVE_POLL,
           poll => DoCloseTab(poll));

        Register<Poll>(App.Messages.MSG_NAME_CHANGED,
            poll => DoRenameTab(string.IsNullOrEmpty(poll.Name) ? "<new>" : poll.Name));

        Register<Poll>(App.Messages.MSG_REFRESH_DATA, poll => DoRefreshPoll(poll));
    }

    private void DoRefreshPoll(Poll poll) {
        if (poll != null) {
            DoCloseTab(poll);
            OpenTab(poll.Name, poll.Name, () => new PollDetailView(poll, false));
        }
    }


    private void MenuLogout_Click(object sender, System.Windows.RoutedEventArgs e) {
        NotifyColleagues(App.Messages.MSG_LOGOUT);
    }

    private void WindowBase_KeyDown(object sender, KeyEventArgs e) {
        if (e.Key == Key.Q && Keyboard.IsKeyDown(Key.LeftCtrl))
            Close();

    }

    private void DoDisplayPoll(Poll poll, bool isNew) {
        if (poll != null) {
            if (isNew) {
                // Show the PollAddView for a new poll
                OpenTab("<New Poll>", poll.Name, () => new PollAddView(poll, true));
            } else {
                // Show the PollAddView for an existing poll
                OpenTab(poll.Name, poll.Name, () => new PollAddView(poll, false));
            }
        }
    }

    private void DoDisplayDetail(Poll poll ) {
        if (poll != null) {
            OpenTab(poll.Name, poll.Name, () => new PollDetailView(poll,false));

        }
    }

    private void OpenTab(string header, string tag, Func<UserControl> createView) {
        var tab = tabControl.FindByTag(tag);
        if (tab == null)
            tabControl.Add(createView(), header, tag);
        else
            tabControl.SetFocus(tab);
    }

    private void DoRenameTab(string header) {
        if (tabControl.SelectedItem is TabItem tab) {
            MyTabControl.RenameTab(tab, header);
            tab.Tag = header;
        }
    }

    private void DoCloseTab(Poll poll) {
        tabControl.CloseByTag(string.IsNullOrEmpty(poll.Name) ? "<New Poll>" : poll.Name);
    }



    // Nécessaire pour pouvoir Dispose tous les UC et leur VM
    protected override void OnClosing(CancelEventArgs e) {
        base.OnClosing(e);
        tabControl.Dispose();
    }

}


