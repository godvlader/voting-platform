using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyPoll.Model;
using MyPoll.View;
using PRBD_Framework;

namespace MyPoll.ViewModel;
public class VotingGridViewModel : ViewModelBase<User, MyPollContext> {

    public VotingGridViewModel(Poll poll) {
        _poll = poll;

        // Fetch the choices for the current poll
        _choices = Context.Choices.OrderBy(c => c.Label).Where(c => c.Poll == poll).ToList();
        //_choices = _poll.Choices.OrderBy(c => c.Label).ToList(); MARCHE PAS

        // Fetch the participants for the current poll
        _participants = _poll.Participants
            .OrderBy(p => p.FullName)
            .Select(p => new VotingParticipantViewModel(this, p, _choices))
            .ToList();

        // Debugging information
        Console.WriteLine($"Loaded {_participants.Count} participants");
        foreach (var participant in _participants) {
            // Replace 'PropertyName' with the actual property names of the VotingParticipantViewModel class you want to display
            Console.WriteLine($"ID: {participant}, Name: {participant.Participant.FullName}");
        }
        Console.WriteLine($"Loaded {_choices.Count} choices");
        Console.WriteLine("Choices:");
        foreach (var choice in _choices) {
            // Replace 'PropertyName' with the actual property names of the Choices class you want to display
            Console.WriteLine($"ID: {choice.Id}, Text: {choice.Label}");
        }
    }


    public VotingGridViewModel() {

    }

    private List<Choice> _choices;
    public IReadOnlyList<Choice> Choices => _choices;

    private List<VotingParticipantViewModel> _participants;
    public List<VotingParticipantViewModel> Participants => _participants;


    private User _participant;
    public User Participant {
        get => _participant;
        set => SetProperty(ref _participant, value, OnRefreshData);
    }

    private Poll _poll;
    public Poll Poll {
        get => _poll;
        set {
            SetProperty(ref _poll, value);
        }
    }

    private bool _editMode;
    public bool EditMode {
        get => _editMode;
        set => SetProperty(ref _editMode, value);
    }
    public void AskEditMode(bool editMode) {
        EditMode = editMode;
        // Change la visibilité des boutons de chacune des lignes
        // (voir la logique dans RegistrationStudentViewModel)
        foreach (var s in Participants)
            s.Changes();
    }

}


