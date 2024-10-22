using System;

namespace PruebaBiinteli.Models;

public class Flights
{
    public int Id { get; set; }
    public string Origin { get; set; } // Change this if you need a specific length, e.g., string Origin { get; set; } = string.Empty; for non-nullable
    public string Destination { get; set; } // Same as above
    public double Price { get; set; } // Use double instead of float
    // Add other properties as needed
}

