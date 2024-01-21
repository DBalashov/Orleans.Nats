namespace Orleans.Nats.Implementations.Membership;

static class Extenders
{
    public static string CreateEtag() =>
        Guid.NewGuid().ToString();
}