using MyPoll.Model;
using PRBD_Framework;

namespace MyPoll.ViewModel {
    public class CommentViewModel : ViewModelCommon {
        private Comment _comment;
        private User _currentUser;

        public CommentViewModel(Comment comment, User currentUser) {
            _comment = comment;
            _currentUser = currentUser;
        }

        public Comment Comment {
            get => _comment;
        }

        public bool CanDelete {
            get {
                return _currentUser != null &&
                ((_comment.poll != null && _currentUser.Id == _comment.poll.Creator.Id) ||
                _currentUser is Administrator);
            }
        }
    }

}
