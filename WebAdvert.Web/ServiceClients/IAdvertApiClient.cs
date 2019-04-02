using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertApi.models;

namespace WebAdvert.Web.ServiceClients
{
    public interface IAdvertApiClient
    {
        Task<AdvertResponse> Create(CreateAdvertModel model);

        Task<bool> Confirm(ConfirmAdvertRequest model);
    }
}
