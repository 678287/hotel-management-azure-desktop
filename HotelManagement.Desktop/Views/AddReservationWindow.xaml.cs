using System;
using System.Windows;

namespace HotelManagement.Desktop.Views
{
    public partial class AddReservationWindow : Window
    {
        public string GuestName => GuestNameBox.Text;
        public DateTime StartDate => StartDatePicker.SelectedDate ?? DateTime.Today;
        public DateTime EndDate => EndDatePicker.SelectedDate ?? DateTime.Today.AddDays(1);

        public AddReservationWindow()
        {
            InitializeComponent();
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GuestName))
            {
                MessageBox.Show("Please enter a guest name.");
                return;
            }

            if (EndDate <= StartDate)
            {
                MessageBox.Show("End date must be after start date.");
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
