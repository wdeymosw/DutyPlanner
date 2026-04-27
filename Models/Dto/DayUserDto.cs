using DutyPlanner.Presentation.ViewModels;

namespace DutyPlanner.Models
{
    public sealed class DayUserDto
    {
        public Guid InstanceId { get; set; }
        public Guid UserID { get; set; }
        public string? Name { get; set; }
        public int Hours { get; set; }
        public DayUserPlacement Placement { get; init; }


        public DayUserDto()
        {

        }


        public DayUserDto(UserViewModel user)
        {
            InstanceId = Guid.NewGuid();
            UserID = user.Id;
            Name = user.Name;
            Hours = user.Hours;
            Placement = DayUserPlacement.Active;

        }
    }

    public enum DayUserPlacement
    {
        Active,
        Reserve
    }

}
