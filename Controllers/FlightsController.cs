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

        private readonly ApplicationDbContext _context; // Contexto de la base de datos
        private readonly HttpClient _httpClient;

        public FlightsController(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context; // Inicializa el contexto
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetFlights()
        {
            // URL de la API externa
            var apiUrl = "https://bitecingcom.ipage.com/testapi/avanzado.js";

            try
            {
                // Hacer la solicitud HTTP GET
                var response = await _httpClient.GetAsync(apiUrl);

                // Verificar si la solicitud fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Error al obtener los datos de la API externa.");
                }

                // Leer el contenido de la respuesta como una cadena JSON
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true
                };

                // Procesar el JSON (deserializarlo)
                var flightData = JsonSerializer.Deserialize<List<FlyghtDto>>(jsonResponse, options);

                // Guardar los datos en la base de datos
                if (flightData != null)
                {
                    foreach (var flight in flightData)
                    {
                        // Verifica si el transporte ya existe
                        var existingTransport = await _context.Transports
                            .FirstOrDefaultAsync(t => t.FlightCarrier == flight.FlightCarrier &&
                                                       t.FlightNumber == flight.FlightNumber);

                        Transports transport;
                        if (existingTransport != null)
                        {
                            // Si ya existe, usar el existente
                            transport = existingTransport;
                        }
                        else
                        {
                            // Si no existe, crear uno nuevo
                            transport = new Transports
                            {
                                FlightCarrier = flight.FlightCarrier,
                                FlightNumber = flight.FlightNumber
                            };
                            // Guarda el nuevo transporte en la base de datos
                            _context.Transports.Add(transport);
                            await _context.SaveChangesAsync(); // Guardar los cambios
                        }

                        // Ahora verifica si el vuelo ya existe
                        var existingFlight = await _context.Flights
                            .FirstOrDefaultAsync(f => f.Origin == flight.DepartureStation &&
                                                       f.Destination == flight.ArrivalStation &&
                                                       f.Price == flight.Price);

                        if (existingFlight == null)
                        {
                            // Si no existe, guardar el nuevo vuelo
                            var flightModel = new Flights
                            {
                                Origin = flight.DepartureStation,
                                Destination = flight.ArrivalStation,
                                Price = flight.Price,
                                FlightNumber = flight.FlightNumber // Asigna la relación con el transporte
                            };

                            // Guarda el vuelo en la base de datos
                            _context.Flights.Add(flightModel);
                        }
                    }

                    // Guardar todos los cambios
                    await _context.SaveChangesAsync();
                }
            


            // Devolver la respuesta procesada (opcionalmente, puedes transformarla antes de devolverla)
            return Ok(flightData);
        }
            catch (HttpRequestException ex)
            {
                // Manejar errores en la solicitud
                return StatusCode(500, $"Error al realizar la solicitud a la API externa: {ex.Message}");
    }
}


    }
}
