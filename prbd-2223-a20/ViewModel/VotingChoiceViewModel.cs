using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontAwesome6;
using System.Windows.Media;
using MyPoll.Model;
using PRBD_Framework;
using System.Windows.Input;
using System.Windows;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace MyPoll.ViewModel
{
    public class VotingChoiceViewModel : ViewModelBase<User, MyPollContext> {
        private readonly Poll CurrentPoll;

        public VotingChoiceViewModel(User participant, Choice choice, Poll currentPoll) {
            CurrentPoll = currentPoll;
            // check if the participant has voted for the specified choice
            IsVoted = participant.Votes.Any(v => v.Choice.Id == choice.Id);

            // if the participant has voted for the choice, retrieve the vote, else create a new vote
            // fetch the existing vote or create a new one if not found
            UserVote = Context.Votes.FirstOrDefault(v => v.Choice.Id == choice.Id && v.user.Id == participant.Id)
                ?? new Vote() { user = participant, Choice = choice, Type = VoteType.NoVote };

            //toggle the vote
            ChangeVote = new RelayCommand<VoteType>(voteType => {
                if (CurrentUser.Id == participant.Id || CurrentUser is Administrator) {
                    //vote type is not changing, there's nothing to do
                    if (UserVote.Type == voteType) {
                        return;
                    }

                    //new vote type is NoVote remove the vote
                    if (voteType == VoteType.NoVote) {
                        Context.Votes.Remove(UserVote);
                    }
                    //otherwise update or add the vote as necessary
                    else {
                        //poll type is Single, remove all existing votes by this user for the current poll
                        if (CurrentPoll.Type == PollType.Single) {
                            NotifyColleagues(App.Messages.MSG_VOTE_CHANGE, UserVote);
                            NotifyColleagues(App.Messages.MSG_EDITMODE, UserVote);
                            var existingVotes = Context.Votes.Where(v => v.user.Id == CurrentUser.Id && v.Choice.Poll.Id == CurrentPoll.Id).ToList();
                            if (voteType == VoteType.NoVote) {
                                return;
                            }
                            foreach (var vote in existingVotes) {
                                Context.Votes.Remove(vote);
                            }
                        }
                        //set the new vote type and add/update the vote
                        UserVote.Type = voteType;
                        if (Context.Entry(UserVote).State == EntityState.Detached) {
                            // attach the user and choice entities first to avoid tracking conflicts
                            Context.Users.Attach(UserVote.user);
                            Context.Choices.Attach(UserVote.Choice);

                            //add the vote
                            Context.Votes.Add(UserVote);
                        } else {
                            Context.Entry(UserVote).State = EntityState.Modified;
                        }
                    }
                    VoteMediator.Instance.RaiseVoteChanged();
                    // Save the changes
                    //IF CONTEXT.SAVECHANGES IS REMOVED => SINGLE VOTES WONT WORK ANYMORE
                    Context.SaveChanges();
                    RaisePropertyChanged(nameof(User.VoteCount));
                    // Update the vote type indicators
                    IsVotedNo = UserVote.Type == VoteType.No;
                    IsVotedYes = UserVote.Type == VoteType.Yes;
                    IsVotedMaybe = UserVote.Type == VoteType.Maybe;
                    UpdateVoteColor();
                }
            });

        }
        //Event subscribing try debut
        private void UpdateVoteColor() {
            VotedYesColor = IsVotedYes ? Brushes.Green : Brushes.Gray;
            Console.WriteLine("Vote color updated!");
        }

        private Brush _votedYesColor;
        public Brush VotedYesColor {
            get => _votedYesColor;
            set => SetProperty(ref _votedYesColor, value);
        }
        //Event subscribing try fin
        private Brush _hasVotedYesColor;
        public Brush HasVotedYesColor {
            get => _hasVotedYesColor;
            set => SetProperty(ref _hasVotedYesColor, value);
        }
        public Brush HasVotedNoColor => IsVotedNo ? Brushes.Red : Brushes.Gray;
        public Brush HasVotedMaybeColor => IsVotedMaybe ? Brushes.OrangeRed : Brushes.Gray;
        public Brush HasVotedNoVoteColor => IsVotedNoVote ? Brushes.Blue : Brushes.Gray;

        public Vote UserVote { get; private set; }
        public Participation Participation { get; private set; }
        public VotingChoiceViewModel() { }

        public ICommand ChangeVote { get; set; }

        private bool _isVotedNoVote;
        public bool IsVotedNoVote {
            get => UserVote.Type == VoteType.NoVote;
            set => SetProperty(ref _isVotedNoVote, value);
        }

        private bool _isVotedYes;
        public bool IsVotedYes {
            get => UserVote.Type == VoteType.Yes;
            set => SetProperty(ref _isVotedYes, value);
        }
        private bool _isVotedMaybe;
        public bool IsVotedMaybe {
            get => UserVote.Type == VoteType.Maybe;
            set => SetProperty(ref _isVotedMaybe, value);
        }
        private bool _isVotedNo;
        public bool IsVotedNo {
            get => UserVote.Type == VoteType.No;
            set => SetProperty(ref _isVotedNo, value);
        }

        private bool _isVoted;
        public bool IsVoted {
            get => _isVoted;
            set => SetProperty(ref _isVoted, value);
        }

        private bool _editMode;

        public bool EditMode {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }

        public EFontAwesomeIcon VotedIcon => UserVote.Type switch {
            VoteType.Yes => EFontAwesomeIcon.Solid_Check,
            VoteType.Maybe => EFontAwesomeIcon.Solid_CircleQuestion,
            VoteType.No => EFontAwesomeIcon.Solid_Xmark,
            _ => EFontAwesomeIcon.None,
        };

        public Brush VotedColor => UserVote.Type switch {
            VoteType.Yes => Brushes.Green,
            VoteType.Maybe => Brushes.Orange,
            VoteType.No => Brushes.Red,
            _ => Brushes.Transparent,
        };

        public string VotedToolTip => IsVoted ? UserVote.Type.ToString() : string.Empty;


    }
}
