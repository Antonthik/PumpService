using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PumpService
{
    //возвращаем статистику клиенту
    [ServiceContract(Namespace = "http://Microsoft.ServiceModel.Samples", SessionMode = SessionMode.Required, CallbackContract = typeof(IPumpServiceCallback))]//В рамках каждой сесси создаем экземпляр


    public interface IPumpnService
        {
            [OperationContract]
            void RunScript();//метод запуска скрипта

            [OperationContract]
            void UpdateAndCompileScript(string fileName);//метод компиляции скрипта
    }
    
}
