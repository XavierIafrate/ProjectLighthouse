using ProjectLighthouse.Model.Programs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayNcProgramCommit : UserControl
    {
        public NcProgramCommit Commit
        {
            get { return (NcProgramCommit)GetValue(CommitProperty); }
            set { SetValue(CommitProperty, value); }
        }

        public static readonly DependencyProperty CommitProperty =
            DependencyProperty.Register("Commit", typeof(NcProgramCommit), typeof(DisplayNcProgramCommit), new PropertyMetadata(null, OnCommitChanged));

        private static void OnCommitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayNcProgramCommit control) return;
            control.DataContext = control.Commit;
        }

        public ICommand OpenCommit
        {
            get { return (ICommand)GetValue(OpenCommitProperty); }
            set { SetValue(OpenCommitProperty, value); }
        }

        public static readonly DependencyProperty OpenCommitProperty =
            DependencyProperty.Register("OpenCommit", typeof(ICommand), typeof(DisplayNcProgramCommit), new PropertyMetadata(null, SetCommand));

        private static void SetCommand(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayNcProgramCommit control) return;

            control.ViewCommitButton.Command = e.NewValue as ICommand;
        }

        public DisplayNcProgramCommit()
        {
            InitializeComponent();
        }
    }
}
