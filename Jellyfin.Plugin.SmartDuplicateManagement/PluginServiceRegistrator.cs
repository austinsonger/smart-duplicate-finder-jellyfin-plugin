using Jellyfin.Plugin.SmartDuplicateManagement.Services;
using Jellyfin.Plugin.SmartDuplicateManagement.Tasks;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.SmartDuplicateManagement
{
    /// <summary>
    /// Register all plugin services with Jellyfin's dependency injection container.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <summary>
        /// Registers the services.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="applicationHost">The application host.</param>
        public void RegisterServices(IServiceCollection serviceCollection, MediaBrowser.Common.IApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<DuplicateDetectionEngine>();
            serviceCollection.AddSingleton<QualityAnalyzer>();
            serviceCollection.AddSingleton<MetadataMerger>();
            serviceCollection.AddSingleton<DataPersistenceService>();
            serviceCollection.AddSingleton<DuplicateScanTask>();
        }
    }
}
