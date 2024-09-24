using Amu;
using Amu.MarketPlugIn;
using Amu.PlugInContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DPS.PolishValidationRules
{
    public class PassportNumber : MarketPlugInMethodBase
    {
        const string fieldID = "PassportNumber";
        const string OK = "OK";
        const string Failed = "Failed";

        public PassportNumber() : base()
        {
            mMethodDescription = "Verification of corectness of Polish passport number";
            MarketParams.GetOrCreate(fieldID).Description = "Polish passport number field value";
            DescribeRevisions.Add("1.0", "Start Revision");

            // Add return message values description
            DescribeMessages.Add(OK, "Given Polish passport number is correct!");
            DescribeMessages.Add(Failed, "Given Polish passport number is incorrect!");

            //Verification of a Polish passport number
            //Analogous to verification of ID number
            //Passport number consists of 9 characters (2 letters + 7 digits)
            //Letters take the values from 10 to 35 (A-Z), A = 10, B = 11, C = 12, etc.
            //Digits retain their values
            //The weights for the sum of the products are 7, 3, 9, 1, 7, 3, 1, 7, 3
            //If the remainder of dividing the sum of products and the number 10 is 0, then the number is correct
        }

        public override void Execute(IDocRepresentation pIDocRepresentation, IDictionary<string, IDocField> pDictFields, IDefMarketValidation pIDefMarketValidation)
        {
            //throw new NotImplementedException();

            IDocField lMessageStatusField = null;

            try
            {
                IDocField P = GetField(pDictFields, fieldID, true);
                lMessageStatusField = GetField(pDictFields, fieldID);
                MsgLogDistrib.Debug("I am initiating verification of the Polish passport number");
                string PValue = (P.Value?.ToString() ?? "").ToUpper();
                MsgLogDistrib.Debug("The tested Polish passport number is: " + PValue);

                if (PValue.Length != 9 || String.IsNullOrEmpty(PValue))
                {
                    MsgLogDistrib.Debug("Passport number has incorrect length. It has to be 9 characters");
                    lMessageStatusField.MessageId = Failed;
                }
                else if (!new Regex("[A-Z]{2}\\d{7}").IsMatch(PValue))
                {
                    MsgLogDistrib.Debug("Passport number has incorrect format. It has to be 2 letter and 7 digits");
                    lMessageStatusField.MessageId = Failed;
                }
                else
                {
                    MsgLogDistrib.Debug("The passport number has the correct length and consists of the correct characters. I am initiating substantive verification");

                    var p_chars = new List<int>();
                    var multip = new List<int>() { 7, 3, 9, 1, 7, 3, 1, 7, 3 };
                    int suma = 0;

                    p_chars.Add(PValue[0] - 65 + 10);
                    p_chars.Add(PValue[1] - 65 + 10);

                    for (int i = 2; i < PValue.Length; i++)
                    {
                        p_chars.Add(int.Parse(PValue.Substring(i, 1)));
                    }

                    for (int i = 0; i < p_chars.Count; i++)
                    {
                        suma += p_chars[i] * multip[i];
                    }

                    MsgLogDistrib.Debug("The sum of the products of the passport is: " + suma.ToString());

                    if (suma % 10 == 0)
                    {
                        MsgLogDistrib.Debug("The passport number is correct");
                        lMessageStatusField.MessageId = OK;
                    }
                    else
                    {
                        MsgLogDistrib.Debug("The passport number is incorrect");
                        lMessageStatusField.MessageId = Failed;
                    }
                }
            }
            catch (Exception pEx)
            {
                if (lMessageStatusField != null)
                    lMessageStatusField.MessageId = Failed;
                throw new Exception("Passport number verification failed", pEx);
            }
        }
    }
}
