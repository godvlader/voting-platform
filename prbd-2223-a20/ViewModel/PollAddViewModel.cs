using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using MyPoll.Model;
using MyPoll.ViewModel;
using PRBD_Framework;
using System.IO;
using SQLitePCL;

namespace MyPoll.ViewModel {
    public class PollAddViewModel : ViewModelCommon {

        public ICommand Save { get; set; }
        public ICommand AddParticipant { get; set; }
        public ICommand RemoveParticipant {
            get {
                return new RelayCommand<User>(user => {
                    //check if the vote count is >= 1
                    if (user.VoteCount >= 1) {
                        var result = MessageBox.Show("Are you sure you want to delete this user and the votes linked to it?",
                                                     "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result != MessageBoxResult.Yes) {
                            //if user clicked no, do not delete
                            return;
                        }
                    }
                    if (user != null) {
                        var votesToDelete = Poll.Choices.SelectMany(c => c.Votes.Where(v => v.user == user)).ToList();
                        _usersToParticipate.Remove(user);
                        foreach (var vote in votesToDelete) {
                            Context.Votes.Remove(vote);
                        }
                    }
                });
            }
           }
        public ICommand AddChoiceCommand { get; set; }
        public ICommand RemoveChoiceCommand {
            get {
                return new RelayCommand<ChoiceViewModel>(choice => {
                    //check if the vote count is >= 1
                    if (choice.VoteCount >= 1) {
                        var result = MessageBox.Show("Are you sure you want to delete this choice and the votes linked to it?",
                                                     "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result != MessageBoxResult.Yes) {
                            // if user clicked no, do not delete
                            return;
                        }
                    }
                    if (choice != null) {
                        _choicesToAdd.Remove(choice);
                        RaisePropertyChanged(nameof(ChoiceViewModel));
                        RaisePropertyChanged(nameof(ChoicesToAdd));
                        NotifyColleagues(ApplicationBaseMessages.MSG_REFRESH_DATA);
                    }
                });
            }
        }

        public ICommand EditChoice { get; set; }

        //EDIT MODE //

        private bool _isEditMode;
        public bool IsEditing {
            get => _isEditMode;
            set {
                _isEditMode = value;
                RaisePropertyChanged(nameof(IsEditing));
            }
        }

        public PollAddViewModel() { }

        private ObservableCollection<User> _usersToParticipate;

        public ObservableCollection<User> UsersToParticipate {
            get => _usersToParticipate;
            set {
                //calculate the vote count for each user based on the current poll
                foreach (var user in value) {
                    user.VoteCount = user.GetVoteCountForPoll(Poll);
                    Console.WriteLine(user.FullName + "<========USERRRRRRRRR");
                    Console.WriteLine(Poll.Id + "<======= POLLLLLLLLLLLLLL");
                    Console.WriteLine(user.VoteCount + "<-------------- VOTE COUNTER HERE <-----------");
                }

                SetProperty(ref _usersToParticipate, value, () => AddParticipantAction());
            }
        }

        private ObservableCollection<ChoiceViewModel> _choicesToAdd = new ObservableCollection<ChoiceViewModel>();

        public ObservableCollection<ChoiceViewModel> ChoicesToAdd {
            get => _choicesToAdd;
            set => _choicesToAdd = value;
        }

        public ICommand Cancel { get; set; }

        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users {
            get {
                _users ??= new ObservableCollection<User>(GetUsers());
                return _users;
            }
        }
        private static IEnumerable<User> GetUsers() => User.GetUsers();

        private Poll _poll;
        public bool IsExisting => !_isNew;

        public static string Creator {
            get => $"Created By {CurrentUser?.FullName}";
        }

        public Poll Poll {
            get { return _poll; }
            set => SetProperty(ref _poll, value);
        }
        private bool _isNew;

        public bool IsNew {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }

        public string Name {
            get => Poll?.Name;
            set => SetProperty(Poll.Name, value, Poll, (m, v) => {
                m.Name = v;
                ValidateName(Poll?.Id);
                RaisePropertyChanged(nameof(Name));
                NotifyColleagues(App.Messages.MSG_NAME_CHANGED, Poll);
            });
        }

        public ICommand AddRandomChoiceCommand { get; private set; }
        public void AddRandomChoice() {
            var randomWord = GetRandomWord(3, 5);
            var newChoice = new ChoiceViewModel(new Choice(), this) { Label = randomWord};
            // UNE A LA FOIS
            ChoicesToAdd.Add(newChoice);
            
        }
        public static string GetRandomWord(int minLength, int maxLength) {
            Random random = new Random();
            string vowels = "aeiou";
            string consonants = "bcdfghjklmnpqrstvwxyz";

            //random length within therange
            int length = random.Next(minLength, maxLength + 1);

            StringBuilder word = new StringBuilder(length);

            for (int i = 0; i < length; i++) {
                string pool = i % 2 == 0 ? consonants : vowels; // alternating between consonants and vowels
                char c = pool[random.Next(pool.Length)];
                if (i == 0) {
                    c = char.ToUpper(c); // capitalize the first letter
                }
                word.Append(c);
            }
            return word.ToString();
        }



