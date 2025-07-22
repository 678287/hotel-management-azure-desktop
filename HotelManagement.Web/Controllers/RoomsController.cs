using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Shared.Models;
using HotelManagement.Shared.Data;

namespace HotelManagement.Web.Controllers
{
    public class RoomsController : Controller
    {
        private readonly HotelDbContext _context;

        public RoomsController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: Rooms
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rooms.ToListAsync());
        }

        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Rooms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Number,Beds,Quality,IsAvailable")] Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Rooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,Beds,Quality,IsAvailable")] Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Rooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
        // GET: Rooms/Search
        public IActionResult Search()
        {
            return View();
        }

        // POST: Rooms/Search
        [HttpPost]
        public async Task<IActionResult> Search(int beds, string quality, DateTime startDate, DateTime endDate)
        {
            var availableRooms = await _context.Rooms
                .Where(r => r.Beds == beds && r.Quality == quality)
                .Where(r => !_context.Reservations.Any(res =>
                    res.RoomId == r.Id &&
                    ((startDate >= res.StartDate && startDate <= res.EndDate) ||
                     (endDate >= res.StartDate && endDate <= res.EndDate) ||
                     (startDate <= res.StartDate && endDate >= res.EndDate))))
                .ToListAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View("SearchResults", availableRooms);
        }

        public async Task<IActionResult> SearchResults(DateTime start, DateTime end, int? beds, string? quality)
        {
            var availableRooms = await _context.Rooms
                .Where(r =>
                    (r.IsAvailable || // Either marked as available
                     !_context.Reservations.Any(res => // Or has no conflicting reservations
                         res.RoomId == r.Id &&
                         start < res.EndDate &&
                         end > res.StartDate)) &&
                    (!beds.HasValue || r.Beds == beds) &&
                    (string.IsNullOrEmpty(quality) || r.Quality == quality))
                .ToListAsync();

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            return View(availableRooms);
        }


    }
}
