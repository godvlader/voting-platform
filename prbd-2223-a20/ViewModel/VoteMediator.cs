using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRBD_Framework;
namespace MyPoll.ViewModel
{
    public class VoteMediator {
        public event EventHandler VoteChanged;

        public static VoteMediator Instance { get; } = new VoteMediator();

        private VoteMediator() { }

        public void RaiseVoteChanged() {
            VoteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

}
