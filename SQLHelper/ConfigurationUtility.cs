using Microsoft.Extensions.Configuration;

namespace SQLHelper
{
    public class ConfigurationUtility
    {
        private static IConfiguration _configuration;

        public static void Configure(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static IConfiguration GetConfiguration()
        {
            return _configuration;
        }
    }
}
