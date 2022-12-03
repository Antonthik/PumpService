using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PumpService
{
    /// <summary>
    /// Сервис возвращения статистики
    /// </summary>
    [ServiceContract]
    public interface IPumpServiceCallback
    {
        [OperationContract]
        void UpdateStatistics(StatisticsService statistics);
    }
}
