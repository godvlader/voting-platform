using MyPoll.Model;
using System.Collections.ObjectModel;
using PRBD_Framework;
using System.Windows.Input;
using MyPoll.View;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace MyPoll.ViewModel;

public class PollsViewModel : ViewModelCommon {
    private ObservableCollection<PollCardViewModel> _polls = new ObservableCollection<PollCardViewModel>();

    public ICommand DisplayPollDetails { get; set; }
    public ObservableCollection<PollCardViewModel> Polls {
        get => _polls;
        set => SetProperty(ref _polls, value);
    }
    private Poll _poll;
    public Poll Poll {
        get => _poll;
        set => SetProperty(ref _poll, value);
    }
    public PollsViewModel(): base() {
        LoadPollCards();
        OnRefreshData();

        ApplyFilter = new RelayCommand(ApplyFilterAction);

        ClearFilter = new RelayCommand(() => Filter = "");

        DisplayPollDetails = new RelayCommand<PollCardViewModel>(vm => {

            NotifyColleagues(App.Messages.MSG_DISPLAY_POLL, vm.Poll);
        });

        NewPoll = new RelayCommand(() => {
            NotifyColleagues(App.Messages.MSG_NEW_POLL, new Poll());
        });


    }

    protected override void OnRefreshData() {
        LoadPollCards();
        
    }

    private void LoadPollCards() {
        //ternary operator to choose the appropriate method for getting polls
        IQueryable<Poll> polls = IsAdmin ? Poll.GetAdminPolls() : Poll.GetPolls(CurrentUser.Id);

        //clear the Polls before adding new items
        Polls.Clear();

        foreach (var poll in polls) {
            var pollCardViewModel = new PollCardViewModel(poll);

            Polls.Add(pollCardViewModel);
    
        }
    }


    private string _filter;
    public string Filter {
        get => _filter;
        set => SetProperty(ref _filter, value, ApplyFilterAction);
    }

    public ICommand ApplyFilter { get; set; }

    private void ApplyFilterAction() {
        Console.WriteLine("Search clicked! " + Filter);
        Console.WriteLine(CurrentUser.FullName);
        Polls = new ObservableCollection<PollCardViewModel>(
                    IsAdmin
                    ? Context.Polls
                        .Where(poll =>
                            poll.Name.Contains(Filter) ||
                            poll.Participants.Any(participant => participant.FullName.Contains(Filter)) ||
                            poll.Choices.Any(choice => choice.Label.Contains(Filter)))
                        .Select(poll => new PollCardViewModel(poll))
                    : Context.Polls
                        .Include(p => p.Participants)
                        .Include(p => p.Choices)
                        .Where(p => (p.CreatorId == CurrentUser.Id || p.Participants.Any(participant => participant.Id == CurrentUser.Id))
                            && (p.Name.Contains(Filter)
                                || p.Participants.Any(participant => participant.FullName.Contains(Filter))
                                || p.Choices.Any(choice => choice.Label.Contains(Filter)))
                        )
                        .OrderBy(p => p.Name)
                        .Select(poll => new PollCardViewModel(poll))
                );

        Console.WriteLine($"{Polls.Count} polls found");

    }


    public ICommand ClearFilter { get; set; }

    public ICommand NewPoll { get; set; }

}

