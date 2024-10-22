using System;

namespace PruebaBiinteli.Models;

public class Journey
{
    public int Id { get; set; }
    public string ListFlight { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public double Price { get; set; } 
  
}
