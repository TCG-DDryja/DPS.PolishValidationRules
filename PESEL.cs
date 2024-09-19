using Amu;
using Amu.MarketPlugIn;
using Amu.PlugInContext;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPS.PolishValidationRules
{
    public class PESEL : MarketPlugInMethodBase
    {
        const string fieldID = "PESEL";
        const string OK = "OK";
        const string Failed = "Failed";

        public PESEL() : base()
        {
            mMethodDescription = "Verification of the correctness of the PESEL number";
            MarketParams.GetOrCreate(fieldID).Description = "";
            DescribeRevisions.Add("1.0", "Start Revision");

            // Add return message values description
            DescribeMessages.Add(OK, "Given PESEL is correct!");
            DescribeMessages.Add(Failed, "Given PESEL is incorrect!");

            // Verification of the Polish personal identification number (PESEL)
            // PESEL number consists of 11 digits, is unique and uniquely identifies an individual
            // Digits [1-6] are the date of birth in the format YYMMDD
            // Digits [7-10] denote the series number with gender indication, digit [10] denotes the gender, female - even, male - odd
            // Digit [11] is a check digit
            // For people born between 1800 and 1899, add 80 to the month
            // For people born between 1900 and 1999, 0 is added to the month
            // For people born between 2000 and 2099, 20 is added to the month
            // For persons born between 2100 - 2199, 40 shall be added to the month
            // For persons born between 2200 and 2299, 60 shall be added to the month
        }

        public override void Execute(IDocRepresentation pIDocRepresentation, IDictionary<string, IDocField> pDictFields, IDefMarketValidation pIDefMarketValidation)
        {
            //throw new NotImplementedException();

            IDocField lMessageStatusField = null;

            try
            {
                IDocField PESEL = GetField(pDictFields, fieldID, true);
                lMessageStatusField = GetField(pDictFields, fieldID);
                MsgLogDistrib.Debug("PESEL verification started");
                string peselValue = PESEL.Value?.ToString() ?? "";
                MsgLogDistrib.Debug("Tested PESEL is: '" + peselValue + ";");

                if (peselValue.Length != 11 || String.IsNullOrEmpty(peselValue))
                {
                    MsgLogDistrib.Debug("The PESEL has an incorrect length. It should have 11 characters");
                    lMessageStatusField.MessageId = Failed;
                }
                else if (!new Regex("\\d{11}").IsMatch(peselValue))
                {
                    MsgLogDistrib.Debug("The PESEL has an incorrect value. It should have 11 digits");
                    lMessageStatusField.MessageId = Failed;
                }
                else
                {
                    MsgLogDistrib.Debug("The PESEL is of the correct length and consists of digits. I am initiating substantive verification");

                    var pesel_digits = new List<int>();
                    var m_digits = new List<int>() { 1, 3, 7, 9, 1, 3, 7, 9, 1, 3};
                    int suma = 0;
                    int m = 0;
                    int pesel_lastD = int.Parse(peselValue.Substring(peselValue.Length - 1, 1));
                    int checkD = 0;

                    for (int i = 0; i < peselValue.Length - 1; i ++)
                    {
                        pesel_digits.Add(int.Parse(peselValue.Substring(i, 1)));
                    }

                    for (int i = 0; i < pesel_digits.Count; i++)
                    {
                        suma += pesel_digits[i] * m_digits[i];
                    }

                    MsgLogDistrib.Debug("The sum of the products of the PESEL is: " + suma.ToString());
                    m = suma % 10;
                    if (m == 0)
                    {
                        checkD = 0;
                    }
                    else
                    {
                        checkD = 10 - m;
                    }

                    MsgLogDistrib.Debug("The number M (sum modulo 10) is: " + m.ToString() + ", and the calculated check digit is: " + checkD.ToString());
                    if (pesel_lastD == checkD)
                    {
                        MsgLogDistrib.Debug("PESEL is correct");
                        lMessageStatusField.MessageId = OK;
                    }
                    else
                    {
                        MsgLogDistrib.Debug("PESEL is incorrect");
                        lMessageStatusField.MessageId = Failed;
                    }
                }
            }

            catch (Exception pEx)
            {
                if (lMessageStatusField != null)
                    lMessageStatusField.MessageId = Failed;
                throw new Exception("PESEL verification failed", pEx);
            }
        }
    }
}
