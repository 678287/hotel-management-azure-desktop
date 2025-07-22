using HotelManagement.Shared.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HotelManagement.Desktop.Views
{
    public partial class AddTaskWindow : Window
    {
        public TaskType SelectedTaskType => (TaskType)Enum.Parse(typeof(TaskType), (TaskTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Cleaning");
        public string Notes => NotesBox.Text;

        public AddTaskWindow()
        {
            InitializeComponent();
            TaskTypeComboBox.SelectedIndex = 0;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Notes))
            {
                MessageBox.Show("Please enter some notes.");
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
