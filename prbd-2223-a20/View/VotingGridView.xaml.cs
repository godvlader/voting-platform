
using System.Windows.Controls;
using MyPoll.Model;
using MyPoll.ViewModel;
using PRBD_Framework;

namespace MyPoll.View {
    /// <summary>
    /// Interaction logic for VotingGridView.xaml
    /// </summary>
    public partial class VotingGridView : UserControlBase {

        public VotingGridView() {
            InitializeComponent();
            /*VotingGridViewModel viewModel = new VotingGridViewModel();
            DataContext = viewModel;*/
        }

        //public User Participant { get; internal set; }
    }

}
