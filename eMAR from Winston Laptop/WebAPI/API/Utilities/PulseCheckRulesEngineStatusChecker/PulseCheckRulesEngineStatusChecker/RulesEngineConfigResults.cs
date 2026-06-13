using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseCheckRulesEngineStatusChecker
{
    public class RulesEngineConfigResults
    {
        public bool RestartNotNeeded { get; set; }
        public string ConfigResults { get; set; } = "";
    }
}
