using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using MyPoll.Model;
using PRBD_Framework;

namespace MyPoll.ViewModel
{
    public class VotingParticipantViewModel : ViewModelBase<User, MyPollContext> {


        public VotingParticipantViewModel(VotingGridViewModel votingGridViewModel, User participant, List<Choice> choices) {
            _votingGridViewModel = votingGridViewModel;
            _choices= choices;
            Participant = participant;
            RefreshVoting();
            //Event subscribing try
            VoteMediator.Instance.VoteChanged += OnVoteChanged;
            Console.WriteLine("Event subscribed!");
            //Event subscribing try
            EditCommand = new RelayCommand(() => EditMode = true);
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);
            Register<Vote> (App.Messages.MSG_VOTE_CHANGE, Vote => RefreshVoting());
            Register<Vote>(App.Messages.MSG_EDITMODE, Vote => EditModeChanged());
        }
        //Event subscribing try
        private void OnVoteChanged(object sender, EventArgs e) {
            // update the colors of VotingChoiceViewModel instances
            foreach (var choiceVM in ChoicesVM) {
                if (choiceVM != sender) {
                    // update the colors to grey for VotingChoiceViewModel
                    Console.WriteLine("COLOR GREYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY------------------------");
                    App.Current.Dispatcher.Invoke(() => {
                        choiceVM.HasVotedYesColor = Brushes.Gray;
                    });
                }
            }
        }
        //Event subscribing try
        private readonly VotingGridViewModel _votingGridViewModel;
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        private readonly List<Choice> _choices;
       
        public User Participant { get; }

        private bool _editMode;

        // La visbilité des boutons de sauvegarde et d'annulation sont bindés sur cette propriété
        public bool EditMode {
            get => _editMode;
            set => SetProperty(ref _editMode, value, EditModeChanged);
        }

        private void EditModeChanged() {
            // Lorsqu'on change le mode d'édition de la ligne, on le signale à chaque cellule
            foreach (VotingChoiceViewModel regVM in _choicesVM) {
                regVM.EditMode = EditMode;
            }

            // On informe le parent qu'on change le mode d'édition de la ligne
            _votingGridViewModel.AskEditMode(EditMode);
        }

        // Méthode appelée par le VM parent pour que la ligne mettre à jour la visibilité des boutons
        public void Changes() {
            RaisePropertyChanged(nameof(Editable));
        }

       
        // La ligne est éditable si elle n'est pas déjà en mode édition et si aucune autre ligne ne l'est
        // On récupére cette info via ParentEditMode
        // La visbilité des boutons d'édition et de suppression sont bindés sur cette propriété
        public bool Editable => !_votingGridViewModel.Poll.Closed && !EditMode && !ParentEditMode && (Participant.Id == CurrentUser.Id || CurrentUser is Administrator);


        public bool ParentEditMode => _votingGridViewModel.EditMode;

        private List<VotingChoiceViewModel> _choicesVM = new();
        public List<VotingChoiceViewModel> ChoicesVM {
            get => _choicesVM;
            private set => SetProperty(ref _choicesVM, value);
        }

        private void Save() {
            Context.SaveChanges();
            EditMode = false;
            RefreshVoting();
            NotifyColleagues(ApplicationBaseMessages.MSG_REFRESH_DATA);
        }

        private void RefreshVoting() {
            // On crée, pour chaque participant, un VotingParticipantViewModel
            // qui sera utilisé par le VotingParticipantViewModel
            // ChoicesVM est la liste qui servira de source
            ChoicesVM = _choices
                .Select(c => new VotingChoiceViewModel(Participant, c, c.Poll))
                .ToList();
        }

        private void Cancel() {
            foreach (var entry in Context.ChangeTracker.Entries()) {
                if (entry.Entity != null) {
                    switch (entry.State) {
                        case EntityState.Modified:
                            // Reload the entity from the database overwriting any property values with values from the database.
                            entry.Reload();
                            break;
                        case EntityState.Added:
                            // Simply remove the entity from the context in the Added state. It will not be saved when SaveChanges is called.
                            Context.Entry(entry.Entity).State = EntityState.Detached;
                            break;
                        case EntityState.Deleted:
                            // Set the entity back to Unchanged. When SaveChanges is called, no operation will be performed in the database.
                            entry.State = EntityState.Unchanged;
                            break;
                    }
                }
            }
            EditMode = false;
            Dispose();
            RefreshVoting();
        }

        private void Delete() {
            // remove votes from the current poll
            var currentPollChoiceIds = _choices.Select(c => c.Id).ToList();
            var votesToRemove = Participant.Votes.Where(v => currentPollChoiceIds.Contains(v.Choice.Id)).ToList();
            foreach (var vote in votesToRemove) {
                Participant.Votes.Remove(vote);
            }

            // save the changes and refresh the voting choices
            Context.SaveChanges();
            NotifyColleagues(ApplicationBaseMessages.MSG_REFRESH_DATA);
            RefreshVoting();
        }

    }
}
