using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MyPoll.Model;
using PRBD_Framework;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyPoll.ViewModel {
    public class ChoiceViewModel: ViewModelCommon {
        private Choice _choice;
        private bool _isInEditMode;

        public PollAddViewModel ParentViewModel { get; set; }

        public ChoiceViewModel(Choice choice, PollAddViewModel parentViewModel) {
            _choice = choice ?? new Choice();
            ParentViewModel = parentViewModel;
        }

        public int Id {
            get { return _choice.Id; }
        }

        public int PollId {
            get { return _choice.PollId; }
        }

        public string Label {
            get { return _choice.Label; }
            set {
                if (!ParentViewModel.ChoicesToAdd.Any(c => c.Label == value)) {
                    if (_choice.Label != value) {
                        _choice.Label = value;
                        RaisePropertyChanged(nameof(Label));
                    }
                    ClearErrors();
                } else {
                    AddError(nameof(Label), "This choice already exists.");
                }
            }
        }

        public int VoteCount {
            get { return _choice.VoteCount; }
        }

        public bool IsInEditMode {
            get { return _isInEditMode; }
            set {
                if (_isInEditMode != value) {
                    _isInEditMode = value;
                    RaisePropertyChanged(nameof(IsInEditMode));
                }
            }
        }

        public ObservableCollection<ChoiceViewModel> Choices { get; set; } = new ObservableCollection<ChoiceViewModel>();

        public ICommand StartEditCommand { get; set; }

        public ChoiceViewModel() {
            StartEditCommand = new RelayCommand<ChoiceViewModel>(StartEdit);
        }

        private void StartEdit(ChoiceViewModel choiceVM) {
            choiceVM.IsInEditMode = !choiceVM.IsInEditMode;
            ClearErrors();
        }

    }
}
