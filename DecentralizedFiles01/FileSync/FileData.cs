using System;
using Newtonsoft.Json;

namespace FileBaseSync
{
    /// <summary>
    /// A file fileName item.
    /// </summary>
    [JsonObject()]
    public class FileData
    {

        #region Public Properties

        /// <summary>
        /// The file fileName.
        /// </summary>
        [JsonProperty("fileName")]
        [JsonRequired()]
        public string FileName { get; set; }

        /// <summary>
        /// The path.
        /// </summary>
        [JsonProperty("path")]
        [JsonRequired()]
        public string Path { get; set; }

        /// <summary>
        /// The file size.
        /// </summary>
        [JsonProperty("size")]
        public long Size { get; set; }

        /// <summary>
        /// When the file was last modified.
        /// </summary>
        [JsonProperty("lastModified")]
        public DateTime LastModified { get; set; }

        #endregion

    }
}
