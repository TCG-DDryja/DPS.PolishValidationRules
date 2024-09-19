using Amu;
using Amu.MarketPlugIn;
using Amu.PlugInContext;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Numerics;

namespace DPS.PolishValidationRules
{
    public class NRB : MarketPlugInMethodBase
    {
        const string fieldID = "NRB";
        const string OK = "OK";
        const string Failed = "Failed";

        public NRB() : base()
        {
            mMethodDescription = "Verification of the correctness of Polish Bank Account Number (NRB)";
            MarketParams.GetOrCreate(fieldID).Description = "";
            DescribeRevisions.Add("1.0", "Start Revision");

            // Add return message values description
            DescribeMessages.Add(OK, "Given NRB is correct!");
            DescribeMessages.Add(Failed, "Given NRB is incorrect!");

            // Verification of the Polish Bank Account Number (NRB) works similarly as IBAN verification, but supports input without the PL prefix in the bank account number
            // This validation rule also support common notation for a bank account number in Polish documents
        }

        public override void Execute(IDocRepresentation pIDocRepresentation, IDictionary<string, IDocField> pDictFields, IDefMarketValidation pIDefMarketValidation)
        {
            //throw new NotImplementedException();

            IDocField lMessageStatusField = null;

            try
            {
                IDocField NRB = GetField(pDictFields, fieldID, true);
                lMessageStatusField = GetField(pDictFields, fieldID);
                MsgLogDistrib.Debug("I am initiating verification of the NRB number");

                string NRBValue = (NRB.Value?.ToString() ?? "").ToUpper();
                string pattern = @"^(PL\d{2} \d{4} \d{4} \d{4} \d{4} \d{4} \d{4})$|^(\d{2} \d{4} \d{4} \d{4} \d{4} \d{4} \d{4})$|^PL\d{26}$|^\d{26}$";
                bool IsValidFormattedNRB = false;

                MsgLogDistrib.Debug("Tested NRB is: " + NRBValue);
                IsValidFormattedNRB = Regex.IsMatch(NRBValue, pattern);
                NRBValue = Regex.Replace(NRBValue, "PL|\\s", "");
                NRBValue = "PL" + NRBValue;
                MsgLogDistrib.Debug("The tested NRB number after cleaning (and adding the PL prefix to the NRB) is: " + NRBValue);

                if (!IsValidFormattedNRB)
                {
                    MsgLogDistrib.Debug("The input NRB has an incorrect format");
                    lMessageStatusField.MessageId = Failed;
                }
                else if (NRBValue.Length != 28 || String.IsNullOrEmpty(NRBValue))
                {
                    MsgLogDistrib.Debug("The NRB has an incorrect length. It should have 28 characters (PL and 26 digits)");
                    lMessageStatusField.MessageId = Failed;
                }
                else if (!new Regex("PL\\d{26}").IsMatch(NRBValue))
                {
                    MsgLogDistrib.Debug("The NRB has an incorrect value. It should have the format PL and 26 digits");
                    lMessageStatusField.MessageId = Failed;
                }
                else
                {
                    MsgLogDistrib.Debug("The NRB has the correct length and consists of the correct characters. I am initiating substantive verification");
                    String checkNRB = NRBValue.Substring(4) + NRBValue.Substring(0, 4);
                    checkNRB = Regex.Replace(checkNRB, "PL", "2521");
                    MsgLogDistrib.Debug("I am changing the original NRB for the purpose of calculating correctness (moving the first 4 characters and replacing PL with 2521): " + checkNRB);

                    BigInteger modulo = BigInteger.Parse(checkNRB) % 97;

                    if (modulo == 1)
                    {
                        MsgLogDistrib.Debug("NRB is correct");
                        lMessageStatusField.MessageId = OK;
                    }
                    else
                    {
                        MsgLogDistrib.Debug("NRB is incorrect");
                        lMessageStatusField.MessageId = Failed;
                    }
                }

            }
            catch (Exception pEx)
            {
                if (lMessageStatusField != null)
                    lMessageStatusField.MessageId = Failed;
                throw new Exception("NRB verification failed", pEx);
            }
        }
    }

    


}
