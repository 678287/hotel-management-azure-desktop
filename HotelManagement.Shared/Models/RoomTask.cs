namespace HotelManagement.Shared.Models
{
    public enum TaskType
    {
        Cleaning,
        Maintenance,
        RoomService
    }

    public enum TaskStatus
    {
        New,
        InProgress,
        Finished
    }

    public class RoomTask
    {
        public int Id { get; set; }
        public TaskType Type { get; set; }
        public TaskStatus Status { get; set; }
        public string Notes { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }
    }
}
