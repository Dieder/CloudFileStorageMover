namespace FileMover.UI.Services;

public interface ICredentialsService
{
    Task SaveCredentialsAsync(string providerId, Dictionary<string, string> credentials);
    Task<Dictionary<string, string>?> GetCredentialsAsync(string providerId);
    Task ClearCredentialsAsync(string providerId);
    Task<bool> IsConfiguredAsync(string providerId);
}
