using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerce_API.Data;
using eCommerce_API.Services;
using eCommerce_API_RST.Dto;
using eCommerce_API_RST.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace eCommerce_API_RST.Controllers
{
	[Route("api/message")]
	[ApiController]
	public class MessageBoardController : ControllerBase
	{
		private readonly iMailService _mail;
		private readonly rst374_cloud12Context _context;
		private IConfiguration _config;
		public MessageBoardController(iMailService mail, rst374_cloud12Context context, IConfiguration config)
		{
			_mail = mail;
			_context = context;
			_config = config;
		}
		[HttpPost]
		public async Task<IActionResult> sendMessage([FromBody] MessageDto message)
		{
			
			if (!ModelState.IsValid)
				return BadRequest(JsonConvert.SerializeObject(ModelState.Values.Select(e => e.Errors).ToList()));
			try
			{
				var receiverEmail = _config["ContactEmail"];
				/* add to messageboard table*/
				var messageboard = new MessageBoard()
				{
					Name = message.Name,
					Subject = message.Subject,
					Content = message.Content,
					Email = message.Email
				};
				await _context.MessageBoard.AddAsync(messageboard);
				//update database
				await _context.SaveChangesAsync();

				/* send email to supplier */
				var subject = message.Subject;
				var content = "Name :" + message.Name + "<br/>";
				content += "Contact Email : " + message.Email + "<br/><br/>";
				content += message.Content;
				await _mail.sendEmail(receiverEmail, subject, content, null);

				return Ok();
			}
			catch (Exception)
			{

				throw;
			}

		}
	}
}
