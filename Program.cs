using GenSP.Models;
using GenSP.T4;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace GenSP
{
    static class Program
    {
        public static string P_ConnectionString;
        public static string P_NameSpace;
        public static string P_ContextSource;
        public static string P_Schema;
        public static bool P_ExcludeSystemObject = true;
        public static string P_OutPutSolutionFolder;
        public static string P_OutPutPhysicalFolder;
        public static string P_OutPutFilename;


        public static List<Sp> SpList = new List<Sp>();
        public static List<SpException> ExceptionList = new List<SpException>();

        static async Task<int> Main(string[] args)
        {
            var cmd = new RootCommand();
            cmd.AddCommand(GenSPScanCommand());
            return await cmd.InvokeAsync(args);
        }

        public static void GenSPScan()
        {

            var dt_SpList = new DataTable();
            var dt_SpParam = new DataTable();
            var dt_SpResult = new DataTable();

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss")} STEP 1 - QUET STORED PROCEDURE");

            dt_SpList = Get_StoreProcedure_List();

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss")} STEP 2 - BAT DAU LAY STORED PROCEDURE");
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss")} STEP 2 - Total Stored Procedure: {dt_SpList.Rows.Count}");

            int i = 1;
            string _schema = "";
            string _sp = "";

            foreach (DataRow r in dt_SpList.Rows)
            {

                _schema = r["ROUTINE_SCHEMA"].ToString();
                _sp = r["ROUTINE_NAME"].ToString();

                dt_SpParam = Get_StoreProcedure_Param(_schema, _sp);
                dt_SpResult = Get_StoreProcedure_Result(_schema, _sp);

                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss")} STEP 2 - {i} / {dt_SpList.Rows.Count} ==> \"{r["ROUTINE_NAME"]}\"");

                var pList = new List<SpParam>();
                foreach (DataRow par in dt_SpParam.Rows)
                {

                    var _p = new SpParam()
                    {

                        Param = par["Parameter"].ToString().Replace("@", ""),
                        Type = SP_GetType(par["Type"].ToString(), (bool)par["is_nullable"]),
                        Length = (par["Length"].GetType().Name == "DBNull" ? null : par["Length"].ToString()),
                        Precision = (par["Precision"].GetType().Name == "DBNull" ? null : par["Precision"].ToString()),
                        Scale = (par["Scale"].GetType().Name == "DBNull" ? null : par["Scale"].ToString()),
                        Order = (par["Order"].GetType().Name == "DBNull" ? null : par["Order"].ToString()),
                        isOutput = (bool)par["is_Output"],
                        isNullable = (bool)par["is_nullable"],
                        Collation = (par["Collation"].GetType().Name == "DBNull" ? null : par["Collation"].ToString()),
                        DbType = SP_GetDbType(par["Type"].ToString()),

                        sql_Param = (par["Parameter"].GetType().Name == "DBNull" ? null : par["Parameter"].ToString()),
                        sql_Type = (par["Type"].GetType().Name == "DBNull" ? null : par["Type"].ToString()),
                        sql_Length = (par["Length"].GetType().Name == "DBNull" ? null : par["Length"].ToString()),
                        sql_Prec = (par["Precision"].GetType().Name == "DBNull" ? null : par["Precision"].ToString()),
                        sql_Scale = (par["Scale"].GetType().Name == "DBNull" ? null : par["Scale"].ToString()),
                        sql_Order = (par["Order"].GetType().Name == "DBNull" ? null : par["Order"].ToString()),
                        sql_isOutput = (par["is_Output"].GetType().Name == "DBNull" ? null : par["is_Output"].ToString()),
                        sql_isNullable = (par["is_nullable"].GetType().Name == "DBNull" ? null : par["is_nullable"].ToString()),
                        sql_Collation = (par["Collation"].GetType().Name == "DBNull" ? null : par["Collation"].ToString()),
                    };



                    pList.Add(_p);
                }

                var rList = new List<SpResultElement>();
                int rCounter = 0;
                foreach (DataRow res in dt_SpResult.Rows)
                {

                    var _r = new SpResultElement()
                    {

                        Name = (string.IsNullOrEmpty(res["name"].ToString()) ? $"Col{rCounter}" : res["name"].ToString()),
                        Type = SP_GetType(res["system_type_name"].ToString(), (bool)res["is_nullable"]),
                        Length = (res["max_length"].GetType().Name == "DBNull" ? null : res["max_length"].ToString()),
                        Precision = (res["precision"].GetType().Name == "DBNull" ? null : res["precision"].ToString()),
                        Scale = (res["scale"].GetType().Name == "DBNull" ? null : res["scale"].ToString()),
                        Order = (res["column_ordinal"].GetType().Name == "DBNull" ? null : res["column_ordinal"].ToString()),
                        isNullable = (bool)res["is_nullable"],
                        Collation = (res["collation_name"].GetType().Name == "DBNull" ? null : res["collation_name"].ToString()),

                        sql_Name = (res["name"].GetType().Name == "DBNull" ? null : res["name"].ToString()),
                        sql_Type = (res["system_type_name"].GetType().Name == "DBNull" ? null : res["system_type_name"].ToString()),
                        sql_Length = (res["max_length"].GetType().Name == "DBNull" ? null : res["max_length"].ToString()),
                        sql_Precision = (res["precision"].GetType().Name == "DBNull" ? null : res["precision"].ToString()),
                        sql_Scale = (res["scale"].GetType().Name == "DBNull" ? null : res["scale"].ToString()),
                        sql_Order = (res["column_ordinal"].GetType().Name == "DBNull" ? null : res["column_ordinal"].ToString()),
                        sql_isNullable = (res["is_nullable"].GetType().Name == "DBNull" ? null : res["is_nullable"].ToString()),
                        sql_Collation = (res["collation_name"].GetType().Name == "DBNull" ? null : res["collation_name"].ToString()),

                    };


                    rList.Add(_r);
                }

                var sp = new Sp()
                {
                    Name = r["ROUTINE_NAME"].ToString(),
                    Schema = r["ROUTINE_SCHEMA"].ToString(),
                    Params = pList,
                    Results = rList
                };

                SpList.Add(sp);

                i++;
            }

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss")} XONG");

            GenSPT4 genSPT4Processed = new GenSPT4(SpList, P_NameSpace, P_OutPutSolutionFolder, P_ContextSource);

            File.WriteAllText(Path.Combine(P_OutPutPhysicalFolder, P_OutPutFilename), genSPT4Processed.TransformText());

            if (ExceptionList.Count > 0)
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss")} Da tim thay exception! Vui long check o file GenSP_log.txt in '{P_OutPutPhysicalFolder}'");
                WriteException();
            }

        }

        private static Command GenSPScanCommand()
        {
            var cmd = new Command("scan", "GenSP.exe conStr Namespace nameStore DBcontext folderModel locationFile fileResult!!!");

            cmd.AddOption(new Option<string>(new[] { "--connection", "-cnn" }, "SQL SERVER 2012+ Connection String")
            {
                Arity = ArgumentArity.ExactlyOne
            });

            cmd.AddOption(new Option<string>(new[] { "--schema", "-sch" }, "SQL SERVER Schema (set * for no filter)")
            {
                Arity = ArgumentArity.ExactlyOne
            });

            cmd.AddOption(new Option<string>(new[] { "--namespace", "-nsp" }, "Solution namespace")
            {
                Arity = ArgumentArity.ExactlyOne,
                IsRequired = false
            });

            cmd.AddOption(new Option<string>(new[] { "--context", "-ctx" }, "Context from")
            {
                Arity = ArgumentArity.ExactlyOne,
                IsRequired = false
            });

            cmd.AddOption(new Option<string>(new[] { "--sfolder", "-sf" }, "Solution Folder (ex 'Model')")
            {
                Arity = ArgumentArity.ExactlyOne,
                IsRequired = false
            });

            cmd.AddOption(new Option<string>(new[] { "--pfolder", "-pf" }, @"Physical Destination Folder (ex 'C:\MySolution\Model')")
            {
                Arity = ArgumentArity.ExactlyOne,
                IsRequired = false
            });

            cmd.AddOption(new Option<string>(new[] { "--filename", "-f" }, @"Output Filename (ex 'GenSPContext.cs')")
            {
                Arity = ArgumentArity.ExactlyOne,
                IsRequired = false
            });

            cmd.Handler = CommandHandler.Create<string, string, string, string, string, string, string>((cnn, nsp, sch, ctx, sf, pf, f) =>
            {
                if (string.IsNullOrEmpty(nsp))
                {
                    nsp = "API";
                }
                if (string.IsNullOrEmpty(ctx))
                {
                    ctx = "DBContext";
                }
                if (string.IsNullOrEmpty(sf))
                {
                    sf = "Models";
                }
                if (string.IsNullOrEmpty(pf))
                {
                    pf = Directory.GetCurrentDirectory();
                }
                if (string.IsNullOrEmpty(f))
                {
                    string currentTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    f = $"Result_{currentTime}.cs";
                }
                P_ConnectionString = cnn;
                P_NameSpace = nsp;
                P_Schema = sch;
                P_ContextSource = ctx;
                P_OutPutSolutionFolder = sf;
                P_OutPutPhysicalFolder = pf;
                P_OutPutFilename = f;    

                if (!string.IsNullOrEmpty(cnn) &&
                   !string.IsNullOrEmpty(nsp) &&
                   !string.IsNullOrEmpty(ctx) &&
                   !string.IsNullOrEmpty(sf) &&
                   !string.IsNullOrEmpty(pf) &&
                   !string.IsNullOrEmpty(f))
                {
                    // Check if the output physical folder exists, and create it if it doesn't.
                    if (!Directory.Exists(pf))
                    {
                        Directory.CreateDirectory(pf);
                    }
                    if (string.IsNullOrEmpty(sch))
                    {
                        Console.WriteLine("GenSP - ERROR: Parameter Missing: schema");
                    }
                    else if (sch == "-sch *")
                    {
                        P_Schema = "*"; // Set the schema to "*" to get all stored procedures
                        GenSPScan();
                    }
                    else
                    {
                        GenSPScan(); // get 1 store
                    }
                }
                else
                {
                    // Output the missing parameters if any
                    Console.WriteLine("GenSP - ERROR:");
                    if (string.IsNullOrEmpty(cnn))
                    {
                        Console.WriteLine("Parameter Missing: connection");
                    }

                }
            });

            return cmd;
        }


        private static string SP_GetType(string type, bool isNullable)
        {
            // chuyen doi kieu du lieu dau vao thanh chu thuong va loai bo khoang trang o dau va cuoi chuoi
            type = type.ToLower().Trim();

            // kiem tra kieu du lieu trong sql server va tra ve kieu du lieu tuong ung trong c#
            if (type == "int")
                return "int" + (isNullable ? "?" : "");
            else if (type == "uniqueidentifier")
                return "Guid" + (isNullable ? "?" : "");
            else if (type == "money" || type.Contains("float") || type.Contains("numeric") || type.Contains("decimal"))
                return "decimal" + (isNullable ? "?" : "");
            else if (type == "text" || type.IndexOf("nvarchar") > -1 || type.IndexOf("varchar") > -1 || type.IndexOf("char") > -1)
                return "string";
            else if (type.Contains("table type"))
                return "DataTable";
            else if (type == "bigint" || type == "smallint")
                return type == "bigint" ? "Int64" : "int" + (isNullable ? "?" : "");
            else if (type == "tinyint")
                return "Byte" + (isNullable ? "?" : "");
            else if (type.IndexOf("smalldatetime") > -1 || type.IndexOf("datetimeoffset") > -1 || type.IndexOf("datetime") > -1 || type.IndexOf("date") > -1)
                return "DateTime" + (isNullable ? "?" : "");
            else if (type == "bit")
                return "bool" + (isNullable ? "?" : "");
            else
                throw new UnknownDBTypeException(type);
        }


        private static string SP_GetDbType(string type)
        {
            // Convert the input type to lowercase and remove leading/trailing spaces
            type = type.ToLower().Trim();

            // Check the SQL Server data type and return the corresponding C# data type
            if (type == "int")
                return "Int32";
            else if (type == "text" || type.IndexOf("nvarchar") > -1 || type.IndexOf("varchar") > -1 || type.IndexOf("char") > -1)
                return "string";
            else if (type.Contains("table type"))
                return "DataTable";
            else if (type == "uniqueidentifier")
                return "Guid";
            else if (type == "money" || type.Contains("float") || type.Contains("numeric") || type.Contains("decimal"))
                return "decimal";
            else if (type == "bigint")
                return "Int64";
            else if (type == "smallint")
                return "Int16";
            else if (type == "tinyint")
                return "Byte";
            else if (type.IndexOf("smalldatetime") > -1 || type.IndexOf("datetimeoffset") > -1 || type.IndexOf("datetime") > -1 || type.IndexOf("date") > -1)
                return "DateTime";
            else if (type == "bit")
                return "Boolean";
            else
                throw new UnknownDBTypeException(type); // Throw an exception for unknown types
        }

        private static DataTable Get_StoreProcedure_List()
        {
            DataTable dtResult = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(P_ConnectionString))
                {
                    string sql = $@"
                        SELECT * 
                          FROM INFORMATION_SCHEMA.ROUTINES
                         WHERE ROUTINE_TYPE = 'PROCEDURE' "
                                          + (P_ExcludeSystemObject ? " AND LEFT(ROUTINE_NAME, 3) NOT IN ('sp_', 'xp_', 'ms_')" : "")
                                          + (P_Schema != "*" ? $"AND ROUTINE_NAME =  '{P_Schema}'" : "") + // Modify the condition for "*" schema
                                        " ORDER BY ROUTINE_NAME";

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(sql, connection);
                    adapter.Fill(dtResult);
                }

                return dtResult;
            }
            catch (Exception e)
            {

                ExceptionList.Add(new SpException() { Method = "Get_StoreProcedure_List", Message = e.Message });

                return dtResult;
            }
        }

        private static DataTable Get_StoreProcedure_Param(string schema, string sp)
        {
            DataTable dtResult = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(P_ConnectionString))
                {
                    string sql = $@"
                                SELECT  
                                   'Parameter' = name,  
                                   'Type'   = type_name(system_type_id),  
                                   'Length'   = CAST(max_length AS INT),  
                                   'Precision'   = CAST(case when type_name(system_type_id) = 'uniqueidentifier' 
                                              then precision  
                                              else OdbcPrec(system_type_id, max_length, precision) end AS INT),  
                                   'Scale'   = CAST(OdbcScale(system_type_id, scale) AS INT),  
                                   'Order'  = CAST(parameter_id AS INT),  
                                   'Collation'   = convert(sysname, 
                                                   case when system_type_id in (35, 99, 167, 175, 231, 239)  
                                                   then ServerProperty('collation') end),
                                  is_Output,
	                              is_nullable
                                  from sys.parameters where object_id = object_id('{schema}.{sp}')
                                  ORDER BY parameter_id
                                ";

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(sql, connection);
                    adapter.Fill(dtResult);
                }

                return dtResult;
            }
            catch (Exception e)
            {

                ExceptionList.Add(new SpException() { Method = "Get_StoreProcedure_Param", FullName = $"{schema}.{sp}", Schema = schema, StoreProcedure = sp, Message = e.Message });

                return dtResult;
            }
        }

        private static DataTable Get_StoreProcedure_Result(string schema, string sp)
        {
            DataTable dtResult = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(P_ConnectionString))
                {
                    string sql = $@"exec sp_describe_first_result_set N'{schema}.{sp}'";

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(sql, connection);
                    adapter.Fill(dtResult);
                }

                return dtResult;
            }
            catch (Exception e)
            {

                ExceptionList.Add(new SpException() { Method = "Get_StoreProcedure_Result", FullName = $"{schema}.{sp}", Schema = schema, StoreProcedure = sp, Message = e.Message });

                return dtResult;
            }
        }

        private static void WriteException()
        {
            StringBuilder sb = new StringBuilder();

            int i = 1;
            try
            {
                foreach (var e in ExceptionList)
                {
                    sb.AppendLine($"{DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss")} - EXCEPTION {i} / {ExceptionList.Count}: {e.StoreProcedure} - {e.Message}");
                    i++;
                }
                File.WriteAllText(Path.Combine(P_OutPutPhysicalFolder, "GenSP_log.txt"), sb.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss")} ERROR!!! --> {e.Message}");
            }
        }

        public static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input.First().ToString().ToUpper() + input.Substring(1)
        };

        public static string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);
            return source.Remove(place, find.Length).Insert(place, replace);
        }

    }
}
