using AppointmentSystem.WPF.Models;
using AppointmentSystem.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AppointmentSystem.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAppointmentApiService _apiService;
    private CancellationTokenSource? _cancellationTokenSource;
    private CancellationTokenSource? _searchCancellationTokenSource;

    public MainViewModel(IAppointmentApiService apiService)
    {
        _apiService = apiService;

        // Initialize with current date
        AppointmentDate = DateTime.Now.AddDays(1);

        // Load initial data
        _ = LoadAppointmentsAsync();
    }

    // ==================== Properties ====================

    [ObservableProperty]
    private ObservableCollection<AppointmentDto> _appointments = new();

    [ObservableProperty]
    private AppointmentDto? _selectedAppointment;

    [ObservableProperty]
    private string _patientName = string.Empty;

    [ObservableProperty]
    private DateTime _appointmentDate;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string _status = "Scheduled";

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private int _editingId;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 20;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _totalPages;

    public ObservableCollection<string> StatusOptions { get; } = new()
    {
        "Scheduled",
        "Completed",
        "Cancelled",
        "NoShow"
    };

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    // ==================== Commands ====================

    [RelayCommand]
    private async Task LoadAppointmentsAsync()
    {
        await ExecuteWithLoadingAsync(async (ct) =>
        {
            var result = await _apiService.GetAppointmentsAsync(
                SearchText,
                CurrentPage,
                PageSize,
                ct);

            Appointments.Clear();
            foreach (var appointment in result.Items)
            {
                Appointments.Add(appointment);
            }

            TotalCount = result.TotalCount;
            TotalPages = result.TotalPages;
            ErrorMessage = null;

            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
        });
    }

    [RelayCommand]
    private void PrepareNewAppointment()
    {
        IsEditing = false;
        PatientName = string.Empty;
        AppointmentDate = DateTime.Now.AddDays(1);
        Description = null;
        Status = "Scheduled";
        ErrorMessage = null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveAppointment))]
    private async Task SaveAppointmentAsync()
    {
        await ExecuteWithLoadingAsync(async (ct) =>
        {
            if (IsEditing)
            {
                var request = new UpdateAppointmentRequest
                {
                    PatientName = PatientName,
                    AppointmentDate = AppointmentDate,
                    Description = Description,
                    Status = Status
                };

                await _apiService.UpdateAppointmentAsync(EditingId, request, ct);
                ShowSuccess("Appointment updated successfully!");
            }
            else
            {
                var request = new CreateAppointmentRequest
                {
                    PatientName = PatientName,
                    AppointmentDate = AppointmentDate,
                    Description = Description
                };

                await _apiService.CreateAppointmentAsync(request, ct);
                ShowSuccess("Appointment created successfully!");
            }

            CancelEdit();
            await LoadAppointmentsAsync();
        });
    }

    private bool CanSaveAppointment()
    {
        return !string.IsNullOrWhiteSpace(PatientName) &&
               AppointmentDate > DateTime.Now &&
               !IsLoading;
    }

    [RelayCommand(CanExecute = nameof(CanEditDelete))]
    private void EditAppointment()
    {
        if (SelectedAppointment == null) return;

        IsEditing = true;
        EditingId = SelectedAppointment.Id;
        PatientName = SelectedAppointment.PatientName;
        AppointmentDate = SelectedAppointment.AppointmentDate;
        Description = SelectedAppointment.Description;
        Status = SelectedAppointment.Status;
        ErrorMessage = null;
    }

    private bool CanEditDelete() => SelectedAppointment != null && !IsLoading;

    [RelayCommand(CanExecute = nameof(CanEditDelete))]
    private async Task DeleteAppointmentAsync()
    {
        if (SelectedAppointment == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete the appointment for {SelectedAppointment.PatientName}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        await ExecuteWithLoadingAsync(async (ct) =>
        {
            await _apiService.DeleteAppointmentAsync(SelectedAppointment.Id, ct);
            ShowSuccess("Appointment deleted successfully!");
            await LoadAppointmentsAsync();
        });
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        PatientName = string.Empty;
        AppointmentDate = DateTime.Now.AddDays(1);
        Description = null;
        Status = "Scheduled";
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchText = string.Empty;
        CurrentPage = 1;
        await LoadAppointmentsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void ExportToCsv()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"Appointments_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };

            using var writer = new StreamWriter(saveFileDialog.FileName);
            using var csv = new CsvWriter(writer, config);

            csv.WriteRecords(Appointments);

            ShowSuccess($"Successfully exported {Appointments.Count} appointments");
        }
        catch (Exception ex)
        {
            ShowError($"Error exporting to CSV: {ex.Message}");
        }
    }

    private bool CanExport() => Appointments?.Count > 0 && !IsLoading;

    [RelayCommand(CanExecute = nameof(HasPreviousPage))]
    private async Task PreviousPageAsync()
    {
        CurrentPage--;
        await LoadAppointmentsAsync();
    }

    [RelayCommand(CanExecute = nameof(HasNextPage))]
    private async Task NextPageAsync()
    {
        CurrentPage++;
        await LoadAppointmentsAsync();
    }

    // ==================== Property Changed Handlers ====================

    partial void OnSearchTextChanged(string value)
    {
        // Debounce search
        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, _searchCancellationTokenSource.Token);
                CurrentPage = 1;
                await LoadAppointmentsAsync();
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation
            }
        });
    }

    partial void OnPatientNameChanged(string value)
    {
        SaveAppointmentCommand.NotifyCanExecuteChanged();
    }

    partial void OnAppointmentDateChanged(DateTime value)
    {
        SaveAppointmentCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsLoadingChanged(bool value)
    {
        SaveAppointmentCommand.NotifyCanExecuteChanged();
        EditAppointmentCommand.NotifyCanExecuteChanged();
        DeleteAppointmentCommand.NotifyCanExecuteChanged();
        ExportToCsvCommand.NotifyCanExecuteChanged();
        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedAppointmentChanged(AppointmentDto? value)
    {
        EditAppointmentCommand.NotifyCanExecuteChanged();
        DeleteAppointmentCommand.NotifyCanExecuteChanged();
    }

    // ==================== Helper Methods ====================

    private async Task ExecuteWithLoadingAsync(Func<CancellationToken, Task> action)
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            await action(_cancellationTokenSource.Token);
        }
        catch (ApiValidationException ex)
        {
            ErrorMessage = ex.Message;
            ShowError(ex.Message);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            ShowError(ex.Message);
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled, ignore
        }
        catch (Exception ex)
        {
            ErrorMessage = "An unexpected error occurred";
            ShowError($"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowSuccess(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    private void ShowError(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }
}