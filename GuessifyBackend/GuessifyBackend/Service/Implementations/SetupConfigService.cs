using GuessifyBackend.DTO.SetupConfig;
using GuessifyBackend.Service.Interfaces;
using System.Text.Json;

namespace GuessifyBackend.Service.Implementations
{
    public class SetupConfigService : ISetupConfigService
    {
        public async Task<SetupConfig?> ParseConfigData(string filePath)
        {
            FileStream stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<SetupConfig>(stream);
        }

    }
}
