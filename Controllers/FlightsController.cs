using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace PruebaBiinteli.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public FlightsController(HttpClient httpClient)
        {
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
                var flightData = JsonSerializer.Deserialize<dynamic>(jsonResponse, options);


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
