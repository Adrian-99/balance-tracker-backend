using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Settings
{
    internal class DataSettings
    {
        public string CategoriesListPath { get; set; }

        public static DataSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("Data")
                .Get<DataSettings>();
        }
    }
}
