using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MyPoll.Model;
using MyPoll.View;
using PRBD_Framework;
using static MyPoll.App;

namespace MyPoll.ViewModel {
    public class PollDetailViewModel : ViewModelCommon {

        private Poll _poll;

        public Poll Poll {
            get => _poll;
            set => SetProperty(ref _poll, value);
        }
        public PollAddViewModel PollAddViewModel { get; } = new();
        public VotingGridView VotingGridView { get; } = new();

        public VotingGridViewModel votingGridViewModel { get; } = new();

        private UserControl _editView;

        public UserControl EditView {
            get => _editView;
            set => SetProperty(ref _editView, value);
        }


        private string _name;
        public string Name {
            get => Poll?.Name;
            set {
                _name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public string Creator {
            get => Poll?.Creator.ToString();
        }

        public bool IsClosedAndOwner {
            get { return Poll.Closed && Poll.Creator.Id == CurrentUser.Id || CurrentUser is Administrator; }
        }

        public bool IsParticipantAndOpenPoll {
            get { return Poll.Participants.Any(user => user.Id == CurrentUser.Id) && !Poll.Closed; }
        }


        //EDIT MODE //
        private bool _isEditing;
        public bool IsEditing {
            get { return _isEditing; }
            set {
                _isEditing = value;
                RaisePropertyChanged(nameof(IsEditing));
            }
        }

        public ICommand EditCommand { get; set; }

        public PollDetailViewModel(Poll poll, bool isNew) : base() {

            Poll = poll;
            IsPollClosed = poll.Closed;
            //pass the poll to the VotingGridViewModel
            votingGridViewModel = new VotingGridViewModel(Poll);
            _comments = new ObservableCollection<CommentViewModel>(
                        GetComments(Poll)
                        .OrderByDescending(c => c.Timestamp) // descending order based on timestamp 
                        .Select(c => new CommentViewModel(c, CurrentUser))
                    );

            AddCommentCommand = new RelayCommand(ToggleTextBoxVisibility);
            PostCommentCommand = new RelayCommand(OnPostComment);
            DeleteCommentCommand = new RelayCommand<CommentViewModel>(DeleteCommentAction);

            EditCommand = new RelayCommand(() => {
                EditView = new PollAddView(poll, isNew);
                IsEditing = true;
            });

            DeleteCommand = new RelayCommand<int>(DeleteAction);

            ReopenCommand = new RelayCommand<Poll>(ReopenCommandAction);

        }

        //REOPEN POLL
        private bool _isPollClosed;
        public bool IsPollClosed {
            get { return _isPollClosed; }
            set {
                if (value != _isPollClosed) {
                    _isPollClosed = value;
                    RaisePropertyChanged(nameof(IsPollClosed));
                    // Trigger property change for Editable
                    RaisePropertyChanged(nameof(VotingParticipantViewModel.Editable));
                }
            }
        }

        public ICommand ReopenCommand { get; set; }

        private void ReopenCommandAction(Poll poll) {
            //notifier les autres vue notify
            Console.WriteLine("open");
            poll.Closed = false;
            IsPollClosed= false;
            Context.SaveChanges();
            NotifyColleagues(ApplicationBaseMessages.MSG_REFRESH_DATA);
            NotifyColleagues(App.Messages.MSG_REFRESH_DATA, Poll);
        }

        //DELETE COMMENT
        public ICommand DeleteCommentCommand { get; set; }

        private void DeleteCommentAction(CommentViewModel commentViewModel) {
            if (commentViewModel.CanDelete) {
                Comment commentToDelete = Context.Comments.SingleOrDefault(c => c.Id == commentViewModel.Comment.Id);
                if (commentToDelete != null) {
                    Context.Comments.Remove(commentToDelete);
                    Comments.Remove(commentViewModel);
                    Context.SaveChanges();
                }
            } else {
                Console.WriteLine("No, you can't delete this comment.");
            }
        }

        //POST COMMENT
        public ICommand PostCommentCommand { get; }

        private string _commentTextBox;
        public string CommentTextBox {
            get => _commentTextBox;
            set {
                if (SetProperty(ref _commentTextBox, value)) {
                    RaisePropertyChanged(nameof(CommentTextBox)); //bind with the xaml 
                }
            }
        }

        private void OnPostComment() {
            if (CanPostComment() && IsParticipantAndOpenPoll || CurrentUser is Administrator) {
                Comment newComment = new() {
                    UserId = CurrentUser.Id,
                    PollId = Poll.Id,
                    Text = CommentTextBox,
                    Timestamp = DateTime.Now
                };
                CommentTextBox = "";
                Comments.Add(new CommentViewModel(newComment, CurrentUser)); //Add to the ObservableCollection
                Context.Add(newComment); //add in the db
                Context.SaveChanges();  //save in the db
                Comments = new ObservableCollection<CommentViewModel>(GetComments(Poll).Select(c => new CommentViewModel(c, CurrentUser)));
                IsTextBoxVisible = false;
            }
            NotifyColleagues(App.Messages.MSG_REFRESH_DATA,Poll);
        }


        private bool CanPostComment() {
            // The PostCommentCommand is enabled only when the CommentTextBox has some text
            return !string.IsNullOrWhiteSpace(CommentTextBox);
        }

        //SPAWN TEXTBOX FOR ADD COMMENT
        private bool _isTextBoxVisible;
        public bool IsTextBoxVisible {
            get => _isTextBoxVisible;
            set => SetProperty(ref _isTextBoxVisible, value);
        }

        private void ToggleTextBoxVisibility() {
            IsTextBoxVisible = !IsTextBoxVisible;
        }

        public ICommand AddCommentCommand { get; }

        //GET COMMENTS BY POLL
        private ObservableCollection<CommentViewModel> _comments;
        public ObservableCollection<CommentViewModel> Comments {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        private IEnumerable<Comment> GetComments(Poll poll) {
            //method should return the comments associated with the current poll.
            return Context.Comments.Where(c => c.PollId == poll.Id).ToList();
        }


        //GET PARTICIPANTS BY POLL
        private ObservableCollection<User> _participants;
        public ObservableCollection<User> Participants {
            get {
                _participants = new ObservableCollection<User>(GetParticipants(Poll));
                return _participants;
            }
        }

        private static IEnumerable<User> GetParticipants(Poll poll) => Poll.GetUsersByPoll(poll);
        
        //GET CHOICES BY POLL
        private ObservableCollection<Choice> _choices;

        public ObservableCollection<Choice> Choices {
            get {
                _choices = new ObservableCollection<Choice>(GetChoices(Poll));

                foreach (var choice in _choices) {
                    System.Diagnostics.Debug.WriteLine("Label: " + choice.Label);

                }
                return _choices;
            }
        }
        private static IEnumerable<Choice> GetChoices(Poll poll) => Poll.GetChoicesByPoll(poll);

        public bool IsPollCreatorOrAdmin {
            get { return Poll.Creator.Id == CurrentUser.Id || CurrentUser is Administrator; }
        }


        //DELETE COMMAND
        public ICommand DeleteCommand { get; }

        private void DeleteAction(int pollId) {
            if (App.Confirm("Are you sure you want to delete this poll?")) {
                // Retrieve the poll from the database
                Poll pollToDelete = Context.Polls.Include(p => p.Comments)
                                                 .Include(p => p.Choices)
                                                 .Include(p => p.Participants)
                                                 .FirstOrDefault(p => p.Id == pollId);

                if (pollToDelete == null) {
                    return;
                }

                // Remove all comments
                Context.Comments.RemoveRange(pollToDelete.Comments);

                // Remove all choices
                Context.Choices.RemoveRange(pollToDelete.Choices);

                // Remove relationships between the poll and participants (if any)
                pollToDelete.Participants.Clear();

                // Remove the poll
                Context.Polls.Remove(pollToDelete);

                Context.SaveChanges();
                NotifyColleagues(App.Messages.MSG_DELETE_POLL, pollToDelete);
                NotifyColleagues(ApplicationBaseMessages.MSG_REFRESH_DATA);
                Console.WriteLine("POLL WITH ID" + pollId + " HAS BEEN DELETED");
            }
        }

        public override void Dispose() {
            votingGridViewModel.Dispose();
            base.Dispose();
        }

    }
}