        //ADD MYSELF
        public ICommand AddMyselfCommand { get; }

        private void AddMyselfCommandAction() {
            if (!UsersToParticipate.Contains(CurrentUser)) {
                // Check if CurrentUser is already in the collection based on Id and Name
                if (!_usersToParticipate.Any(u => u.Id == CurrentUser.Id && u.FullName == CurrentUser.FullName)) {
                    _usersToParticipate.Add(CurrentUser);
                }
            }
        }

        //ADD ALL USERS
        public ICommand AddAllParticipantsCommand { get; }

        private void AddAllParticipantsAction() {
            // add all users to the list
            UsersToParticipate.Clear();
            foreach (var user in Users) {
                _usersToParticipate.Add(user);
            }
        }

        //GET POLL TYPE TO DISPLAY
        public static PollType[] PollTypeValues => Poll.GetTypes();

        //GET POLLTYPE VALUE
        private PollType _selectedPollType;
        public PollType SelectedPollType {
            get => _selectedPollType;
            set {
                _selectedPollType = value;
                RaisePropertyChanged(nameof(SelectedPollType));
            }
        }

        //GET USER ID FROM COMBOBOX TO ADD TO PARTICIPATION TABLE
        private User _selectedUser;
        public User SelectedUser {
            get => _selectedUser;
            set {
                _selectedUser = value;
                RaisePropertyChanged(nameof(SelectedUser));
            }
        }
        //ADD PARTICIPANT TO THE LIST
        public void AddParticipantAction() {
            Console.WriteLine("AddParticipantAction");
            if (_selectedUser != null) {
                // If the user is not already in the list, add them
                if (!_usersToParticipate.Contains(_selectedUser)) {
                    _usersToParticipate.Add(_selectedUser);
                }
            }
        }

        private bool CanAddParticipant() {
            return _selectedUser != null && !_usersToParticipate.Contains(_selectedUser);
        }

        //ADD CHOICES TO LIST
        private string _label;
        public string Label {
            get { return _label; }
            set {
                if (!string.Equals(_label, value)) {
                    _label = value;
                    RaisePropertyChanged();
                }
            }
        }
        public void AddChoiceAction() {
            // Validate if label is not empty
            ClearErrors();
            if (string.IsNullOrWhiteSpace(Label)) {
                AddError(nameof(Label), "Choice cannot be empty.");
                return;
            }

            // Validate if label length is >= 3
            if (Label.Length < 3) {
                AddError(nameof(Label), "Choice must be at least 3 characters long.");
                return;
            }

            // Validate if the choice exists already
            if (ChoicesToAdd.Any(c => c.Label == Label)) {
                AddError(nameof(Label), "This choice already exists.");
                return;
            }
            
            // If all validations pass, create and add the new choice
            var newChoiceViewModel = new ChoiceViewModel(new Choice { Label = Label }, this);
            ChoicesToAdd.Add(newChoiceViewModel);
            var newChoiceModel = new Choice() {
                Label = newChoiceViewModel.Label,
                PollId = Poll.Id
            };
            Context.Choices.Add(newChoiceModel);
            Label = "";
        }

