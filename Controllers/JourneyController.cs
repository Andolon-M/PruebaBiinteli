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

        // Constructor que inyecta el contexto de base de datos
        public JourneyController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Endpoint para obtener vuelos desde un origen hacia un destino.
        /// Verifica primero si ya existen rutas almacenadas en la base de datos,
        /// y si no, busca rutas directas o con escalas.
        /// </summary>
        /// <param name="origen">Ciudad de origen</param>
        /// <param name="destino">Ciudad de destino</param>
        /// <returns>Retorna las rutas encontradas o un mensaje de error</returns>
        [HttpGet("find-flights")]
        public async Task<IActionResult> GetFlights(string origen, string destino)
        {
            // Verificar que los parámetros de origen y destino no estén vacíos
            if (string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(destino))
            {
                return BadRequest("Los parámetros de origen y destino son requeridos.");
            }

            // Verificar si la ruta ya existe en la base de datos
            var routeExist = await RouteExist(origen, destino);
            if (routeExist.Any()) // Si ya existe, la retornamos
            {
                return Ok(routeExist); // Retorna las rutas existentes
            }

            // Obtener vuelos directos
            var directRoutes = await GetDirectFlights(origen, destino);

            // Si se encuentran rutas directas, se guardan y se retornan
            if (directRoutes.Any())
            {
                await SaveJourneysToDatabase(directRoutes);
                return Ok(directRoutes);
            }

            // Si no hay rutas directas, obtener vuelos con escalas
            var connectingRoutes = await GetConnectingFlights(origen, destino);

            // Si no se encuentran rutas de conexión
            if (!connectingRoutes.Any())
            {
                return NotFound("No se encontraron vuelos disponibles.");
            }

            // Guardar las rutas de conexión en la base de datos
            await SaveJourneysToDatabase(connectingRoutes);

            return Ok(connectingRoutes); // Retorna las rutas encontradas
        }

        /// <summary>
        /// Obtiene vuelos directos entre un origen y un destino.
        /// </summary>
        /// <param name="origen">Ciudad de origen</param>
        /// <param name="destino">Ciudad de destino</param>
        /// <returns>Lista de rutas de vuelos directos</returns>
        private async Task<List<FlightRoute>> GetDirectFlights(string origen, string destino)
        {
            // Buscar vuelos directos en la base de datos
            var directFlights = await _context.Flights
                .Where(f => f.Origin == origen && f.Destination == destino)
                .ToListAsync();

            // Si se encuentran vuelos directos, calcular el precio total y crear una ruta
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

            // Si no hay vuelos directos, retornar una lista vacía
            return new List<FlightRoute>();
        }

        /// <summary>
        /// Busca vuelos con escalas entre un origen y un destino.
        /// </summary>
        /// <param name="origen">Ciudad de origen</param>
        /// <param name="destino">Ciudad de destino</param>
        /// <returns>Lista de rutas de vuelos con escalas</returns>
        private async Task<List<FlightRoute>> GetConnectingFlights(string origen, string destino)
        {
            // Obtener vuelos desde el origen hacia cualquier destino intermedio
            var flightsFromOrigin = await _context.Flights
                .Where(f => f.Origin == origen)
                .ToListAsync();

            var routes = new List<FlightRoute>();

            // Iterar sobre los vuelos que parten del origen
            foreach (var flight in flightsFromOrigin)
            {
                // Buscar vuelos de conexión desde el destino del vuelo actual hasta el destino final
                var connectingFlights = await _context.Flights
                    .Where(f => f.Origin == flight.Destination && f.Destination == destino)
                    .ToListAsync();

                // Crear rutas de vuelos con escalas
                foreach (var connectingFlight in connectingFlights)
                {
                    var route = new FlightRoute
                    {
                        Origin = origen,
                        Destination = destino,
                        TotalPrice = flight.Price + connectingFlight.Price,
                        Flights = new List<Flights> { flight, connectingFlight }
                    };
                    routes.Add(route); // Añadir la ruta a la lista de rutas
                }
            }

            return routes; // Retornar las rutas con escalas
        }

        /// <summary>
        /// Guarda las rutas de vuelos en la base de datos, 
        /// asegurándose de no duplicar rutas existentes.
        /// </summary>
        /// <param name="routes">Lista de rutas a guardar</param>
        private async Task SaveJourneysToDatabase(List<FlightRoute> routes)
        {
            foreach (var route in routes)
            {
                // Verificar si la ruta ya existe en la base de datos
                var existingJourney = await _context.Journeys
                    .FirstOrDefaultAsync(j =>
                        j.Origin == route.Origin &&
                        j.Destination == route.Destination &&
                        j.Price == route.TotalPrice);

                // Si la ruta ya existe, no se guarda de nuevo
                if (existingJourney != null)
                {
                    continue; // Salta a la siguiente ruta
                }

                // Crear y añadir una nueva ruta si no existe
                var journey = new Journey
                {
                    ListFlight = string.Join(", ", route.Flights.Select(f => f.FlightNumber)),
                    Origin = route.Origin,
                    Destination = route.Destination,
                    Price = route.TotalPrice
                };

                _context.Journeys.Add(journey); // Añadir la nueva ruta
            }

            await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos
        }

        /// <summary>
        /// Verifica si ya existen rutas entre un origen y un destino en la base de datos.
        /// </summary>
        /// <param name="origen">Ciudad de origen</param>
        /// <param name="destino">Ciudad de destino</param>
        /// <returns>Lista de rutas existentes</returns>
        private async Task<List<Journey>> RouteExist(string origen, string destino)
        {
            // Buscar rutas existentes en la base de datos entre el origen y el destino
            var existingJourneys = await _context.Journeys
                .Where(j => j.Origin == origen && j.Destination == destino)
                .ToListAsync();

            // Retornar la lista de rutas existentes o una lista vacía
            return existingJourneys;
        }

    }

    /// <summary>
    /// Clase que representa una ruta de vuelo,
    /// con detalles del origen, destino, precio total y los vuelos que componen la ruta.
    /// </summary>
    public class FlightRoute
    {
        public string Origin { get; set; } // Ciudad de origen
        public string Destination { get; set; } // Ciudad de destino
        public double TotalPrice { get; set; } // Precio total de la ruta
        public List<Flights> Flights { get; set; } // Lista de vuelos que componen la ruta
    }
}
