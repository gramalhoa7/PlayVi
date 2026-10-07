namespace PlayViApp.Services;

public record AvatarOption(string Id)
{
    public string Image => $"{Id}.png";
}

public static class Avatars
{
    public static readonly List<AvatarOption> All = new()
    {
        new("bolsonarodanca"),
        new("bolsonarolaion"),
        new("cristianonaometratariaassim"),
        new("souhomofobico"),
        new("neymar"),
        new("abner"),
        new("cachorro"),
        new("drrat"),
        new("maca"),
        new("minion"),
    };
}