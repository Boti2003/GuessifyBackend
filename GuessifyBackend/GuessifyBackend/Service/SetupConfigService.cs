using GuessifyBackend.DTO.SetupConfig;
using System.Text.Json;

namespace GuessifyBackend.Service
{
    public class SetupConfigService
    {
        public async Task<SetupConfig> ParseConfigData(string filePath)
        {
            FileStream stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<SetupConfig>(stream);
        }

    }
}
