using GuessifyBackend.DTO.SetupConfig;

namespace GuessifyBackend.Service.Interfaces
{
    public interface IMusicDbSetupService
    {
        Task BuildMusicDbStructure(SetupConfig config);
    }
}