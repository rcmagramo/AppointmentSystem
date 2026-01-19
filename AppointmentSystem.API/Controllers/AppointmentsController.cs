using AppointmentSystem.API.DTOs;
using AppointmentSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppointmentSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(IAppointmentService service,
            ILogger<AppointmentsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all appointments");
                var appointments = await _service.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"Getting appointment {id}");
                var appointment = await _service.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointment {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> Create(CreateAppointmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating new appointment");
                var created = await _service.CreateAppointmentAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AppointmentDto>> Update(int id, UpdateAppointmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation($"Updating appointment {id}");
                var updated = await _service.UpdateAppointmentAsync(id, dto);
                if (updated == null)
                {
                    return NotFound();
                }
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating appointment {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting appointment {id}");
                var deleted = await _service.DeleteAppointmentAsync(id);
                if (!deleted)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting appointment {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}