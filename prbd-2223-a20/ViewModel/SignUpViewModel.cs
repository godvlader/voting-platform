using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using MyPoll.Model;
using MyPoll.View;
using PRBD_Framework;

namespace MyPoll.ViewModel {
    internal class SignUpViewModel : ViewModelBase<User, MyPollContext> {

        private string _fullName;
        public string FullName {
            get => _fullName;
            set {
                SetProperty(ref _fullName, value, () => ValidateAll());
            }
        }

        private string _email;
        public string Email {
            get => _email;
            set {
                SetProperty(ref _email, value, () => ValidateAll());
            }
        }

        private string _password;
        public string Password {
            get => _password;
            set {
                SetProperty(ref _password, value, () => ValidateAll());
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword {
            get => _confirmPassword;
            set {
                SetProperty(ref _confirmPassword, value, () => ValidateAll());
            }
        }

        public ICommand CancelCommand { get; set; }


        public ICommand SignUpCommand { get; set; }

        public SignUpViewModel() {
            SignUpCommand = new RelayCommand(SignUp);
            CancelCommand = new RelayCommand(Cancel);
        }
        private void Cancel() {
            var loginWindow = new LoginView();
            var signupWindow = System.Windows.Application.Current.Windows.OfType<SignUpView>().FirstOrDefault();
                signupWindow.Close();
            
            loginWindow.Show();
        }
        private void SignUp() {
            var signupWindow = App.Current.Windows.OfType<SignUpView>().FirstOrDefault();
            if (ValidateAll()) {
                var newUser = new User {
                    FullName = FullName,
                    Email = Email,
                    Password = SecretHasher.Hash(Password)
                };

                Context.Users.Add(newUser);
                Context.SaveChanges();
               
                //send a message to navigate to the PollView 
                NotifyColleagues(App.Messages.MSG_SIGNUP, newUser);

                signupWindow.Close();

            }
        }
        private bool ValidateAll() {
            ClearErrors();

            if (string.IsNullOrEmpty(FullName)) {
                AddError(nameof(FullName), "FullName is required");
            }

            if (string.IsNullOrEmpty(Email)) {
                AddError(nameof(Email), "Email is required");
            }
            string emailRegexPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

            if (!Regex.IsMatch(Email, emailRegexPattern)) {
                AddError(nameof(Email), "Email is not in a valid format");
            }

            if (string.IsNullOrEmpty(Password)) {
                AddError(nameof(Password), "Password is required");
            } else if (Password.Length < 2) {
                AddError(nameof(Password), "length must be >= 3");
            }

            if (string.IsNullOrEmpty(ConfirmPassword)) {
                AddError(nameof(ConfirmPassword), "Confirm password is required");
            } else if (Password != ConfirmPassword) {
                AddError(nameof(ConfirmPassword), "Passwords do not match");
            } else if (ConfirmPassword.Length < 2) {
                AddError(nameof(ConfirmPassword), "length must be >= 3");
            }

            using (var context = new MyPollContext()) {
                var userWithSameFullName = context.Users.SingleOrDefault(u => u.FullName == FullName);
                if (userWithSameFullName != null) {
                    AddError(nameof(FullName), "This full name is already in use");
                }

                var userWithSameEmail = context.Users.SingleOrDefault(u => u.Email == Email);
                if (userWithSameEmail != null) {
                    AddError(nameof(Email), "This email is already in use");
                }
            }

            return !HasErrors;
        }

    }
}
