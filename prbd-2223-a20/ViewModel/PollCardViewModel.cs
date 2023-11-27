using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyPoll.Model;
using PRBD_Framework;

namespace MyPoll.ViewModel
{
    public class PollCardViewModel : ViewModelCommon {
        private Poll _poll;
        public Poll Poll {
            get => _poll;
            set => SetProperty(ref _poll, value);
        }

        public PollCardViewModel(Poll poll) {
            Poll = poll;
        }

        public string BestChoices { get=>Poll.BestChoice; }

        public string CardColor {
            get {
                if (Poll.Closed) {
                    return "#FFE6DC";
                }
                // Check if there are no votes from current user in this Poll.
                if (!Poll.Choices.SelectMany(c => c.Votes).Any(v => v.UserId == App.CurrentUser.Id)) {
                    return "#D3D3D3";
                }
                if (Poll.Participants.Any(u => u.Id == App.CurrentUser.Id)) {
                    return "#C4E0C4";
                }
                // Return a default color if none of the above conditions are met
                return "#default_color";
            }
        }


    }
}
