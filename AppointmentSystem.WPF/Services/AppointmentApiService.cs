using AppointmentSystem.WPF.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentSystem.WPF.Services
{
    public class AppointmentApiService : IAppointmentApiService
    {
        private readonly HttpClient _httpClient;

        public AppointmentApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
           
            _httpClient.BaseAddress = new Uri("https://localhost:7205");
        }

        public async Task<List<AppointmentModel>> GetAllAppointmentsAsync()
        {
            var response = await _httpClient.GetAsync("/api/appointments");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<AppointmentModel>>(content);
        }

        public async Task<AppointmentModel> GetAppointmentByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/appointments/{id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AppointmentModel>(content);
        }

        public async Task<AppointmentModel> CreateAppointmentAsync(AppointmentModel appointment)
        {
            var json = JsonConvert.SerializeObject(appointment);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/appointments", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AppointmentModel>(responseContent);
        }

        public async Task<AppointmentModel> UpdateAppointmentAsync(int id, AppointmentModel appointment)
        {
            var json = JsonConvert.SerializeObject(appointment);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"/api/appointments/{id}", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AppointmentModel>(responseContent);
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/appointments/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}