using AppointmentSystem.WPF.Commands;
using AppointmentSystem.WPF.Models;
using AppointmentSystem.WPF.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AppointmentSystem.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IAppointmentApiService _apiService;

        public MainViewModel(IAppointmentApiService apiService)
        {
            _apiService = apiService;

            // Initialize commands
            LoadAppointmentsCommand = new RelayCommand(async _ => await LoadAppointmentsAsync());
            AddAppointmentCommand = new RelayCommand(_ => PrepareNewAppointment());
            SaveAppointmentCommand = new RelayCommand(async _ => await SaveAppointmentAsync(),
                _ => CanSaveAppointment());
            EditAppointmentCommand = new RelayCommand(EditAppointment,
                _ => SelectedAppointment != null);
            DeleteAppointmentCommand = new RelayCommand(async _ => await DeleteAppointmentAsync(),
                _ => SelectedAppointment != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());

            // Load initial data
            Task.Run(async () => await LoadAppointmentsAsync());
        }

        // Properties
        private ObservableCollection<AppointmentModel> _appointments;
        public ObservableCollection<AppointmentModel> Appointments
        {
            get => _appointments;
            set => SetProperty(ref _appointments, value);
        }

        private AppointmentModel _selectedAppointment;
        public AppointmentModel SelectedAppointment
        {
            get => _selectedAppointment;
            set => SetProperty(ref _selectedAppointment, value);
        }

        private string _patientName;
        public string PatientName
        {
            get => _patientName;
            set => SetProperty(ref _patientName, value);
        }

        private DateTime _appointmentDate = DateTime.Now;
        public DateTime AppointmentDate
        {
            get => _appointmentDate;
            set => SetProperty(ref _appointmentDate, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private string _status = "Scheduled";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        private int _editingId;

        // Commands
        public ICommand LoadAppointmentsCommand { get; }
        public ICommand AddAppointmentCommand { get; }
        public ICommand SaveAppointmentCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand DeleteAppointmentCommand { get; }
        public ICommand CancelCommand { get; }

        // Methods
        private async Task LoadAppointmentsAsync()
        {
            try
            {
                var appointments = await _apiService.GetAllAppointmentsAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Appointments = new ObservableCollection<AppointmentModel>(appointments);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading appointments: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrepareNewAppointment()
        {
            IsEditing = false;
            PatientName = string.Empty;
            AppointmentDate = DateTime.Now;
            Description = string.Empty;
            Status = "Scheduled";
        }

        private async Task SaveAppointmentAsync()
        {
            try
            {
                var appointment = new AppointmentModel
                {
                    PatientName = PatientName,
                    AppointmentDate = AppointmentDate,
                    Description = Description,
                    Status = Status
                };

                if (IsEditing)
                {
                    await _apiService.UpdateAppointmentAsync(_editingId, appointment);
                    MessageBox.Show("Appointment updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _apiService.CreateAppointmentAsync(appointment);
                    MessageBox.Show("Appointment created successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CancelEdit();
                await LoadAppointmentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving appointment: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditAppointment(object parameter)
        {
            if (SelectedAppointment != null)
            {
                IsEditing = true;
                _editingId = SelectedAppointment.Id;
                PatientName = SelectedAppointment.PatientName;
                AppointmentDate = SelectedAppointment.AppointmentDate;
                Description = SelectedAppointment.Description;
                Status = SelectedAppointment.Status;
            }
        }

        private async Task DeleteAppointmentAsync()
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to delete this appointment?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _apiService.DeleteAppointmentAsync(SelectedAppointment.Id);
                    MessageBox.Show("Appointment deleted successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadAppointmentsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting appointment: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;
            PatientName = string.Empty;
            AppointmentDate = DateTime.Now;
            Description = string.Empty;
            Status = "Scheduled";
        }

        private bool CanSaveAppointment()
        {
            return !string.IsNullOrWhiteSpace(PatientName);
        }
    }
}