using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace PumpService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]//Наш сервис будет работать в концепции сессии пользователей (PerSession - объкт создается на каждую сессию)
    public class PumpService : IPumpnService
    {
        private readonly IScriptService _scriptService;
        private readonly IStatisticsService _statisticsService;
        private readonly ISettingsService _serviceSettings;

        public PumpService()
        {
            _statisticsService = new StatisticsService();
            _serviceSettings = new SettingsService();
            _scriptService = new ScriptService(Callback, _serviceSettings, _statisticsService);
        }

        /// <summary>
        /// Запуск скрипта
        /// </summary>
        public void RunScript()
        {
            _scriptService.Run(10);
        }

        public void UpdateAndCompileScript(string fileName)
        {
            _serviceSettings.FileName = fileName;
            _scriptService.Compile();
        }

        //Свойство
        IPumpServiceCallback Callback
        {
            get
            {
                if (OperationContext.Current != null)
                    return OperationContext.Current.GetCallbackChannel<IPumpServiceCallback>();
                else
                    return null;
            }
        }
    }
}
