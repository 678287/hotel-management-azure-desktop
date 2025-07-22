using System.Collections.Generic;

namespace HotelManagement.Shared.Models
{
    public class Room
    {
        public int Id { get; set; }
        public int Number { get; set; } // Actual room number
        public int Beds { get; set; }
        public string Quality { get; set; } // Ex: Standard, Premium, Suite
        public bool IsAvailable { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<RoomTask> RoomTasks { get; set; } = new List<RoomTask>();

        // Constructor to initialize collections
        public Room()
        {
            Reservations = new List<Reservation>();
            RoomTasks = new List<RoomTask>();
        }
    }
}
