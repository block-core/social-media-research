using System;
using Newtonsoft.Json;

namespace FileIo{
	/// <summary>
	/// A file fileName item.
	/// </summary>
	[JsonObject()]
	public class FileItemInfo {

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
		/// The file name.
		/// </summary>
		[JsonProperty("filename")]
		[JsonRequired()]
		public string Filename { get; set; }

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

		/// <summary>
		/// The file version.
		/// </summary>
		[JsonProperty("version")]
		public string Version { get; set; }

		#endregion

	}
}