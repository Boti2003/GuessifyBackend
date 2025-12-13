using GuessifyBackend.DTO.SetupConfig;

namespace GuessifyBackend.Service.Interfaces
{
    public interface ISetupConfigService
    {
        Task<SetupConfig?> ParseConfigData(string filePath);
    }
}