using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using PruebaBiinteli.Models;
using PruebaBiinteli.Dtos;
using PruebaBiinteli.Data;
using Microsoft.EntityFrameworkCore;

namespace PruebaBiinteli.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Contexto para interactuar con la base de datos
        private readonly HttpClient _httpClient; // Cliente HTTP para hacer solicitudes a APIs externas

        public FlightsController(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context; // Inicializa el contexto de la base de datos
            _httpClient = httpClient; // Inicializa el cliente HTTP
        }

        /// <summary>
        /// Obtiene vuelos desde una API externa, los procesa y guarda en la base de datos si no existen.
        /// </summary>
        /// <returns>Devuelve los datos de vuelos obtenidos y guardados.</returns>
        [HttpGet]
        public async Task<IActionResult> GetFlights()
        {
            // URL de la API externa que proporciona los datos de vuelos
            var apiUrl = "https://bitecingcom.ipage.com/testapi/avanzado.js";

            try
            {
                // Hacer la solicitud HTTP GET a la API externa
                var response = await _httpClient.GetAsync(apiUrl);

                // Verificar si la solicitud fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Error al obtener los datos de la API externa.");
                }

                // Leer el contenido de la respuesta como una cadena JSON
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Opciones para deserializar el JSON, permitiendo comas finales
                var options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true
                };

                // Deserializar el JSON en una lista de objetos FlyghtDto
                var flightData = JsonSerializer.Deserialize<List<FlyghtDto>>(jsonResponse, options);

                // Si se obtuvieron datos válidos de la API
                if (flightData != null)
                {
                    foreach (var flight in flightData)
                    {
                        // Verifica si el transporte (aerolínea y número de vuelo) ya existe
                        var existingTransport = await _context.Transports
                            .FirstOrDefaultAsync(t => t.FlightCarrier == flight.FlightCarrier &&
                                                      t.FlightNumber == flight.FlightNumber);

                        Transports transport;
                        if (existingTransport != null)
                        {
                            // Si ya existe, se usa el transporte existente
                            transport = existingTransport;
                        }
                        else
                        {
                            // Si no existe, se crea uno nuevo
                            transport = new Transports
                            {
                                FlightCarrier = flight.FlightCarrier,
                                FlightNumber = flight.FlightNumber
                            };
                            _context.Transports.Add(transport); // Se agrega el transporte a la base de datos
                            await _context.SaveChangesAsync(); // Guardar cambios en la base de datos
                        }

                        // Verifica si el vuelo ya existe en la base de datos
                        var existingFlight = await _context.Flights
                            .FirstOrDefaultAsync(f => f.Origin == flight.DepartureStation &&
                                                      f.Destination == flight.ArrivalStation &&
                                                      f.Price == flight.Price);

                        if (existingFlight == null)
                        {
                            // Si el vuelo no existe, se crea uno nuevo
                            var flightModel = new Flights
                            {
                                Origin = flight.DepartureStation,
                                Destination = flight.ArrivalStation,
                                Price = flight.Price,
                                FlightNumber = flight.FlightNumber // Asigna la relación con el transporte
                            };

                            // Se agrega el vuelo a la base de datos
                            _context.Flights.Add(flightModel);
                        }
                    }

                    // Guardar todos los cambios realizados en la base de datos
                    await _context.SaveChangesAsync();
                }

                // Devolver la respuesta con los datos de vuelo procesados
                return Ok(flightData);
            }
            catch (HttpRequestException ex)
            {
                // Manejar los errores de la solicitud HTTP
                return StatusCode(500, $"Error al realizar la solicitud a la API externa: {ex.Message}");
            }
        }
    }
}
