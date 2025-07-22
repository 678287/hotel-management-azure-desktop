using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Shared.Models;
using HotelManagement.Shared.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagement.Web.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReservationsController(HotelDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Book a room (POST)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int roomId, DateTime startDate, DateTime endDate)
        {
            // Get the room first
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                return RedirectToAction("Search", "Rooms");
            }

            // Check if room is actually available for these dates
            bool isAvailable = await IsRoomAvailable(roomId, startDate, endDate);
            if (!isAvailable)
            {
                return RedirectToAction("Search", "Rooms");
            }

            // Create reservation
            var user = await _userManager.GetUserAsync(User);
            var reservation = new Reservation
            {
                RoomId = roomId,
                GuestName = user.UserName,
                UserId = user.Id,
                StartDate = startDate,
                EndDate = endDate
            };

            // Update room availability
            room.IsAvailable = false;

            _context.Reservations.Add(reservation);
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyReservations");
        }

        private async Task<bool> IsRoomAvailable(int roomId, DateTime startDate, DateTime endDate)
        {
            return !await _context.Reservations
                .AnyAsync(r => r.RoomId == roomId &&
                              startDate < r.EndDate &&
                              endDate > r.StartDate);
        }

        // Show user's reservations
        public async Task<IActionResult> MyReservations()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            return View(reservations);
        }
    }
}
