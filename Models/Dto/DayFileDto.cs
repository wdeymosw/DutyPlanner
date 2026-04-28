namespace DutyPlanner.Models
{
    public class DayFileDto
    {
        public string Comment { get; set; } = "";
        public List<DayUserDto> Users { get; set; } = [];
    }
}
