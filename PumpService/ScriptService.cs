using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PumpService
{
    //Класс для компиляции и запуска скрипта
    public class ScriptService : IScriptService
    {
        private CompilerResults results = null;//переменная - результат компиляции готовый для запуска 
        private readonly IStatisticsService _statisticsService;//Сервис для выдачи статистики
        private readonly ISettingsService _settingsService;//Настройки сервисв - по какому имени файла будет запуск скрипта
        private readonly IPumpServiceCallback _pumpServiceCallback;//Уведомление клиента

        public ScriptService(
            IPumpServiceCallback callback,
            ISettingsService serviceSettings,
            IStatisticsService statisticsService)
        {
            _settingsService = serviceSettings;
            _statisticsService = statisticsService;
            _pumpServiceCallback = callback;
        }
        /// <summary>
        /// Компиляция и сборка скрипта
        /// </summary>
        /// <returns></returns>
        public bool Compile()
        {
            try
            {
                CompilerParameters compilerParameters = new CompilerParameters();//параметры для компиляции
                compilerParameters.GenerateInMemory = true;//Сборка и компиляция в памяти

                //Библиотеки для компиляции скрипта
                compilerParameters.ReferencedAssemblies.Add("System.dll");
                compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
                compilerParameters.ReferencedAssemblies.Add("System.Data.dll");
                compilerParameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                compilerParameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

                FileStream fileStream = new FileStream(_settingsService.FileName, FileMode.Open);
                byte[] buffer;//загружаем файл
                try
                {
                    int length = (int)fileStream.Length;
                    buffer = new byte[length];
                    int count;
                    int sum = 0;
                    while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                        sum += count;
                }
                finally
                {
                    fileStream.Close();
                }
                CSharpCodeProvider provider = new CSharpCodeProvider();//Инициализация кода
                results = provider.CompileAssemblyFromSource(compilerParameters, System.Text.Encoding.UTF8.GetString(buffer));//Компилируем из источника - библиотеки + текст кода

                //Проверяем результат компиляции на валидность
                if (results.Errors != null && results.Errors.Count != 0)
                {
                    string compileErrors = string.Empty;//строка ошибок
                    for (int i = 0; i < results.Errors.Count; i++)
                    {
                        if (compileErrors != string.Empty)
                        {
                            compileErrors += "\n";
                        }
                        compileErrors += results.Errors[i];
                    }

                    return false;//не откомпилировали
                }
                return true;//все успешно
            }
            catch (Exception e)
            {
                return false;
            }
        }
        /// <summary>
        /// Запуск скрипта
        /// </summary>
        /// <param name="count"></param>
        public void Run(int count)
        {
            if (results == null || (results != null && results.Errors != null && results.Errors.Count > 0))
            {
                if (Compile() == false)
                {
                    return;//выход
                }
            }

            Type t = results.CompiledAssembly.GetType("Sample.SampleScript");
            if (t == null)
            {
                return;//выход
            }
            MethodInfo entryPointMethod = t.GetMethod("EntryPoint");
            if (entryPointMethod == null)
            {
                return;//Если точки входа нет, то выходим
            }

            Task.Run(() =>
            {
                for (int i = 0; i < count; i++)//несколько раз выполняем метод entryPoin
                {
                    if ((bool)entryPointMethod.Invoke(Activator.CreateInstance(t), null))//Activator-создает объект на базе типа.Второй параметр -входные параметры объекта/метода(EntryPoint).Преобразуем полученный объек к типу -ДаунКастинг) 
                    {
                        _statisticsService.SuccessTacts++;//если было испешно выполнение скрипта
                    }
                    else
                    {
                        _statisticsService.ErrorTacts++;
                    }
                    _statisticsService.AllTacts++;
                    _pumpServiceCallback.UpdateStatistics((StatisticsService)_statisticsService);//отправка на клиента статистики
                    Thread.Sleep(1000);
                }
            });

        }
    }
}