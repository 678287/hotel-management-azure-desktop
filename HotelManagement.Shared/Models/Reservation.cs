using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Shared.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string GuestName { get; set; }  
        public string UserId { get; set; }     
        public IdentityUser User { get; set; } 

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }
    }
}
