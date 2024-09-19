using Amu;
using Amu.MarketPlugIn;
using Amu.PlugInContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DPS.PolishValidationRules
{
    public class REGON : MarketPlugInMethodBase
    {
        const string fieldID = "REGON";
        const string OK = "OK";
        const string Failed = "Failed";

        public REGON() : base ()
        {
            mMethodDescription = "Verification of correctness of Polish Business Identifier (REGON)";
            MarketParams.GetOrCreate(fieldID).Description = "REGON number field value";
            DescribeRevisions.Add("1.0", "Start Revision");

            // Add return message values description
            DescribeMessages.Add(OK, "Given REGON number is correct!");
            DescribeMessages.Add(Failed, "Given REGON number is incorrect!");

            //REGON number verification
            //REGON is a unique value identifying an entrepreneur in Poland
            //REGON can be a string of 9 or 14 digits in length (14 digits length REGON includes 9 digits REGON)
            //For a 9-digit REGON number, the 9 digit is a check digit calculated by summing the products of the weights 8 9 2 3 4 5 6 7 and modulo 11 (modulo result = check digit)
            //For a 14-digit REGON number, the 14th digit is a check digit calculated by adding up the products of the weights 2 4 8 5 0 9 7 3 6 1 2 4 8 and modulo 11 (modulo result = check digit)
        }

        public override void Execute(IDocRepresentation pIDocRepresentation, IDictionary<string, IDocField> pDictFields, IDefMarketValidation pIDefMarketValidation)
        {
            //throw new NotImplementedException();

            IDocField lMessageStatusField = null;

            try
            {
                IDocField REGON = GetField(pDictFields, fieldID, true);
                lMessageStatusField = GetField(pDictFields, fieldID);
                MsgLogDistrib.Debug("I am initiating REGON verification");
                string REGONValue = REGON.Value?.ToString() ?? "";
                MsgLogDistrib.Debug("Tested REGON is: " + REGONValue);

                if ((REGONValue.Length != 9 && REGONValue.Length != 14) || String.IsNullOrEmpty(REGONValue))
                {
                    MsgLogDistrib.Debug("The REGON has an incorrect length. It should have 9 or 14 characters");
                    lMessageStatusField.MessageId = Failed;
                }
                else if (!new Regex("^\\d{9}$|^\\d{14}$").IsMatch(REGONValue))
                {
                    MsgLogDistrib.Debug("The REGON has an incorrect value. It should have 9 or 14 digits");
                    lMessageStatusField.MessageId = Failed;
                }
                else
                {
                    MsgLogDistrib.Debug("The REGON is of the correct length and consists of digits. I am initiating substantive verification");

                    var regon_digits = new List<int>();
                    int suma = 0;
                    int checkD = 0;
                    int lastD = int.Parse(REGONValue.Substring(REGONValue.Length - 1, 1));

                    for (int i = 0; i < REGONValue.Length - 1; i++)
                    {
                        regon_digits.Add(int.Parse(REGONValue.Substring(i, 1)));
                    }

                    if (REGONValue.Length == 9)
                    {
                        var multip_digits = new List<int>() { 8, 9, 2, 3, 4, 5, 6, 7 };
                        for (int i = 0; i < regon_digits.Count; i++)
                        {
                            suma += regon_digits[i] * multip_digits[i];
                        }
                        checkD = suma % 11;
                    }
                    else if (REGONValue.Length == 14)
                    {
                        var multip_digits = new List<int>() { 2, 4, 8, 5, 0, 9, 7, 3, 6, 1, 2, 4, 8 };
                        for (int i = 0; i < regon_digits.Count; i++)
                        {
                            suma += regon_digits[i] * multip_digits[i];
                        }
                        checkD = suma % 11;
                    }
                    else
                    {
                        //If this code is running, then something went wrong
                        if (lMessageStatusField != null)
                            lMessageStatusField.MessageId = Failed;
                    }

                    MsgLogDistrib.Debug("The sum of the products of REGON is: " + suma.ToString() + ", and the calculated check digit is: " + checkD.ToString());

                    if (checkD == lastD)
                    {
                        MsgLogDistrib.Debug("REGON is correct");
                        lMessageStatusField.MessageId = OK;
                    }
                    else
                    {
                        MsgLogDistrib.Debug("REGON is incorrect");
                        lMessageStatusField.MessageId = Failed;
                    }
                }
            }
            catch (Exception pEx)
            {
                if (lMessageStatusField != null)
                    lMessageStatusField.MessageId = Failed;
                throw new Exception("REGON verification failed", pEx);
            }
        }
    }
}
