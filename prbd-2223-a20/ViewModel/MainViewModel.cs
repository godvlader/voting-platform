using System.Collections.ObjectModel;
using System.Windows.Input;
using MyPoll.Model;
using MyPoll.View;
using PRBD_Framework;

namespace MyPoll.ViewModel;

public class MainViewModel : ViewModelBase<User, MyPollContext> {

    public ICommand ReloadDataCommand { get; set; }

    public MainViewModel() : base() {
        Users = new ObservableCollection<User>(Context.Users);

        ReloadDataCommand = new RelayCommand(() => {
            // refuser un reload s'il y a des changements en cours
            if (Context.ChangeTracker.HasChanges()) return;
            // permet de renouveller le contexte EF
            App.ClearContext();
            // notifie tout le monde qu'il faut rafraîchir les données
            NotifyColleagues(ApplicationBaseMessages.MSG_REFRESH_DATA);
        });

        Register(App.Messages.MSG_SIGNUP, (Action<User>)OnUserSignedUp);
    }

    private void OnUserSignedUp(User user) {
        
        ConnectedUser = user;
        RaisePropertyChanged(nameof(Title));
        
    }


    public static string Title {
        get => $"My Poll ({CurrentUser?.FullName})";
    }

    private User _connectedUser;
    public User ConnectedUser {
        get => _connectedUser;
        set {
            SetProperty(ref _connectedUser, value);
            RaisePropertyChanged(nameof(Title));
        }
    }

    private ObservableCollection<User> _users;

    public ObservableCollection<User> Users {
        get => _users;
        set => SetProperty(ref _users, value, () => {
            Console.WriteLine("Cette ligne est appelée après assignation de Members");
        });
    }

}

