using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sync.Dtos;
using System.Drawing;
using Microsoft.Extensions.Configuration;

namespace Sync.Services
{
	public interface IImageService
	{
		Task<Tuple<bool, string,string, byte[]>> FormFileToBinary( IFormFile file, ILogger logger);
		Task<Tuple<bool, string>> FormBinaryToFile(int code, byte[] binaryFile, string extensionName, string folder, ILogger logger);
	}
	public class ImageService : IImageService
	{
		private readonly IConfiguration _config;
		public ImageService(IConfiguration config)
		{
			_config = config;
		}

		/*	Convert binary to image */
		public async Task<Tuple<bool, string>> FormBinaryToFile(int code, byte[] binaryFile,string extensionName, string folder, ILogger logger)
		{
			try
			{
				/* Create an images folder to save pictures */
				var folderPath = Path.Combine(_config["RootPath"], folder);
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}
				var newFileName = code + "-" + GetTimestamp(DateTime.Now) + extensionName;
			//			Path.GetFileNameWithoutExtension(file.FileName) + "-" + file.GetHashCode() + Path.GetExtension(file.FileName);
				var filePath = Path.Combine(folderPath, newFileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					return new Tuple<bool, string>(true, filePath); //return file
				}
			}
			catch (Exception)
			{

				throw;
			}
		}


		/* Convert image to binary*/
		public async Task<Tuple<bool, string, string, byte[]>> FormFileToBinary( IFormFile file, ILogger logger)
		{
			/* Validate input */
			if (file is null)
			{
				return new Tuple<bool, string, string, byte[]>(false, "The picture is not selected.","", null);
			}

			if (file.Length == 0)
			{
				return new Tuple<bool, string, string, byte[]>(false, "The picture is empty.", "", null);
			}

			string[] allowedFileExtensions = { ".JPEG", ".GIF", ".PNG", ".JPG", ".TIF", ".PSD" };
			if (!allowedFileExtensions.Contains(Path.GetExtension(file.FileName).ToUpper()))
			{
				return new Tuple<bool, string, string, byte[]>(false, "Wrong file format. Only JPEG, GIF, PNG, JPG, TIF, and PSD are supported.", "",null);
			}
			try
			{
				var newFileName = Path.GetFileNameWithoutExtension(file.FileName) +
						"-" + file.GetHashCode() +
							Path.GetExtension(file.FileName);

				var extensionName = Path.GetExtension(file.FileName);
				using (var ms = new MemoryStream())
				{
					await file.CopyToAsync(ms);
					var fileBytes = ms.ToArray();
					string s = Convert.ToBase64String(fileBytes);
					/* Log */
					logger.LogInformation($"{file.Name} - Newly uploaded file with name = {file.Name}");
					return new Tuple<bool, string,string, byte[]>(true, newFileName, extensionName, fileBytes);
				}
			}
			catch (Exception ex)
			{
				return new Tuple<bool, string, string, byte[]>(false, ex.ToString(), "", null);
			}
		}

		public static String GetTimestamp(DateTime value)
		{
			return value.ToString("yyyyMMddHHmmssffff");
		}
	}
}
