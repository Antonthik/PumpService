using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpService
{
    internal interface IScriptService
    {
        bool Compile();//метод компиляции
        void Run(int count);// count - количество запусков скрипта
    }
}
