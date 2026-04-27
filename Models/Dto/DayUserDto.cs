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


        public DayUserDto(Guid userId, string? name, int hours, DayUserPlacement placement = DayUserPlacement.Active)
        {
            InstanceId = Guid.NewGuid();
            UserID = userId;
            Name = name;
            Hours = hours;
            Placement = placement;
        }
    }

    public enum DayUserPlacement
    {
        Active,
        Reserve
    }

}
