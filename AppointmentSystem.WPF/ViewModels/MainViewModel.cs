using AppointmentSystem.WPF.Commands;
using AppointmentSystem.WPF.Models;
using AppointmentSystem.WPF.Services;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AppointmentSystem.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IAppointmentApiService _apiService;
        private ObservableCollection<AppointmentModel> _allAppointments; 

        public List<string> StatusOptions { get; } = new List<string>
        {
            "Scheduled",
            "Completed",
            "Cancelled"
        };

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
            ExportToCsvCommand = new RelayCommand(_ => ExportToCsv(),
                _ => Appointments != null && Appointments.Count > 0);
            ClearSearchCommand = new RelayCommand(_ => ClearSearch()); 

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

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterAppointments();
                }
            }
        }

        // Commands
        public ICommand LoadAppointmentsCommand { get; }
        public ICommand AddAppointmentCommand { get; }
        public ICommand SaveAppointmentCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand DeleteAppointmentCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ICommand ClearSearchCommand { get; }

        // Methods
        private async Task LoadAppointmentsAsync()
        {
            try
            {
                var appointments = await _apiService.GetAllAppointmentsAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _allAppointments = new ObservableCollection<AppointmentModel>(appointments);
                    FilterAppointments(); 
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading appointments: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Filter appointments based on search text
        private void FilterAppointments()
        {
            if (_allAppointments == null)
                return;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
               Appointments = new ObservableCollection<AppointmentModel>(_allAppointments);
            }
            else
            {
                var filtered = _allAppointments.Where(a =>
                    a.PatientName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Appointments = new ObservableCollection<AppointmentModel>(filtered);
            }
        }

        // Clear search
        private void ClearSearch()
        {
            SearchText = string.Empty;
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

        private void ExportToCsv()
        {
            try
            {
                var appointmentsToExport = Appointments ?? _allAppointments;

                if (appointmentsToExport == null || appointmentsToExport.Count == 0)
                {
                    MessageBox.Show("No appointments to export.", "Export",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"Appointments_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                    };

                    using (var writer = new StreamWriter(saveFileDialog.FileName))
                    using (var csv = new CsvWriter(writer, config))
                    {
                        // Write header
                        csv.WriteField("ID");
                        csv.WriteField("Patient Name");
                        csv.WriteField("Appointment Date");
                        csv.WriteField("Status");
                        csv.WriteField("Description");
                        csv.NextRecord();

                        // Write data
                        foreach (var appointment in appointmentsToExport)
                        {
                            csv.WriteField(appointment.Id);
                            csv.WriteField(appointment.PatientName);
                            csv.WriteField(appointment.AppointmentDate.ToString("yyyy-MM-dd HH:mm:ss"));
                            csv.WriteField(appointment.Status);
                            csv.WriteField(appointment.Description ?? string.Empty);
                            csv.NextRecord();
                        }
                    }

                    var message = string.IsNullOrWhiteSpace(SearchText)
                        ? $"Successfully exported {appointmentsToExport.Count} appointments"
                        : $"Successfully exported {appointmentsToExport.Count} filtered appointments";

                    MessageBox.Show($"{message} to:\n{saveFileDialog.FileName}",
                        "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}