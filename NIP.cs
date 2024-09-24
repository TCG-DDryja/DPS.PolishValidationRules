using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Amu.MarketPlugIn;
using Amu.PlugInContext;
using Amu;

namespace DPS.PolishValidationRules
{
    public class NIP : MarketPlugInMethodBase
    {
        const string fieldID = "NIP";
        const string OK = "OK";
        const string Failed = "Failed";

        public NIP() : base()
        {
            mMethodDescription = "Verification of the correctness of the Polish VAT ID (NIP)";
            MarketParams.GetOrCreate(fieldID).Description = "NIP Number field value";
            DescribeRevisions.Add("1.0", "Start Revision");

            // Add return message values description
            DescribeMessages.Add(OK, "Given NIP Number is correct!");
            DescribeMessages.Add(Failed, "Given NIP Number is incorrect!");

            // Verification of the NIP number
            // NIP consists of 10 digits but may be written in different ways
            // Optionally there may be spaces instead of hyphens, the NIP may sometimes be prefixed with PL
            // The last, tenth digit of the NIP is a check digit
            // To calculate the check-digit multiply each digit of the NIP in turn by the weights 6, 5, 7, 2, 3, 4, 5, 6, 7
            // Then add up the above products and calculate the remainder of dividing by 11
            // Received result is a check digit
            // Check digit can't be 10 (NIP is built in that way that this is impossible)
        }

        public override void Execute(IDocRepresentation pIDocRepresentation, IDictionary<string, IDocField> pDictFields, IDefMarketValidation pIDefMarketValidation)
        {
            //throw new NotImplementedException();

            IDocField lMessageStatusField = null;

            try
            {
                IDocField NIP = GetField(pDictFields, fieldID, true);
                lMessageStatusField = GetField(pDictFields, fieldID);
                MsgLogDistrib.Debug("I am initiating verification of the NIP");

                string NIPValue = (NIP.Value?.ToString() ?? "").ToUpper();
                //Allowed patterns - 10 digits, optionally grouped with "-" and optionally with "PL" prefix
                string pattern = @"^\d{10}$|^PL\d{10}$|^\d{3}-\d{3}-\d{2}-\d{2}$|^PL\d{3}-\d{3}-\d{2}-\d{2}$|^\d{3} \d{3} \d{2} \d{2}$|^PL\d{3} \d{3} \d{2} \d{2}$";
                bool IsValidFormattedNIP = false;
                MsgLogDistrib.Debug("Tested NIP is: " + NIPValue);

                IsValidFormattedNIP = Regex.IsMatch(NIPValue, pattern, RegexOptions.IgnoreCase);

                NIPValue = Regex.Replace(NIPValue, "^PL|-","",RegexOptions.IgnoreCase);
                MsgLogDistrib.Debug("Testowany numer NIP po oczyszczeniu to: " + NIPValue);
                
                if (!IsValidFormattedNIP)
                {
                    MsgLogDistrib.Debug("The input NIP has an incorrect format");
                    lMessageStatusField.MessageId = Failed;
                }
                else if (NIPValue.Length != 10 || String.IsNullOrEmpty(NIPValue))
                {
                    MsgLogDistrib.Debug("The NIP has an incorrect length. It should have 10 characters");
                    lMessageStatusField.MessageId = Failed;
                }
                else if (!new Regex("\\d{10}").IsMatch(NIPValue))
                {
                    MsgLogDistrib.Debug("The NIP has an incorrect value. It should have 10 digits");
                    lMessageStatusField.MessageId = Failed;
                }
                else
                {
                    MsgLogDistrib.Debug("The NIP is of the correct length and consists of digits. I am initiating substantive verification");

                    var nip_digits = new List<int>();
                    var m_digits = new List<int>() { 6, 5, 7, 2, 3, 4, 5, 6, 7 };
                    int suma = 0;
                    int nip_lastD = int.Parse(NIPValue.Substring(NIPValue.Length -1, 1));
                    int checkD = 0;

                    for (int i = 0; i < NIPValue.Length - 1; i++)
                    {
                        nip_digits.Add(int.Parse(NIPValue.Substring(i, 1)));
                    }

                    for (int i = 0; i < nip_digits.Count; i++)
                    {
                        suma += nip_digits[i] * m_digits[i];
                    }

                    MsgLogDistrib.Debug("The sum of the products of the NIP is: " + suma.ToString());

                    checkD = suma % 11;
                    MsgLogDistrib.Debug("The calculated check digit is: " + checkD.ToString());

                    if (checkD == nip_lastD)
                    {
                        MsgLogDistrib.Debug("NIP is correct");
                        lMessageStatusField.MessageId = OK;
                    }
                    else
                    {
                        MsgLogDistrib.Debug("NIP is incorrect");
                        lMessageStatusField.MessageId = Failed;
                    }
                }

            }
            catch (Exception pEx)
            {
                if (lMessageStatusField != null)
                    lMessageStatusField.MessageId = Failed;
                throw new Exception("NIP verification failed", pEx);
            }
        }
    }
}
