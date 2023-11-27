using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore.Query.Internal;
using MyPoll.ViewModel;
using MyPoll.Model;
using PRBD_Framework;

namespace MyPoll.View {
    /// <summary>
    /// Interaction logic for PollAddView.xaml
    /// </summary>
    public partial class PollAddView : UserControl {
        public PollAddView() {
            InitializeComponent();
        }

        private readonly PollAddViewModel _vm;

        public PollAddView(Poll poll, bool isNew) {
            InitializeComponent();
            _vm = new PollAddViewModel(poll, isNew);
            DataContext = _vm;
        }
        private void EditButton_Click(object sender, RoutedEventArgs e) {
            Button editButton = (Button)sender;
            var choiceViewModel = (ChoiceViewModel)editButton.DataContext;

            // Toggle the IsInEditMode property
            choiceViewModel.IsInEditMode = !choiceViewModel.IsInEditMode;

            // Update the visibility of the TextBlock and TextBox
            var choiceTextBlock = (TextBlock)editButton.FindName("ChoiceTextBlock");
            var choiceTextBox = (TextBox)editButton.FindName("ChoiceTextBox");

            if (choiceTextBlock != null && choiceTextBox != null) {
                choiceTextBlock.Visibility = choiceViewModel.IsInEditMode ? Visibility.Collapsed : Visibility.Visible;
                choiceTextBox.Visibility = choiceViewModel.IsInEditMode ? Visibility.Visible : Visibility.Collapsed;
            }
        }

    }

}
