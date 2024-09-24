using Amu;
using Amu.MarketPlugIn;
using Amu.PlugInContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DPS.PolishValidationRules
{
    public class ID_Number : MarketPlugInMethodBase
    {
        const string fieldID = "PolishIDNumber";
        const string OK = "OK";
        const string Failed = "Failed";
    
        public ID_Number() : base()
        {
            mMethodDescription = "Verification of the correctness of the Polish ID card number";
            MarketParams.GetOrCreate(fieldID).Description = "ID number field";
            DescribeRevisions.Add("1.0", "Start Revision");

            DescribeMessages.Add(OK, "Given ID number is correct");
            DescribeMessages.Add(Failed, "Given ID number is incorrect");

            //Verification of Polish ID works as following:
            //ID number should looks like this: XXXnnnnnn or XXX nnnnnn, where X - letter, n - digit
            //First 3 characters (letters) have to be converted into numbers, a = 10, b = 11, c = 12, etc. digits remain as digits
            //Every number is multiplied with following digits in that order: 7, 3, 1, 9, 7, 3, 1, 7, 3
            //Sum of products is divided by 10, if remainder is 0 then ID number is correct.
        }

        public override void Execute(IDocRepresentation pIDocRepresentation, IDictionary<string, IDocField> pDictFields, IDefMarketValidation pIDefMarketValidation)
        {
            IDocField lMessageStatusField = null;

            try
            {
                IDocField ID = GetField(pDictFields, fieldID, true);
                lMessageStatusField = GetField(pDictFields, fieldID);
                MsgLogDistrib.Debug("Verification Polish ID started");
                MsgLogDistrib.Debug("Tested ID number: " + ID);

                //Two formats of ID number are allowed
                string pattern = @"^[A-Z]{3}\s?[0-9]{6}$";
                bool IsValidFormattedID = false;
                
                //Conversion of field value (ID number) to text, to lowercase
                string IDValue = (ID.Value?.ToString() ?? "").ToLower();

                //Checking whether input format is correct
                IsValidFormattedID = Regex.IsMatch(IDValue, pattern, RegexOptions.IgnoreCase);
                IDValue = Regex.Replace(IDValue, @"\s", "");

                if (!IsValidFormattedID)
                {
                    MsgLogDistrib.Debug("Input ID number is in wrong format");
                    lMessageStatusField.MessageId = Failed;
                }
                //Verification of ID number length
                else if (IDValue.Length != 9 || String.IsNullOrEmpty(IDValue))
                    {
                        MsgLogDistrib.Debug("ID number has incorrect length. It has to be 9 characters");
                        lMessageStatusField.MessageId = Failed;
                    }

                //Verification that the first three characters of the ID number are letters
                else if (!new Regex("[a-z]").IsMatch(IDValue.Substring(0,3)))
                    {
                        MsgLogDistrib.Debug("First three characters of ID number have to be letters");
                        lMessageStatusField.MessageId = Failed;
                    }

                //Verification that the last 6 characters of the ID number are digits
                else if (!new Regex("[0-9]").IsMatch(IDValue.Substring(3, 6)))
                    {
                        MsgLogDistrib.Debug("Last six characters of ID number have to be digits");
                        lMessageStatusField.MessageId = Failed;
                    }

                //Initial verification successful, proceed to check digit calculation stage
                else
                {
                    //MsgLogDistrib.Debug("Wstępna weryfikacja numeru dowodu osobistego zakończona sukcesem. Następuje obliczenie cyfry kontrolnej");
                    MsgLogDistrib.Debug("Initial verification of the ID number successfully completed. A check digit calculation takes place");
                    var znaki = new List<int>();
                    //Converting letters into digits a = 10, b = 11, c = 12 and writing the converted letters and digits into an array of characters
                    //The lowercase letter a in ASCII is 97, so we subtract 97 and then add 10
                    znaki.Add(IDValue[0] - 97 + 10);
                    znaki.Add(IDValue[1] - 97 + 10);
                    znaki.Add(IDValue[2] - 97 + 10);

                    for (int i = 3; i < IDValue.Length; i++)
                    {
                        znaki.Add(int.Parse(IDValue.Substring(i, 1)));
                    }

                    //Creation of a list of weights - multipliers
                    int suma = 0;
                    var mnozniki = new List<int>() { 7, 3, 1, 9, 7, 3, 1, 7, 3 };

                    //Creation of the sum of products of ID numbers and multipliers
                    for (int i = 0; i < znaki.Count; i++)
                    {
                        suma += znaki[i] * mnozniki[i];
                    }
                    //MsgLogDistrib.Debug("Suma iloczynów wynosi: " + suma.ToString());
                    MsgLogDistrib.Debug("The sum of products is: " + suma.ToString());

                    int cyfra_kontrolna = suma % 10;
                    //MsgLogDistrib.Debug("Reszta z dzielenia sumy iloczynów i liczby 10 wynosi: " + cyfra_kontrolna.ToString());
                    MsgLogDistrib.Debug("The remainder of dividing the sum of the products and the number 10 is: " + cyfra_kontrolna.ToString());

                    if (cyfra_kontrolna == 0)
                    {
                        MsgLogDistrib.Debug("Given ID number is correct");
                        lMessageStatusField.MessageId = OK;
                    }
                    else
                    {
                        MsgLogDistrib.Debug("Given ID number is incorrect");
                        lMessageStatusField.MessageId = Failed;
                    }
                }
            }

            catch (Exception pEx)
            {
                if (lMessageStatusField != null)
                    lMessageStatusField.MessageId = Failed;
                throw new Exception("ID number verification failed.", pEx);
            }
        }

    }
}
