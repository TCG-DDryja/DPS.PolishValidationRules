using Amu.MarketPlugIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPS.PolishValidationRules
{
    //Implement market validation methods for Polish values.

    public class PolishValidationRules : MarketPlugInBase
    {
        public PolishValidationRules() : base()
        {
            AddMarketMethod("CheckPolishID", new ID_Number());
            AddMarketMethod("CheckPESEL", new PESEL());
            AddMarketMethod("CheckNIP", new NIP());
            AddMarketMethod("CheckPolishBAN", new NRB());
            AddMarketMethod("CheckREGON", new REGON());
            AddMarketMethod("CheckPolishPassport", new PassportNumber());
        }
    }
}