        //POLL VALIDATIONS
        private bool _closed;
        public bool Closed {
            get { return _closed; }
            set {
                if (_closed != value) {
                    _closed = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<Comment> Comments { get; internal set; }
        public ObservableCollection<Choice> Choices { get; internal set; }
        public ObservableCollection<User> Participants { get; internal set; }

        public bool ValidateName(int? currentPollId = null) {
            ClearErrors();

            if (string.IsNullOrWhiteSpace(Name)) {
                AddError(nameof(Name), "Name is required.");
            } else if (Name.Length < 3 || Name.Length > 50) {
                AddError(nameof(Name), "Name must be between 3 and 50 characters long.");
            } else {
                using (var context = new MyPollContext()) {
                    var pollWithSameName = context.Polls.FirstOrDefault(u => u.Name == Name);

                    //explicit check to make sure we are not validating the poll against itself
                    if (pollWithSameName != null && pollWithSameName.Id != currentPollId) {
                        AddError(nameof(Name), "This Poll already exists.");
                    }
                }
            }

            return !HasErrors;
        }


        //SAVE NEW POLL
        public override void SaveAction() {
            bool isClosed = Closed;
            bool isValidName = IsNew ? ValidateName() : ValidateName(Poll.Id);
            if (isValidName) {
                // If the poll is new, assign the properties and add it to the context
                if (IsNew) {
                    Poll.Closed = isClosed;
                    Poll.Name = Name;
                    Poll.Type = SelectedPollType;
                    Poll.CreatorId = CurrentUser.Id;

                    // Add new choices
                    foreach (var choiceViewModel in ChoicesToAdd) {
                        var newChoice = new Choice() {
                            Label = choiceViewModel.Label,
                            PollId = Poll.Id
                        };
                        Poll.Choices.Add(newChoice);
                    }

                    Context.Add(Poll);
                    IsNew = false;
                } else {
                    Poll.Closed = isClosed;
                    Poll.Name = Name;
                    Poll.Type = SelectedPollType;
                    Poll.Participants = Participants;

                    //update existing choices and add new choices
                    foreach (var choiceViewModel in ChoicesToAdd) {
                        var existingChoice = Poll.Choices.FirstOrDefault(c => c.Id == choiceViewModel.Id);
                        if (existingChoice != null) {
                            // Update the existing choice
                            existingChoice.Label = choiceViewModel.Label;
                        } else {
                            // Add a new choice
                            var newChoice = new Choice() {
                                Label = choiceViewModel.Label,
                                PollId = Poll.Id
                            };
                            Poll.Choices.Add(newChoice);
                        }
                    }

                    // Delete removed choices
                    foreach (var choice in Poll.Choices.ToList()) {
                        if (!ChoicesToAdd.Any(c => c.Id == choice.Id)) {
                            Context.Choices.Remove(choice);
                        }
                    }

                    Context.Update(Poll);
                }

                // Save the changes to the Poll and Choices entities
                Context.SaveChanges();

                // Create Participation objects for each user in _usersToParticipate
                Context.Participations.AddRange(_usersToParticipate.Select(user => new Participation {
                    UserId = user.Id,
                    PollId = Poll.Id
                }));

                Context.SaveChanges();

                NotifyColleagues(App.Messages.MSG_SAVE_POLL, Poll);
                NotifyColleagues(ApplicationBaseMessages.MSG_REFRESH_DATA);
            }
        }

        private string _originalName;
        private PollType _originalType;

        private bool CanSaveAction() {
            bool hasChanges = Closed != Poll.Closed
                              || UsersToParticipate?.Count != Poll.Participants?.Count
                              || ChoicesToAdd?.Count != Poll.Choices?.Count
                              || !string.Equals(Name, _originalName)
                              || SelectedPollType != _originalType;            

            bool hasParticipants = UsersToParticipate != null && UsersToParticipate.Count > 0;
            bool hasChoices = ChoicesToAdd != null && ChoicesToAdd.Count > 0;
            bool hasName = !string.IsNullOrEmpty(Name);

            // Check if there are any choices with validation errors
            bool choicesAreValid = ChoicesToAdd != null && !ChoicesToAdd.Any(choice => choice.HasErrors);

            return hasChanges && hasParticipants && hasChoices && choicesAreValid && (!IsNew || hasName);
        }




        //CANCEL NEW POLL
        public override void CancelAction() {
            foreach (var entry in Context.ChangeTracker.Entries()) {
                switch (entry.State) {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
            // Revert any changes to the Poll
            Poll.Reload();
            // Notify all bindings to update
            RaisePropertyChanged();
            // Notify to refresh data
            NotifyColleagues(ApplicationBaseMessages.MSG_REFRESH_DATA);
            // Notify to close the tab
            NotifyColleagues(App.Messages.MSG_CLOSE_TAB, Poll);
        }


        private bool CanCancelAction() {
            return true;
        }


        public bool ComboboxEnabled {
            get {
                foreach (var user in UsersToParticipate) {
                    if (user.VoteCount > 1)
                        return false;
                }
                return true;
            }
        }

        public PollAddViewModel(Poll poll, bool isNew) {
            Poll = poll;
            IsNew = isNew;
            _originalName = Poll.Name;  // Store the original name
            _originalType = Poll.Type;
            SelectedPollType = Poll.Type;
            Closed = Poll.Closed;
            if (!IsNew) {
                UsersToParticipate = new ObservableCollection<User>(poll.Participants);
                var choiceModels = Context.Choices
                            .OrderBy(c => c.Label)
                            .Where(c => c.Poll == poll)
                            .ToList();

                var choiceViewModels = new List<ChoiceViewModel>();
                foreach (var choiceModel in choiceModels) {
                    var choiceViewModel = new ChoiceViewModel(choiceModel, this);
                    choiceViewModels.Add(choiceViewModel);
                }
                ChoicesToAdd = new ObservableCollection<ChoiceViewModel>(choiceViewModels);

            } else {
                UsersToParticipate = new ObservableCollection<User>();
                ChoicesToAdd = new ObservableCollection<ChoiceViewModel>();
            }

            Save = new RelayCommand(SaveAction, CanSaveAction);
            Cancel = new RelayCommand(CancelAction, CanCancelAction);
            AddParticipant = new RelayCommand(AddParticipantAction, CanAddParticipant);
            AddAllParticipantsCommand = new RelayCommand(AddAllParticipantsAction);
            AddMyselfCommand = new RelayCommand(AddMyselfCommandAction);
            AddChoiceCommand = new RelayCommand(AddChoiceAction);
            AddRandomChoiceCommand = new RelayCommand(AddRandomChoice);

            RaisePropertyChanged();
        }
    }
}

