using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using log4net;
using PulseCheck.Data.Common.Rest;
using PulseCheck.Data.Common.Utilities;
using PulseCheck.QCPR.Domain.Contract;
using PulseCheck.QCPR.Logic.Bindings;
using PulseCheck.QCPR.Logic.Bindings.Harris.UCW.BLL.Bindings;

namespace PulseCheck.QCPR.Data.Loader.Console
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly AutoFacQcprLogicRegistrations AutoFacQcprLogicRegistrations = new AutoFacQcprLogicRegistrations();


        static void Main(string[] args)
        {
            Logger.Info($"{Assembly.GetExecutingAssembly().GetName().Name} starting");

            try
            {
                Logger.Info($"Register automapper bindings");
                AutoMapperRegistrationSingleton.Register();
                
                Logger.Info($"Loading DI bindings");
                AutoFacQcprLogicRegistrations.LoadContainer();

                var qcprManager = AutoFacQcprLogicRegistrations.GetType<IQcprManager>();

                Logger.Info($"Retrieving Json from QCPR");
                string json = qcprManager.GetQcprJsonFromVendor();

                if (!JsonUtil.ValidateJson(json))
                    throw new FormatException($"Invalid Json: {json}");

                Logger.Info($"Saving Import Data");
                qcprManager.SaveImportData(json);
            }
            catch (Exception e)
            {
                Logger.Error($"Error encountered: {e.Message}", e );
#if DEBUG
                System.Console.ReadLine();
#endif
            }

            Logger.Info($"{Assembly.GetExecutingAssembly().GetName().Name} stopping");
        }
    }
}
