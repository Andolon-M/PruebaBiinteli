using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaBiinteli.Data;
using PruebaBiinteli.Models;

namespace PruebaBiinteli.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JourneyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JourneyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("find-flights")]
        public async Task<IActionResult> GetFlights(string origen, string destino)
        {
            if (string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(destino))
            {
                return BadRequest("Los parámetros de origen y destino son requeridos.");
            }

            // Verificar si la ruta ya existe
            var routeExist = await RouteExist(origen, destino);
            if (routeExist.Any()) // Verifica si hay rutas existentes
            {
                return Ok(routeExist); // Retorna las rutas existentes
            }

            // Obtener vuelos directos
            var directRoutes = await GetDirectFlights(origen, destino);

            // Si hay rutas directas, las guardamos y retornamos
            if (directRoutes.Any())
            {
                await SaveJourneysToDatabase(directRoutes);
                return Ok(directRoutes);
            }

            // Obtener vuelos con escalas
            var connectingRoutes = await GetConnectingFlights(origen, destino);

            // Si no hay rutas de conexión
            if (!connectingRoutes.Any())
            {
                return NotFound("No se encontraron vuelos disponibles.");
            }

            // Guardar rutas de conexión en la base de datos
            await SaveJourneysToDatabase(connectingRoutes);

            return Ok(connectingRoutes);
        }

        private async Task<List<FlightRoute>> GetDirectFlights(string origen, string destino)
        {
            var directFlights = await _context.Flights
                .Where(f => f.Origin == origen && f.Destination == destino)
                .ToListAsync();

            if (directFlights.Any())
            {
                var totalPrice = directFlights.Sum(f => f.Price);
                return new List<FlightRoute>
                {
                    new FlightRoute
                    {
                        Origin = origen,
                        Destination = destino,
                        TotalPrice = totalPrice,
                        Flights = directFlights
                    }
                };
            }

            return new List<FlightRoute>();
        }

        private async Task<List<FlightRoute>> GetConnectingFlights(string origen, string destino)
        {
            var flightsFromOrigin = await _context.Flights
                .Where(f => f.Origin == origen)
                .ToListAsync();

            var routes = new List<FlightRoute>();

            foreach (var flight in flightsFromOrigin)
            {
                var connectingFlights = await _context.Flights
                    .Where(f => f.Origin == flight.Destination && f.Destination == destino)
                    .ToListAsync();

                foreach (var connectingFlight in connectingFlights)
                {
                    var route = new FlightRoute
                    {
                        Origin = origen,
                        Destination = destino,
                        TotalPrice = flight.Price + connectingFlight.Price,
                        Flights = new List<Flights> { flight, connectingFlight }
                    };
                    routes.Add(route);
                }
            }

            return routes;
        }

        private async Task SaveJourneysToDatabase(List<FlightRoute> routes)

        {
            foreach (var route in routes)
            {
                // Verificar si ya existe una ruta con los mismos campos
                var existingJourney = await _context.Journeys
                    .FirstOrDefaultAsync(j =>
                        j.Origin == route.Origin &&
                        j.Destination == route.Destination &&
                        j.Price == route.TotalPrice);

                // Si la ruta ya existe, no la guardamos
                if (existingJourney != null)
                {
                    continue; // Salta a la siguiente ruta
                }

                // Crear nueva ruta si no existe
                var journey = new Journey
                {
                    ListFlight = string.Join(", ", route.Flights.Select(f => f.FlightNumber)),
                    Origin = route.Origin,
                    Destination = route.Destination,
                    Price = route.TotalPrice
                };

                _context.Journeys.Add(journey);
            }

            await _context.SaveChangesAsync(); // Guardar cambios en la base de datos
        }

        private async Task<List<Journey>> RouteExist(string origen, string destino)
        {
            // Verificar si ya existen rutas con los mismos campos
            var existingJourneys = await _context.Journeys
                .Where(j => j.Origin == origen && j.Destination == destino)
                .ToListAsync();

            // Retornar la lista de rutas existentes o una lista vacía
            return existingJourneys;
        }

    }

    public class FlightRoute
    {
        public string Origin { get; set; } // Origen inicial
        public string Destination { get; set; } // Destino final
        public double TotalPrice { get; set; } // Precio total de la ruta
        public List<Flights> Flights { get; set; } // Listado de vuelos
    }
}
