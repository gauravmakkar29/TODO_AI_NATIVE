namespace TodoApi.Models.DTOs;

public class ThemePreferenceRequest
{
    public string Theme { get; set; } = "light"; // "light" or "dark"
}

public class ThemePreferenceResponse
{
    public string Theme { get; set; } = "light";
}


