using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Lokad.Cqrs.StreamingStorage;

namespace Hub.Web
{
    public  class GlobalLog
    {
        public static string EncodeError(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var pdb = new Rfc2898DeriveBytes("errors", new byte[] { 0x4A, 0x87, 0x23, 0x71, 0x45, 0x56, 0x68, 0x14, 0x02, 0x84 });

            var aes = new AesManaged();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);
            using (var a = new MemoryStream())
            using (var e = aes.CreateEncryptor())
            using (var s = new CryptoStream(a, e, CryptoStreamMode.Write))
            {
                s.Write(bytes, 0, bytes.Length);
                s.FlushFinalBlock();
                return Convert.ToBase64String(a.ToArray(), Base64FormattingOptions.InsertLineBreaks);
            }

        }
        public void LogError(Guid id, string render)
        {
            // this one should be non-blocking and non-failing

            try
            {
                // script-kiddies.
                if (ShouldLog(render))
                {
                    Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                PerformLogging(id, render);
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine(ex);
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

         bool ShouldLog(string error)
        {
            // script kiddies.
            // System.Web.HttpException (0x80004005): The controller for path 
            // '/phpMyAdmin-2.8.2/scripts/setup.php' 
            // was not found or does not implement IController.

            if (error.Contains(".php'"))
                return false;


            return true;
        }

        public static bool Is404(string error)
        {
            if (string.IsNullOrEmpty(error))
                return false;
            if (error.IndexOf("was not found or does not implement IController", StringComparison.InvariantCultureIgnoreCase) > 0)
                return true;
            return false;
        }

        public static string RenderError(Guid id, string explicitError)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Error: " + id);
            try
            {
                
                
                builder.AppendLine("Explicit Error:");
                builder.AppendLine(explicitError ?? "None").AppendLine();

                var context = HttpContext.Current;
                if (context != null)
                {
                    foreach (var exception in context.AllErrors ?? new Exception[0])
                    {
                        builder.AppendLine("Context Exception:").AppendLine(exception.ToString()).AppendLine();
                    }
                    builder.AppendFormat("Request: {0}", context.Request.Url);
                }
                
            }
            catch(Exception ex)
            {
                builder.AppendLine(ex.ToString());
            }
            return builder.ToString();
        }

        readonly IStreamContainer _container;
        public GlobalLog(IStreamContainer container)
        {
            _container = container;
        }

        void PerformLogging(Guid id, string error)
        {
            _container.Create();

            const string format = "{0:yyyy-MM-dd-HH-mm}-{1}-web.txt";
            var uri = string.Format(format, DateTime.UtcNow, id.ToString().ToLowerInvariant());
            var blob = _container.GetItem(uri);
            blob.WriteText(error);;
        }
    }
}