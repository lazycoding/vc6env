using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace vc6env
{
    class Program
    {
        static string include;
        static string library;
        static void Main(string[] args)
        {
            string filePath = @"vc6.reg";
            string content = ReadRegTemplate(filePath);

            Console.WriteLine(content);

            string directory = AppDomain.CurrentDomain.BaseDirectory;
            include = directory + @"Common";
            include = include.Replace(@"\",@"\\");
            library = directory + @"Lib";
            library = library.Replace(@"\", @"\\");

            string modified = ModifyContent(content);

            Console.WriteLine(modified);

            filePath = CreateNewRegFile(modified);

            RunReg(filePath);
        }

        private static string ModifyContent(string content)
        {
            var rows = content.Split(Environment.NewLine.ToCharArray());
            string include_dirs = "\"Include Dirs\"";
            string lib_dirs = "\"Library Dirs\"";
            for (int i = 0; i < rows.Length;i++)
            {
                var row = rows[i];
                if (string.IsNullOrEmpty(row))
                {
                    continue;
                }
                var index_inc = row.IndexOf(include_dirs);
                var index_lib = row.IndexOf(lib_dirs);
                if (index_inc >= 0)
                {
                    row = row.Substring(0, row.Length - 1);
                    row = row + include + "\"";                    
                }
                if (index_lib >= 0)
                {
                    row = row.Substring(0, row.Length - 1);
                    row = row + library + "\"";
                }
                rows[i] = row;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var item in rows)
            {

                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }
                sb.Append(item);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static void RunReg(string regFile)
        {
           
        }

        private static string CreateNewRegFile(string content)
        {
            var filepath = AppDomain.CurrentDomain.BaseDirectory + "tmp.reg";
            try
            {
                using (FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate))
                {
                    var buffer = Encoding.Unicode.GetBytes(content);
                    fs.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return filepath;
        }

        private static string ReadRegTemplate(string filePath)
        {
            var content = string.Empty;
            try
            {
                var fs = Type.GetType("vc6env.Program").Assembly.GetManifestResourceStream("vc6env." + filePath);
                fs.Seek(0, SeekOrigin.End);
                var length = fs.Position;
                fs.Seek(0, SeekOrigin.Begin);

                var buffer = new byte[length];
                fs.Read(buffer, 0, (int)length);

                content = Encoding.Unicode.GetString(buffer);

                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return content;
        }
    }
}
