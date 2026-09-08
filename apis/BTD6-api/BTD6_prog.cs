using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Hente-funktion: Læser og oversætter JSON-filen til C#-objekter
List<Tower> GetTowersFromFile()
{
    var jsonText = File.ReadAllText("BTD6.json");
    // Vi bruger JsonSerializer til at omdanne teksten til rigtige data, .NET kan forstå
    var data = JsonSerializer.Deserialize<List<Tower>>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    return data ?? new List<Tower>();
}

// 1. Det gamle endpoint: Hent ALT (hele JSON-strukturen)
app.MapGet("/BTD6", () => {
    return Results.Ok(GetTowersFromFile());
});

// 2. NYT: Hent KUN en liste med alle navnene
app.MapGet("/BTD6/names", () => {
    var allTowers = GetTowersFromFile();

    // Her trækker vi KUN "Navn" ud fra hver bruger
    var namesOnly = allTowers.Select(u => u.navn).ToList();

    return Results.Ok(namesOnly);
});

// 3. NYT: Hent KUN én specifik agent ud fra deres Navn (f.eks. /AOS/Coulson)
app.MapGet("/BTD6/{navn}", (string navn) => {
    var allTowers = GetTowersFromFile();

    // Vi leder efter agenter, hvor navnet indeholder det, man søgte efter
    // StringComparison.OrdinalIgnoreCase gør den ligeglad med store/små bogstaver
    var matchingTowers = allTowers
        .Where(u => u.navn.Contains(navn, StringComparison.OrdinalIgnoreCase))
        .ToList();

    // Hvis vi ikke fandt nogen overhovedet
    if (!matchingTowers.Any())
    {
        return Results.NotFound(new { fejl = $"Ingen towers matchede søgningen: '{navn}'" });
    }

    return Results.Ok(matchingTowers);
});

app.Run();

// --- herunder fortæller vi .NET, hvordan din JSON-fil er bygget op ---
public class TowerRoot
{
    public List<Tower> Towers { get; set; } = new();
}

public class Tower
{
    public string navn { get; set; } = string.Empty;
    public string rolle { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public string kategori { get; set; } = string.Empty;
}
