using HotelManagement.Desktop.Views;
using HotelManagement.Shared.Data;
using HotelManagement.Shared.Models;
using HotelManagement.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;

namespace HotelManagement.Desktop
{
    public partial class MainWindow : Window
    {
        private List<Room> _rooms;

        // Delegate + Event when a room is selected
        public delegate void RoomSelectedEventHandler(Room room);
        public event RoomSelectedEventHandler RoomSelected;

        private readonly LoginService _loginService;

        public MainWindow(LoginService loginService)
        {
            InitializeComponent();
            _loginService = loginService;

            LoadRooms();
            RoomSelected += OnRoomSelected;
        }

        private void LoadRooms()
        {
            try
            {
                using var context = DbContextFactory.Create();
                _rooms = context.Rooms
                                .Include(r => r.Reservations)
                                .Include(r => r.RoomTasks)
                                .ToList();

                RoomsList.ItemsSource = _rooms;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load rooms: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RoomsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RoomsList.SelectedItem is Room selectedRoom)
            {
                // Trigger delegate event
                RoomSelected?.Invoke(selectedRoom);
            }
        }

        private void OnRoomSelected(Room room)
        {
            RoomNumberText.Text = $"Room {room.Number}";
            RoomQualityText.Text = $"Quality: {room.Quality}";

            // Updated availability check based on current date
            bool hasCurrentReservation = room.Reservations.Any(r =>
                r.StartDate <= DateTime.Today && r.EndDate >= DateTime.Today);
            RoomAvailabilityText.Text = hasCurrentReservation ? "Occupied" : "Available";

            ReservationsList.ItemsSource = room.Reservations
                .Select(r => $"{r.GuestName}: {r.StartDate.ToShortDateString()} - {r.EndDate.ToShortDateString()}");

            TasksList.ItemsSource = room.RoomTasks
              .Select(t => $"{t.Type} - {t.Status} - {t.Notes}");
        }

        private void AddReservation_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsList.SelectedItem is not Room selectedRoom)
            {
                MessageBox.Show("Please select a room first.");
                return;
            }

            var window = new AddReservationWindow();
            if (window.ShowDialog() == true)
            {
                try
                {
                    using var context = DbContextFactory.Create();

                    // Reload room with tracking
                    var roomToUpdate = context.Rooms.FirstOrDefault(r => r.Id == selectedRoom.Id);
                    if (roomToUpdate == null)
                    {
                        MessageBox.Show("Room no longer exists.");
                        return;
                    }

                    var newReservation = new Reservation
                    {
                        GuestName = window.GuestName,
                        StartDate = window.StartDate,
                        EndDate = window.EndDate,
                        RoomId = roomToUpdate.Id,
                        UserId = _loginService.LoggedInUser?.Id // must not be null
                    };

                    context.Reservations.Add(newReservation);

                    // Only mark room as unavailable if reservation starts today or earlier
                    if (window.StartDate <= DateTime.Today && window.EndDate >= DateTime.Today)
                    {
                        roomToUpdate.IsAvailable = false;
                    }

                    context.SaveChanges();

                    MessageBox.Show("Reservation added successfully.");

                    LoadRooms();
                    RoomsList.SelectedItem = _rooms.FirstOrDefault(r => r.Id == selectedRoom.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add reservation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsList.SelectedItem is not Room selectedRoom)
            {
                MessageBox.Show("Please select a room.");
                return;
            }

            var taskWindow = new AddTaskWindow();
            if (taskWindow.ShowDialog() == true)
            {
                var newTask = new RoomTask
                {
                    RoomId = selectedRoom.Id,
                    Type = taskWindow.SelectedTaskType,
                    Status = HotelManagement.Shared.Models.TaskStatus.New,
                    Notes = taskWindow.Notes
                };

                try
                {
                    using var context = DbContextFactory.Create();
                    context.RoomTasks.Add(newTask);
                    context.SaveChanges();

                    MessageBox.Show("Task added.");
                    LoadRooms();
                    RoomsList.SelectedItem = _rooms.FirstOrDefault(r => r.Id == selectedRoom.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        private void DeleteReservation_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsList.SelectedItem is not Room selectedRoom)
            {
                MessageBox.Show("Please select a room first.");
                return;
            }

            if (ReservationsList.SelectedItem == null)
            {
                MessageBox.Show("Please select a reservation to delete.");
                return;
            }

            var selectedString = ReservationsList.SelectedItem.ToString();
            var parts = selectedString.Split(':');
            if (parts.Length < 1)
            {
                MessageBox.Show("Unable to parse reservation.");
                return;
            }

            string guestName = parts[0].Trim();

            try
            {
                using var context = DbContextFactory.Create();
                var reservation = context.Reservations
                                         .FirstOrDefault(r => r.GuestName == guestName && r.RoomId == selectedRoom.Id);

                if (reservation != null)
                {
                    context.Reservations.Remove(reservation);

                    // Check if there are any current reservations left
                    var hasCurrentReservations = context.Reservations
                        .Any(r => r.RoomId == selectedRoom.Id &&
                               r.StartDate <= DateTime.Today &&
                               r.EndDate >= DateTime.Today);

                    var room = context.Rooms.First(r => r.Id == selectedRoom.Id);
                    room.IsAvailable = !hasCurrentReservations;

                    context.SaveChanges();
                    MessageBox.Show("Reservation deleted.");
                    LoadRooms(); // Refresh UI
                }
                else
                {
                    MessageBox.Show("Reservation not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting reservation: {ex.Message}");
            }
        }

        private void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsList.SelectedItem is not Room selectedRoom)
            {
                MessageBox.Show("Please select a room.");
                return;
            }

            if (ReservationsList.SelectedItem == null)
            {
                MessageBox.Show("Please select a reservation to check in.");
                return;
            }

            try
            {
                using var context = DbContextFactory.Create();
                var room = context.Rooms.First(r => r.Id == selectedRoom.Id);
                room.IsAvailable = false;
                context.SaveChanges();

                MessageBox.Show("Guest checked in. Room marked as occupied.");
                LoadRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Check-in failed: {ex.Message}");
            }
        }

        private void CheckOut_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsList.SelectedItem is not Room selectedRoom)
            {
                MessageBox.Show("Please select a room.");
                return;
            }

            if (ReservationsList.SelectedItem == null)
            {
                MessageBox.Show("Please select a reservation to check out.");
                return;
            }

            var selectedString = ReservationsList.SelectedItem.ToString();
            var guestName = selectedString.Split(':')[0].Trim();

            try
            {
                using var context = DbContextFactory.Create();
                var reservation = context.Reservations
                                         .FirstOrDefault(r => r.GuestName == guestName && r.RoomId == selectedRoom.Id);

                if (reservation != null)
                {
                    context.Reservations.Remove(reservation);

                    // Check if there are any other current reservations
                    var hasCurrentReservations = context.Reservations
                        .Any(r => r.RoomId == selectedRoom.Id &&
                               r.StartDate <= DateTime.Today &&
                               r.EndDate >= DateTime.Today);

                    var room = context.Rooms.First(r => r.Id == selectedRoom.Id);
                    room.IsAvailable = !hasCurrentReservations;

                    context.SaveChanges();
                    MessageBox.Show("Guest checked out and reservation removed.");
                    LoadRooms();
                }
                else
                {
                    MessageBox.Show("Reservation not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Check-out failed: {ex.Message}");
            }
        }
    }
}