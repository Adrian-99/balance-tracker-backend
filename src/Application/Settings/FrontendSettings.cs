using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Settings
{
    public class FrontendSettings
    {
        public string Address { get; set; }

        public static FrontendSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("Frontend")
                .Get<FrontendSettings>();
        }
    }
}
