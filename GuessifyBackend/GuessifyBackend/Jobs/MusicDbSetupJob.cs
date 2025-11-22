using GuessifyBackend.Service.Interfaces;
using Quartz;

namespace GuessifyBackend.Jobs
{
    public class MusicDbSetupJob : IJob
    {
        private ISetupConfigService _setupConfigService;
        private IMusicDbSetupService _musicDbSetupService;
        private readonly ILogger<MusicDbSetupJob> _logger;
        public MusicDbSetupJob(ISetupConfigService setupConfigService, IMusicDbSetupService musicDbSetupService, ILogger<MusicDbSetupJob> logger)
        {
            _setupConfigService = setupConfigService;
            _musicDbSetupService = musicDbSetupService;
            _logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var setup = await _setupConfigService.ParseConfigData("setup_game.json");
                if (setup == null)
                {

                    throw new JobExecutionException("MusicdbSetup job failed: setup configuration is null");
                }
                await _musicDbSetupService.BuildMusicDbStructure(setup);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during MusicDbSetupJob: " + ex.Message);

                throw new JobExecutionException("MusicdbSetup job failed" + ex);
            }


        }
    }
}
