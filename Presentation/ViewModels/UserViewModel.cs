using DutyPlanner.Presentation.Commands;
using DutyPlanner.Presentation.ViewModels.Base;
using System.Diagnostics;

namespace DutyPlanner.Presentation.ViewModels
{
    public class UserViewModel : ViewModel
    {
        public Guid InstanceId { get; }
        public Guid Id { get; }   // мастер-Id

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                Set(ref _isEditing, value);
                BeginEditCommand?.RaiseCanExecuteChanged();
                EndEditCommand?.RaiseCanExecuteChanged();
            }
        }

        private int _hours;
        public int Hours
        {
            get => _hours;
            set => Set(ref _hours, value);
        }

        public LambdaCommand? BeginEditCommand { get; private set; }
        public LambdaCommand? EndEditCommand { get; private set; }
        public LambdaCommand? RemoveCommand { get; private set; }
        public LambdaCommand? ShowReportCommand { get; private set; }


        public UserViewModel(
        Guid id,
        string name,
        int hours,
        Action<UserViewModel> beginEdit,
        Action<UserViewModel> endEdit,
        Action<UserViewModel> remove,
        Action<UserViewModel> showReport)
        {
            Id = id;
            InstanceId = Guid.NewGuid();
            _name = name;
            _hours = hours;

            BeginEditCommand = new LambdaCommand(
                () => beginEdit(this),
                () => !IsEditing
            );

            EndEditCommand = new LambdaCommand(
                () => endEdit(this),
                () => IsEditing
            );

            RemoveCommand = new LambdaCommand(
                () => remove(this)
            );

            ShowReportCommand = new LambdaCommand(
                () => showReport(this)
            );
        }





    }
}
