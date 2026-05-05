using DutyPlanner.Models;

namespace DutyPlanner.Presentation.ViewModels
{
    public class DayUserViewModel : ViewModel
    {
        public Guid InstanceId { get; }
        public Guid UserID { get; }
        public string? Name { get; }


        private DayUserPlacement _placement;
        public DayUserPlacement Placement
        {
            get => _placement;
            set => Set(ref _placement, value);
        }

        #region Hours
        private int _hours;
        public int Hours
        {
            get => _hours;
            set => Set(ref _hours, Math.Clamp(value, 1, 24));
        }
        #endregion


        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => Set(ref _isEditing, value);
        }

        public DayUserViewModel(DayUserDto dto)
        {
            InstanceId = dto.InstanceId;
            UserID = dto.UserID;
            Name = dto.Name;
            Hours = dto.Hours;
        }

        public DayUserDto ToDto() => new()
        {
            InstanceId = InstanceId,
            UserID = UserID,
            Name = Name,
            Hours = Hours,
            Placement = Placement
        };



    }
}
