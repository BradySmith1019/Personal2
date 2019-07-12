using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data.SqlClient;
using System.Data;

namespace AutoSentencer
{
    class Program
    {
        private static string DictionaryDB;

        static void Main(string[] args)
        {
            DictionaryDB = ConfigurationManager.ConnectionStrings["DictionaryDB"].ConnectionString;

            string input = Console.ReadLine();

            Int32.TryParse(input, out int numLetters);

            string output = RandomWord(numLetters);

            Console.WriteLine(output);

            while ((input = Console.ReadLine()) != null)
            {
                string[] yesNo = input.Split();

                if (yesNo[0].ToLower().Equals("y"))
                {
                    string type = yesNo[1].ToLower();
                    switch (type)
                    {
                        case "verb":
                            AddVerb(output);
                            break;
                        case "adjective":
                            AddAdjective(output);
                            break;
                        case "noun":
                            AddNoun(output);
                            break;
                        case "pronoun":
                            AddPronoun(output);
                            break;
                        case "adverb":
                            AddAdverb(output);
                            break;
                        case "preposition":
                            AddPreposition(output);
                            break;
                        case "conjunction":
                            AddConjunction(output);
                            break;
                    }
                    output = RandomWord(numLetters);

                    Console.WriteLine(output);
                }
                else if (yesNo[0].ToLower().Equals("n"))
                {
                    using (SqlConnection conn = new SqlConnection(DictionaryDB))
                    {
                        conn.Open();

                        using (SqlTransaction trans = conn.BeginTransaction())
                        {
                            using (SqlCommand command = new SqlCommand("insert into Words (Nonwords) values(@Nonwords)", conn, trans))
                            {
                                command.Parameters.AddWithValue("@Nonwords", output);

                                if (command.ExecuteNonQuery() != 1)
                                {
                                    throw new Exception("Query Failed Unexpectedly");
                                }

                            }

                            trans.Commit();
                        }
                    }

                    output = RandomWord(numLetters);

                    Console.WriteLine(output);
                }
                else
                {
                    Console.WriteLine("Invalid Input");
                }
                
            }
        }

        private static void AddVerb(string input)
        {
            using (SqlConnection conn = new SqlConnection(DictionaryDB))
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("insert into Words (Verbs) values(@Verb)", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Verb", input);

                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query Failed Unexpectedly");
                        }
                    }

                    trans.Commit();
                }
            }
        }

        private static void AddAdjective(string input)
        {
            using (SqlConnection conn = new SqlConnection(DictionaryDB))
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("insert into Words (Adjectives) values(@Adjective)", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Adjective", input);

                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query Failed Unexpectedly");
                        }
                    }

                    trans.Commit();
                }
            }
        }

        private static void AddNoun(string input)
        {
            using (SqlConnection conn = new SqlConnection(DictionaryDB))
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("insert into Words (Nouns) values(@Noun)", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Noun", input);

                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query Failed Unexpectedly");
                        }
                    }

                    trans.Commit();
                }
            }
        }

        private static void AddPronoun(string input)
        {
            using (SqlConnection conn = new SqlConnection(DictionaryDB))
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("insert into Words (Pronouns) values(@Pronoun)", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Pronoun", input);

                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query Failed Unexpectedly");
                        }
                    }

                    trans.Commit();
                }
            }
        }

        private static void AddAdverb(string input)
        {
            using (SqlConnection conn = new SqlConnection(DictionaryDB))
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("insert into Words (Adverbs) values(@Adverb)", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Adverb", input);

                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query Failed Unexpectedly");
                        }
                    }

                    trans.Commit();
                }
            }
        }

        private static void AddPreposition(string input)
        {
            using (SqlConnection conn = new SqlConnection(DictionaryDB))
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("insert into Words (Prepositions) values(@Preposition)", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Preposition", input);

                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query Failed Unexpectedly");
                        }
                    }

                    trans.Commit();
                }
            }
        }

        private static void AddConjunction(string input)
        {
            using (SqlConnection conn = new SqlConnection(DictionaryDB))
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("insert into Words (Conjunctions) values(@Conjunction)", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Conjunction", input);

                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query Failed Unexpectedly");
                        }
                    }

                    trans.Commit();
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

            if (CheckWord(s.ToString()))
            {
                return s.ToString();
            }
            else
            {
                return RandomWord(length);
            }
        }

        private static bool CheckWord(string word)
        {
            using (SqlConnection conn = new SqlConnection(DictionaryDB))
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select Nonwords from Words where Nonwords = @Nonwords", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Nonwords", word);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                reader.Close();
                                trans.Commit();
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }
    }
}
