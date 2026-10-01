namespace PlayViAPI.Models;

public class Plan
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string BillingPeriod { get; set; } = "Monthly"; // Monthly ou Yearly
}

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<TitleGenre> TitleGenres { get; set; } = new();
}

public class Title
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Movie"; // Movie ou Series
    public int ReleaseYear { get; set; }
    public string? Description { get; set; }
    public string? VideoUrl { get; set; }
    public string? PosterUrl { get; set; }
    public List<TitleGenre> TitleGenres { get; set; } = new();
}

public class TitleGenre
{
    public int TitleId { get; set; }
    public Title Title { get; set; } = null!;
    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}

public class Profile
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public bool IsKids { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Subscription
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active"; // Active ou Canceled
}