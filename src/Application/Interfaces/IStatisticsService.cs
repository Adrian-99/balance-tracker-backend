using Application.Dtos.Ingoing.Statistics;
using Application.Dtos.Outgoing.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStatisticsService
    {
        Task<StatisticsResponseDto> GenerateStatisticsAsync(Guid userId, StatisticsRequestDto statisticsRequest);
    }
}
