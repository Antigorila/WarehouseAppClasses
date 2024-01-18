using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.IO;
using System.Text.RegularExpressions;

namespace WarehouseAppClasses
{
    abstract class Faker2
    {
        static void ConsoleMessage(string promt, string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(promt + " ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void ConsoleMessageLine(string promt, string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(promt + " ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void ConsoleError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string datasource = "127.0.0.1";
        public static int port = 3306;
        public static string username = "root";
        public static string password = "";
        public static string database = string.Empty;

        public static string connectionstring = $"datasource={datasource};port={port};username={username};password={password};database={database};";
        public static MySqlConnection con = new MySqlConnection(connectionstring);


        public static void SetDatabaseName(string databaseName)
        {
            database = databaseName;
            connectionstring = $"datasource={datasource};port={port};username={username};password={password};database={databaseName};";
        }
        static private void InsertData(string command)
        {
            con.Open();
            string insert = command;
            using (MySqlCommand insertCommand = new MySqlCommand(insert, con))
            {
                insertCommand.ExecuteNonQuery();
            }
            con.Close();
        }
        static public List<string[]> SqlQuery(string query)
        {
            if (!query.Contains($"USE {database};"))
            {
                query = string.Concat($"USE {database};", query);
            }
            List<string[]> results = new List<string[]>();
            con.Open();

            using (MySqlCommand cmd = new MySqlCommand(query, con))
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string[] row = new string[reader.FieldCount];

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[i] = reader[i].ToString();
                    }

                    results.Add(row);
                }
            }
            con.Close();
            return results;
        }
        static public void ShowTables(string promt)
        {
            ConsoleMessageLine(promt, "Tables: ");
            List<string[]> lis = SqlQuery($"SHOW TABLES FROM {database}");
            for (int i = 0; i < lis.Count; i++)
            {
                ConsoleMessageLine(promt, $"{i + 1}. Table name: {lis[i][0]}");
            }
        }
        public static List<string> Tables()
        {
            List<string> returnList = new List<string>();
            List<string[]> lis = SqlQuery($"SHOW TABLES FROM {database}");
            for (int i = 0; i < lis.Count; i++)
            {
                returnList.Add(lis[i][0]);
            }
            return returnList;
        }
        static public void DeleteTable(string promt)
        {
            ConsoleMessageLine(promt, "Tables: ");
            List<string[]> lis = SqlQuery($"SHOW TABLES FROM {database}");
            for (int i = 0; i < lis.Count; i++)
            {
                ConsoleMessageLine(promt, $"{i + 1}. Table name: {lis[i][0]}");
            }
            ConsoleMessage(promt, "Enter the table name: ");
            string tableName = Console.ReadLine();

            con.Open();
            string deleteTableQuery = $"USE {database}; DROP TABLE {tableName};";
            using (MySqlCommand command = new MySqlCommand(deleteTableQuery, con))
            {
                command.ExecuteNonQuery();
            }
            con.Close();
        }

