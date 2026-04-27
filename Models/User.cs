namespace DutyPlanner.Models
{
    public class User
    {
        public Guid Id { get; }
        public string Name { get; }

        public int Hours { get; }


        public User(Guid id, string name, int hours)
        {
            Id = id;
            Name = name;
            Hours = hours;
        }
    }
}
