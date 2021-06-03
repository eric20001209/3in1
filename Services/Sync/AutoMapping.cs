using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Sync.Models;
using Sync.Dtos;

namespace Sync.Services
{
	public class AutoMapping : Profile
	{
		public AutoMapping()
		{
			CreateMap<Card, CardDto>();
			CreateMap<ImageUploadDto, Image>();
		}
	}
}
