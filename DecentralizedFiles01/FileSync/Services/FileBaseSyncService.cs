using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileBaseSync;

namespace FileBaseSync.Services
{
    public class FileBaseSyncService
    {
        private ILocalFileService localFileService;
        private IFileBaseService fileBaseService;


        public FileBaseSyncService(ILocalFileService _localFileService, IFileBaseService _fileBaseService)
        {
            localFileService = _localFileService;
            fileBaseService = _fileBaseService;
        }



    }
}
