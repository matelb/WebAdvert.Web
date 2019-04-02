using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.ServiceClients;
using AdvertApi.models;
using WebAdvert.Web.Models.AdvertManagement;

namespace WebAdvert.Web
{
    public class AutoMapperRegister : Profile
    {

        public AutoMapperRegister()
        {
            CreateMap<CreateAdvertModel, AdvertModel>().ReverseMap();
            CreateMap<AdvertResponse, CreateAdvertResponse>().ReverseMap();
            CreateMap<ConfirmAdvertRequest, ConfirmAdvertModel>().ReverseMap();
            CreateMap<CreateAdvertViewModel, CreateAdvertModel>().ReverseMap();
        }
    }
}
