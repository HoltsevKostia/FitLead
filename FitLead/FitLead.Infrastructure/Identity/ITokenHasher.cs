namespace FitLead.Infrastructure.Identity
{
    public interface ITokenHasher
    {
        string ComputeSha256Base64(string token);
        bool FixedTimeEquals(string left, string right);
    }
}
