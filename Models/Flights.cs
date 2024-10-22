using System;

namespace PruebaBiinteli.Models;

public class Flights
{    public int Id { get; set; } 
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public double Price { get; set; } 
    public int Transport { get; set; }
}
