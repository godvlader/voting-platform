using System;
using System.Windows.Input;
using MyPoll.Model;
using PRBD_Framework;
using System.Linq;
using MyPoll.View;
using System.Text.RegularExpressions;

namespace MyPoll.ViewModel;

public partial class LoginViewModel : ViewModelBase<User, MyPollContext> {
    public ICommand LoginCommand { get; set; }

    public ICommand LoginAsCommand { get; }

    private string _email;

    public string Email {
        get => _email;
        set => SetProperty(ref _email, value, () => Validate());
    }

    private string _password;

    public string Password {
        get => _password;
        set => SetProperty(ref _password, value, () => Validate());
    }

    public LoginViewModel() {
        LoginCommand = new RelayCommand(LoginAction,
            () => { return _email != null && _password != null && !HasErrors; });

        LoginAsCommand = new RelayCommand<string>(LoginAs);

        OpenSignUpViewCommand = new RelayCommand(OpenSignUpView);
    }

    private void LoginAs(string credentials) {
        var parts = credentials.Split('|');
        Email = parts[0];
        Password = parts[1];
        LoginAction();
    }


    private void LoginAction() {
        if (Validate()) {
            User user = Context.Users.SingleOrDefault(u => u.Email == Email);
            NotifyColleagues(App.Messages.MSG_LOGIN, user);

        }
    }

    public override bool Validate() {
        ClearErrors();

        User user = Context.Users.SingleOrDefault(u => u.Email == Email);
        Regex emailRegex = MyRegex();

        if (string.IsNullOrEmpty(Email))
            AddError(nameof(Email), "required");
        else if (!emailRegex.IsMatch(Email))
            AddError(nameof(Email), "invalid format");
        else if (user == null)
            AddError(nameof(Email), "does not exist");
        else             if (string.IsNullOrEmpty(Password))
                AddError(nameof(Password), "required");
            else if (user != null && !SecretHasher.Verify(Password, user.Password))
            AddError(nameof(Password), "wrong password");

        return !HasErrors;
    }

    public ICommand OpenSignUpViewCommand { get; }

    private void OpenSignUpView() {
        var signUpView = new SignUpView();
        var loginWindow = System.Windows.Application.Current.Windows.OfType<LoginView>().FirstOrDefault();
        loginWindow?.Close();
        signUpView.ShowDialog();
    }

    protected override void OnRefreshData() {
    }

    [GeneratedRegex("^[\\w-]+(\\.[\\w-]+)*@([\\w-]+\\.)+[a-zA-Z]{2,7}$")]
    private static partial Regex MyRegex();
}
