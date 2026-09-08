using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Hente-funktion: Læser og oversætter JSON-filen til C#-objekter
List<Agent> GetAOSFromFile()
{
    var jsonText = File.ReadAllText("AOS.json");
    // Vi bruger JsonSerializer til at omdanne teksten til rigtige data, .NET kan forstå
    var data = JsonSerializer.Deserialize<List<Agent>>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    return data ?? new List<Agent>();
}

// 1. Det gamle endpoint: Hent ALT (hele JSON-strukturen)
app.MapGet("/AOS", () => {
    return Results.Ok(GetAOSFromFile());
});

// 2. NYT: Hent KUN en liste med alle navnene
app.MapGet("/AOS/names", () => {
    var allAOS = GetAOSFromFile();

    // Her trækker vi KUN "Navn" ud fra hver bruger
    var namesOnly = allAOS.Select(u => u.navn).ToList();

    return Results.Ok(namesOnly);
});

// 3. NYT: Hent KUN én specifik agent ud fra deres Navn (f.eks. /AOS/Coulson)
app.MapGet("/AOS/{navn}", (string navn) => {
    var allAOS = GetAOSFromFile();

    // Vi leder efter agenter, hvor navnet indeholder det, man søgte efter
    // StringComparison.OrdinalIgnoreCase gør den ligeglad med store/små bogstaver
    var matchingAgents = allAOS
        .Where(u => u.navn.Contains(navn, StringComparison.OrdinalIgnoreCase))
        .ToList();

    // Hvis vi ikke fandt nogen overhovedet
    if (!matchingAgents.Any())
    {
        return Results.NotFound(new { fejl = $"Ingen agenter matchede søgningen: '{navn}'" });
    }

    return Results.Ok(matchingAgents);
});

app.Run();

// --- herunder fortæller vi .NET, hvordan din JSON-fil er bygget op ---
public class AgentRoot
{
    public List<Agent> AOS { get; set; } = new();
}

public class Agent
{
    public string navn { get; set; } = string.Empty;
    public string rolle { get; set; } = string.Empty;
    public int saeson { get; set; }
    public string race { get; set; } = string.Empty;
}
