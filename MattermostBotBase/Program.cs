using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceCore.Global.DataObjects;
using ServiceCore.Global.Logging;
using ServiceCore.Services;
using ServiceCore.Services.RestService;
using ServiceCore.Services.RestService.Examples;
using ServiceCore.Services.TopShelf;

namespace MattermostBotBase
{
    class Program
    {
        private static void Main(string[] args)
        {
            //Do dis to prevent not initialized exceptions when getting the service
            ServiceManager<RestService>.Initialize();
            //Create a logger
            var logger = new ServiceLogger(new LoggerSettings("Test"));
            try
            {
                //-------------Without Top shelf
                //Get the actual service to deal with (static, so only 1 instance in existence)
                var service = ServiceManager<RestService>.GetService();

                //Let the service configure itself if you dont need something special
                var config = service.Configure(logger, new ExampleSetting { AutoConfig = true, ExampleDisabled = true });
                //That's already it, have fun!
                service.Start(config);
                //Automatically disposed as well
                service.Stop();

                //-------------With Top shelf
                //Configure again since it got disposed
                var serviceConfig = service.Configure(logger, new ExampleSetting { AutoConfig = true, ExampleDisabled = true });
                //Initialize the host factory helper
                HostFactoryHelper.Init(logger);
                HostFactoryHelper.Run(service, serviceConfig,
                    new HostConfiguration()
                    {
                        Description = "My Mattermost Service",
                        DisplayName = "Best Mattermost Service Ever",
                        ServiceName = "Best Mattermost Service Ever"
                    });
            }
            catch (Exception e)
            {
                //do sth..
                logger.Error("Failed to start the service", e);
            }
        }
    }
}
