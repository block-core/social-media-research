using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileBaseSync.Models
{
    public class FileBaseCredentialsOptions
    {
        public const string Credentials = "FileBaseCredentials";
        public string AccessKey { get; set; } = String.Empty;
        public string SecretKey { get; set; } = String.Empty;
    }
}

