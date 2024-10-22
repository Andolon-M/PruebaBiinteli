using System;
using System.Collections.Generic; // Agregar esto para usar List

namespace PruebaBiinteli.Models
{
    public class Transports
    {
        public int Id { get; set; }
        public string FlightCarrier { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;

    }
}