        static public bool ContainsIllegalRegex(string input)
        {
            bool containsIllegalPattern = false;
            string[] illegalPatterns = { @"\s", @"[\W&&[^_]]", @"\b(select|insert|update|delete|table)\b", @"[!@#$%^&*()+=\[\]{};':"",.<>?/\\|~`]", "[áéűó]" };

            foreach (var pattern in illegalPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase) || char.IsDigit(input[0]))
                {
                    containsIllegalPattern = true;
                    ConsoleError("Input contains illegal pattern or is in a bad format");
                    break;
                }
            }

            return containsIllegalPattern;
        }

        static bool BetweenContainsNumber(string input, char startChar, char endChar)
        {
            int startIndex = input.IndexOf(startChar);
            int endIndex = input.IndexOf(endChar);

            if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
            {
                string subString = input.Substring(startIndex + 1, endIndex - startIndex - 1);
                foreach (char c in subString)
                {
                    if (char.IsDigit(c))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static Random random = new Random();
        private static string RandomDate()
        {
            int year = DateTime.Today.AddYears(-random.Next(1, 11)).Year;
            int month = random.Next(1, 13);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int day = random.Next(1, daysInMonth + 1);

            return $"{year:D4}-{month:D2}-{day:D2}";
        }

        private static string RandomDate(int fromYearsAgo, int toYearsAgo)
        {
            int thisYear = int.Parse(DateTime.Now.Year.ToString());
            int year = random.Next(thisYear - fromYearsAgo, thisYear + toYearsAgo);
            int month = random.Next(1, 13);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int day = random.Next(1, daysInMonth + 1);

            return $"{year:D4}-{month:D2}-{day:D2}";
        }

        static private string GetTypes(string[] Datas)
        {
            if (Datas.Length == 1)
            {
                if (Datas[0].ToLower() == "id")
                {
                    return "id int NOT NULL PRIMARY KEY AUTO_INCREMENT,";
                }
                ConsoleError("Invalid form");
                return null;
            }
            else
            {
                string returnString = Datas[0];
                bool FK = false;
                int FKindex = 0;
                for (int i = 1; i < Datas.Length; i++)
                {
                    if (Datas[i].Contains("PK"))
                    {
                        returnString += " PRIMARY KEY";
                    }
                    else if (Datas[i].Contains("FK"))
                    {
                        returnString += " int,";
                        FK = true;
                        FKindex = i;
                    }
                    else if (Datas[i].ToLower().Contains("varchar"))
                    {
                        if (!Datas[i].Contains("(") || !Datas[i].Contains(")"))
                        {
                            ConsoleError("Bad format for varchar");
                            returnString = string.Empty;
                        }
                        else
                        {
                            if (!BetweenContainsNumber(Datas[i], '(', ')'))
                            {
                                ConsoleError("Bad format for varchar");
                                returnString = string.Empty;
                            }
                            else
                            {
                                returnString += " " + Datas[i] + ",";
                            }
                        }
                    }
                    else
                    {
                        returnString += " " + Datas[i] + ",";
                    }
                }
                if (FK)
                {
                    string[] FKdata = Datas[FKindex].Split('-');
                    returnString += $" FOREIGN KEY ({Datas[0]}) REFERENCES {FKdata[1]}({FKdata[2]}),";
                    return returnString;
                }
                else
                {
                    return returnString;
                }
            }
        }

        static public void CreateTable(string promt)
        {
            string tablename = string.Empty;
            while (true)
            {
                ConsoleMessage(promt, "Enter the table name: ");
                tablename = Console.ReadLine();
                if (tablename == "--help")
                {
                    ConsoleMessageLine(promt, "Can't contain space, special chars(except underscore) and SQL commands(SELECT, INSERT, UPDATE, DELETE) and cant start with a number but can contains number");
                }
                else if (Tables().Contains(tablename))
                {
                    ConsoleError("Table name already exist");
                }
                else if (!ContainsIllegalRegex(tablename))
                {
                    ConsoleMessageLine(promt, "Table name approved");
                    break;
                }
            }

            List<string> tables = new List<string>();
            ConsoleMessageLine(promt, "Enter the colums names and it's types. Enter --type to see the types or --rule to see the rules or --help to get help.");
            while (true)
            {
                ConsoleMessage(promt, "Column name: ");
                string input = Console.ReadLine();
                if (input == "")
                {
                    break;
                }
                else if (input == "--type")
                {
                    List<string> Types = new List<string>
                        {
                            "CHAR(size)",
                            "VARCHAR(size)",
                            "TEXT(size)",
                            "LONGTEXT",
                            "TINYINT(size)",
                            "BOOL",
                            "BOOLEAN",
                            "INT(size)",
                            "INTEGER(size)",
                            "BIGINT(size)",
                            "FLOAT(size, d)",
                            "FLOAT(p)",
                            "DOUBLE(size, d)",
                            "DATE",
                            "DATETIME(YYYY-MM-DD hh:mm:ss)",
                            "TIMESTAMP(YYYY-MM-DD hh:mm:ss)",
                        };
                    for (int i = 0; i < Types.Count; i++)
                    {
                        ConsoleMessageLine(promt, Types[i]);
                    }
                }
                else if (input == "--rule")
                {
                    ConsoleMessageLine(promt, "Can't contain space, special chars(except underscore) and SQL commands(SELECT, INSERT, UPDATE, DELETE) and cant start with a number but can contains number, if you want to make an id just write id and the program will make it: primary key, auto incremetn, not null");
                }
                else if (input == "--help")
                {
                    ConsoleMessageLine(promt, "test int(11)");
                    ConsoleMessageLine(promt, "test1 varchar(255) UNIQUE");
                    ConsoleMessageLine(promt, "test2 int(11) NULL");
                    ConsoleMessageLine(promt, "test3 int(11) PK");
                    ConsoleMessageLine(promt, "test4 FK-table2-id");
                    ConsoleMessageLine(promt, "test5 int(11) DEFAULT(1)");
                    ConsoleMessageLine(promt, "test6 varchar DEFAULT('example')");
                }
                else if (tables.Contains(input.Split(' ')[0]))
                {
                    ConsoleError("You already have a column named like this");
                }
                else if (!ContainsIllegalRegex(input.Split(' ')[0]))
                {
                    string[] Datas = input.Split(' ');
                    tables.Add(GetTypes(Datas));
                }
            }

            string CreateTableCommand = $"USE {database}; CREATE TABLE {tablename} (";
            for (int i = 0; i < tables.Count; i++)
            {
                CreateTableCommand += tables[i];
            }
            if (CreateTableCommand[CreateTableCommand.Length - 1] == ',')
            {
                CreateTableCommand = CreateTableCommand.Remove(CreateTableCommand.Length - 1);
            }
            CreateTableCommand += ");";

            con.Open();
            using (MySqlCommand command = new MySqlCommand(CreateTableCommand, con))
            {
                command.ExecuteNonQuery();
            }
            con.Close();
            ConsoleMessageLine(promt, "Table created");
        }
        private static List<string> ListFromTxT(string txtName)
        {
            if (!txtName.Contains(".txt"))
            {
                txtName = txtName + ".txt";
            }
            StreamReader olvas = new StreamReader(txtName);
            List<string> DatasList = new List<string>();
            while (!olvas.EndOfStream)
            {
                string egysor = olvas.ReadLine();
                if (egysor != null)
                {
                    DatasList.Add(egysor);
                }
            }
            olvas.Close();
            return DatasList;
        }

        private static Dictionary<string, string> ColumnsAndTypesOfTable(string tablename)
        {
            Dictionary<string, string> ColumnsAndTypes = new Dictionary<string, string>();
            List<string[]> lis = SqlQuery($"USE {database}; SHOW COLUMNS FROM {tablename};");
            for (int i = 0; i < lis.Count; i++)
            {
                ColumnsAndTypes.Add(lis[i][0], lis[i][1]);
            }

            return ColumnsAndTypes;
        }
        public static string PeelingFromBracket(string input)
        {
            int startIndex = input.IndexOf('(');
            int endIndex = input.IndexOf(')');
            if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
            {
                string extractedText = input.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
                return extractedText;
            }
            return null;
        }

        static public void FakerTable(string promt)
        {
            List<string> TableList = Tables();
            string tableName = string.Empty;
            for (int i = 0; i < TableList.Count; i++)
            {
                ConsoleMessageLine(promt, TableList[i]);
            }
            while (true)
            {
                ConsoleMessage(promt, "Enter the name of the table you want to fill with Dummy Datas: ");
                string input = Console.ReadLine();
                if (TableList.Contains(input))
                {
                    ConsoleMessageLine(promt, "Table name approved");
                    tableName = input;
                    break;
                }
                else
                {
                    ConsoleError("You might missed the name?");
                }
            }
            ConsoleMessageLine(promt, "Columns: ");
            Dictionary<string, string> ColumnsAndTypes = ColumnsAndTypesOfTable(tableName);
            foreach (var column in ColumnsAndTypes)
            {
                ConsoleMessageLine(promt, $"Column name: {column.Key} - Column type: {column.Value}");
            }

            List<string> DummyTypes = ListFromTxT("DummyDatas.txt");
            Dictionary<string, string> ColumnsWithDummy = new Dictionary<string, string>();
            ConsoleMessageLine(promt, "Enter the Column the enter what kind of Dummy data you want to fill it, press enter when you done");


            while (true)
            {
                string columnName = string.Empty;
                ConsoleMessage(promt, "Enter the column name: ");
                columnName = Console.ReadLine();
                if (ColumnsAndTypes.ContainsKey(columnName))
                {
                    while (true)
                    {
                        ConsoleMessage(promt, "Enter the column Dummy type: ");
                        string DummyDataType = Console.ReadLine();
                        if (DummyDataType == "--help")
                        {
                            ConsoleMessageLine(promt, "You can create random dates with: 'date' or 'date(10-0)'. The first one just generate a random date between this year minus 10(so its in the recent past). But you can specify how much to subtract and add for this year for example: date(70-0) this will generate dates in the past 70 yers until now.");
                            ConsoleMessageLine(promt, "You can create random numbers with: 'number' or 'number(10-0)'. The first one just generate a random number between 0 and 10. But you can also specify that it should create a random number between what and what.");
                            ConsoleMessageLine(promt, "You can create a true or false data(0 or 1) with 'TrueOrFalse'. 0 is true 1 is false");
                            ConsoleMessageLine(promt, "You can fake a Foregin Key with: 'FK-othertable-foreginkey' For example: 'FK-workTable-ID'");
                            ConsoleMessageLine(promt, "Here is the Dummy Data Types: ");
                            for (int i = 0; i < DummyTypes.Count; i++)
                            {
                                ConsoleMessageLine(promt, $"\t{DummyTypes[i]}");
                            }
                        }
                        else if (DummyDataType.ToLower().Contains("date"))
                        {
                            if (BetweenContainsNumber(DummyDataType, '(', ')'))
                            {
                                ColumnsWithDummy.Add(columnName, $"date:{PeelingFromBracket(DummyDataType)}");
                                break;
                            }
                            else
                            {
                                ColumnsWithDummy.Add(columnName, "date");
                                break;
                            }
                        }
                        else if (DummyDataType.ToLower().Contains("number"))
                        {
                            if (BetweenContainsNumber(DummyDataType, '(', ')'))
                            {
                                ColumnsWithDummy.Add(columnName, $"number:{PeelingFromBracket(DummyDataType)}");
                                break;
                            }
                            else
                            {
                                ColumnsWithDummy.Add(columnName, "number");
                                break;
                            }
                        }
                        else if (DummyDataType.ToLower().Contains("trueorfalse"))
                        {
                            ColumnsWithDummy.Add(columnName, "TrueOrFalse");
                            break;
                        }
                        else if (DummyDataType.ToLower().Contains("fk"))
                        {
                            try
                            {
                                string[] datas = DummyDataType.Split('-');
                                if (SqlQuery($"SELECT {datas[2]} FROM {datas[1]}").Count >= 1)
                                {
                                    ColumnsWithDummy.Add(columnName, $"FK-{datas[1]} - {datas[2]}");
                                }
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            break;
                        }
                        else if (DummyTypes.Contains(DummyDataType))
                        {
                            ColumnsWithDummy.Add(columnName, DummyDataType);
                            break;
                        }
                        else if (DummyDataType == "" || DummyDataType == " ")
                        {
                            ConsoleError("You must enter the Dummy type then you can finish");
                        }
                        else
                        {
                            ConsoleError("You might missed something?");
                        }
                    }
                }
                else if (columnName == "" || columnName == " ")
                {
                    break;
                }
                else if (columnName == "--help")
                {
                    ConsoleMessageLine(promt, "Just enter a column name, then press enter then you can choose a Dummy Data to fill it");
                }
                else
                {
                    ConsoleError("You might missed the name?");
                    columnName = string.Empty;
                }
            }

            ConsoleMessage(promt, $"Enter how many specimen you want to make: ");
            int count = int.Parse(Console.ReadLine());
            try
            {
                Random rng = new Random();
                for (int i = 0; i < count; i++)
                {
                    string insertData = $"USE {database}; INSERT INTO {tableName} (";
                    string values = " VALUES (";
                    foreach (var data in ColumnsWithDummy)
                    {
                        if (data.Value.Contains("date"))
                        {
                            if (data.Value.Contains(":"))
                            {
                                insertData += data.Key + ",";
                                string[] dates = data.Value.Split(':');
                                int from = int.Parse(dates[1].Split('-')[0]);
                                int to = int.Parse(dates[1].Split('-')[1]);
                                values += $"'{RandomDate(from, to)}',";
                            }
                            else
                            {
                                insertData += data.Key + ",";
                                values += $"'{RandomDate()}',";
                            }
                        }
                        else if (data.Value.Contains("number"))
                        {
                            if (data.Value.Contains(":"))
                            {
                                insertData += data.Key + ",";
                                string[] dates = data.Value.Split(':');
                                int from = int.Parse(dates[1].Split('-')[0]);
                                int to = int.Parse(dates[1].Split('-')[1]);

                                values += $"'{rng.Next(from, to)}',";
                            }
                            else
                            {
                                insertData += data.Key + ",";
                                values += $"'{rng.Next(1, 11)}',";
                            }
                        }
                        else if (data.Value.Contains("TrueOrFalse"))
                        {
                            insertData += data.Key + ",";
                            values += $"'{rng.Next(0, 2)}',";
                        }
                        else if (data.Value.Contains("FK"))
                        {
                            insertData += data.Key + ",";
                            string[] datas = data.Value.Split('-');

                            List<string[]> ForeginKeys = SqlQuery($"SELECT {datas[2]} FROM {datas[1]}");
                            values += $"{ForeginKeys[rng.Next(0, ForeginKeys.Count())][0]},";
                        }
                        else
                        {
                            insertData += data.Key + ",";
                            List<string> Dummys = ListFromTxT(data.Value);
                            values += $"'{Dummys[rng.Next(0, Dummys.Count)]}',";
                        }
                    }

                    if (insertData[insertData.Length - 1] == ',')
                    {
                        insertData = insertData.Remove(insertData.Length - 1);
                    }

                    if (values[values.Length - 1] == ',')
                    {
                        values = values.Remove(values.Length - 1);
                    }

                    insertData += ")";
                    values += ")";

                    InsertData(insertData + values);
                }
                ConsoleMessageLine(promt, "The table and the specified columns were uploaded successfully with Dummy datas");
            }
            catch (Exception)
            {
                throw;
            }
        }
        static private bool IllegalDummyData(string DummyDataName)
        {
            List<string> DummyDatasList = ListFromTxT("DummyDatas.txt");

            bool IllegalName = false;
            for (int i = 0; i < DummyDatasList.Count; i++)
            {
                string currentName = DummyDatasList[i];
                if (currentName.Length >= 4 && currentName.Substring(currentName.Length - 4) == ".txt")
                {
                    currentName = currentName.Substring(0, currentName.Length - 4);
                    if (currentName == DummyDataName)
                    {
                        IllegalName = true;
                        break;
                    }
                }
            }
            return IllegalName;
        }
        static public void DummyTypes(string promt)
        {
            List<string> DummyTypes = ListFromTxT("DummyDatas.txt");
            for (int i = 0; i < DummyTypes.Count(); i++)
            {
                ConsoleMessageLine(promt, DummyTypes[i]);
            }

            while (true)
            {
                ConsoleMessage(promt, "Enter Dummy Data type names to see what they contains or just press enter to break: ");
                string input = Console.ReadLine();
                if (input == "" || input == " ")
                {
                    ConsoleMessageLine(promt, "Abort");
                    break;
                }
                else if (DummyTypes.Contains(input))
                {
                    List<string> DummyTypesDatas = ListFromTxT(input + ".txt");
                    for (int i = 0; i < DummyTypesDatas.Count; i++)
                    {
                        ConsoleMessageLine(promt, DummyTypesDatas[i]);
                    }
                }
                else
                {
                    ConsoleError("You might missed the name?");
                }
            }
        }
        static public void CreateNewDummyDataTypes(string promt)
        {
            List<string> DummyDatasList = ListFromTxT("DummyDatas.txt");

            string newDummyDataName = string.Empty;
            while (true)
            {
                ConsoleMessage(promt, "Enter the name of the Dummy Data list you want to create: ");
                newDummyDataName = Console.ReadLine();
                if (IllegalDummyData(newDummyDataName))
                {
                    ConsoleError("This name or data type already exist.");
                    newDummyDataName = string.Empty;
                }
                else
                {
                    break;
                }
            }

            if (newDummyDataName.Contains(".txt"))
            {
                newDummyDataName = newDummyDataName.Remove(newDummyDataName.Length - 4, 4);
            }
            List<string> newDummyDatas = new List<string>();

            ConsoleMessageLine(promt, "Just enter the datas as a text and then press enter. If you finished just press enter. If you need any help just enter --help. If you want to check the alrady existing Dummy Data types just enter: --types");
            while (true)
            {
                ConsoleMessage(promt, "Enter the Dummy data text: ");
                string input = Console.ReadLine();
                if (input == "")
                {
                    break;
                }
                else if (input == "--types")
                {
                    for (int i = 0; i < DummyDatasList.Count; i++)
                    {
                        ConsoleMessageLine(promt, DummyDatasList[i]);
                    }
                }
                else if (input == "--help")
                {
                    ConsoleMessageLine(promt, "Accordng to the name of the datas just enter the datas. For example if your Dummy Data name is cars then just enter car names like: Ford, Mercedes, Toyota. When you finished just press enter.");
                }
                else
                {
                    newDummyDatas.Add(input);
                }
            }

            StreamWriter ir = new StreamWriter(newDummyDataName + ".txt");
            for (int i = 0; i < newDummyDatas.Count; i++)
            {
                ir.WriteLine(newDummyDatas[i]);
            }
            ir.Close();

            StreamWriter writeDummyLog = new StreamWriter("DummyDatas.txt");
            for (int i = 0; i < DummyDatasList.Count; i++)
            {
                writeDummyLog.WriteLine(DummyDatasList[i]);
            }
            writeDummyLog.WriteLine(newDummyDataName);
            writeDummyLog.Close();
        }

        static public void ExpandDummyData(string promt)
        {
            ConsoleMessageLine(promt, "Choose a Dummy data type you want to expand:");
            List<string> DummyDatasList = ListFromTxT("DummyDatas.txt");
            List<string> ModifyDummyDatasItems = new List<string>();
            string ModifyDummyData = string.Empty;
            for (int i = 0; i < DummyDatasList.Count; i++)
            {
                ConsoleMessageLine(promt, DummyDatasList[i]);
            }

            while (true)
            {
                ConsoleMessage(promt, "Enter the Dummy data type name: ");
                ModifyDummyData = Console.ReadLine();
                if (!DummyDatasList.Contains(ModifyDummyData))
                {
                    ConsoleError("Invalid type");
                }
                else
                {
                    ModifyDummyDatasItems = ListFromTxT(ModifyDummyData + ".txt");
                    ConsoleMessageLine(promt, "Type name accepted");
                    break;
                }
            }

            ConsoleMessage(promt, "You want to modify the items inside the library or add new ones?(modify, add)? ");
            string mORa = Console.ReadLine();
            if (mORa.ToLower().Contains("modify"))
            {
                for (int i = 0; i < ModifyDummyDatasItems.Count; i++)
                {
                    ConsoleMessageLine(promt, $"{i + 1} {ModifyDummyDatasItems[i]}");
                }
                ConsoleMessage(promt, "Enter the index of the item you want to modify: ");
                int index = int.Parse(Console.ReadLine());
                if (index - 1 > ModifyDummyDatasItems.Count())
                {
                    ConsoleError("Invalid input");
                }
                else
                {
                    ConsoleMessageLine(promt, ModifyDummyDatasItems[index - 1]);
                    ConsoleMessage(promt, "Enter the modified text or type --delete to delet it: ");
                    string input = Console.ReadLine();
                    if (input == "--delete")
                    {
                        ModifyDummyDatasItems.RemoveAt(index - 1);
                        StreamWriter ir = new StreamWriter(ModifyDummyData + ".txt");
                        for (int i = 0; i < ModifyDummyDatasItems.Count; i++)
                        {
                            ir.WriteLine(ModifyDummyDatasItems[i]);
                        }
                        ir.Close();
                        ConsoleMessageLine(promt, "Delet was successful");
                    }
                    else
                    {
                        ModifyDummyDatasItems.RemoveAt(index - 1);
                        StreamWriter ir = new StreamWriter(ModifyDummyData + ".txt");
                        for (int i = 0; i < ModifyDummyDatasItems.Count; i++)
                        {
                            ir.WriteLine(ModifyDummyDatasItems[i]);
                        }
                        ir.WriteLine(input);
                        ir.Close();
                        ConsoleMessageLine(promt, "Modify was successful");
                    }
                }
            }
            else if (mORa.ToLower().Contains("add"))
            {
                ConsoleMessage(promt, "Enter the new text data: ");
                string newData = Console.ReadLine();
                if (newData == "" || newData == " ")
                {
                    ConsoleError("You need to enter something");
                }
                else if (!ModifyDummyDatasItems.Contains(newData))
                {
                    StreamWriter ir = new StreamWriter(ModifyDummyData + ".txt");
                    for (int i = 0; i < ModifyDummyDatasItems.Count; i++)
                    {
                        ir.WriteLine(ModifyDummyDatasItems[i]);
                    }
                    ir.WriteLine(newData);
                    ir.Close();
                    ConsoleMessageLine(promt, "The new data was successfully added");
                }
                else
                {
                    ConsoleError("You need to enter something new");
                }
            }
            else
            {
                ConsoleError("Invalid input");
            }
        }
    }
}
