using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Loaders
{
    static class Common
    {
        public static DataSet GetDataSet(string sql)
        {
            var ds = new DataSet();

            using (var cnn = new SqlConnection(SampleConfig.ConnectionString))
            {
                try
                {
                    cnn.Open();
                    using (var cmd = new SqlCommand(sql, cnn))
                    {
                        using (var a = new SqlDataAdapter(cmd))
                        {
                            a.Fill(ds);
                        }
                    }

                    cnn.Close();
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    Console.WriteLine($"Exception : {ex.Message}");
                }
            }

            return ds;
        }
    }
}
