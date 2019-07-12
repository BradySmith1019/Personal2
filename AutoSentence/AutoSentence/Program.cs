using System;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;

namespace AutoSentence
{
    class DictionaryBuilder
    {
        static void Main(string[] args)
        {
            private static string DictionaryDB = ConfigurationManager.ConnectionStrings["DictionaryDB"].ConnectionString;

            string input;

            while ((input = Console.ReadLine()) != null)
            {
                if (Int32.TryParse(input, out int numLetters))
                {
                    string word = RandomWord(numLetters);

                    Console.WriteLine(word);
                }

                else
                {
                    string[] yesNo = input.Split();

                    if (yesNo[0].ToLower().Equals("y"))
                    {

                    }
                }
               
            }
        }

        private static string RandomWord(int length)
        {
            StringBuilder s = new StringBuilder();

            Random r = new Random();

            for (int i = 0; i < length; i++)
            {

                int charNum = r.Next(97, 123);

                char letter = (char)charNum;

                s.Append(letter);
            }

            return s.ToString();
        }
    }
}
