using AdvertApi.models;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebAdvert.Web.ServiceClients
{
    public class AdvertApiClient : IAdvertApiClient
    {

        private readonly IConfiguration configuration;

        private readonly HttpClient client;

        private readonly IMapper mapper;

        public AdvertApiClient(IConfiguration configuration, HttpClient client, IMapper mapper)
        {
            this.configuration = configuration;
            this.client = client;
            this.mapper = mapper;

            var createUrl = configuration.GetSection("AdvertApi").GetValue<string>("BaseUrl");
            client.BaseAddress = new Uri(createUrl);
            client.DefaultRequestHeaders
              .Accept
              .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
        }

        public async Task<bool> Confirm(ConfirmAdvertRequest model)
        {
            var advertModel = mapper.Map<ConfirmAdvertModel>(model);
            var jsonModel = JsonConvert.SerializeObject(advertModel);
            var response = await client.PutAsync(new Uri($"{client.BaseAddress}/confirm"), new StringContent(jsonModel)).ConfigureAwait(false);
            return response.StatusCode == System.Net.HttpStatusCode.OK;


        }

        public async Task<AdvertResponse> Create(CreateAdvertModel model)
        {
            var advertApiModel = mapper.Map<AdvertModel>(model);//new AdvertModel(); //Automapper
            var jsonModel = JsonConvert.SerializeObject(advertApiModel);

            var response = await client.PostAsync(new Uri($"{client.BaseAddress}/Create"), new StringContent(jsonModel,encoding:Encoding.UTF8,mediaType: "application/json")).ConfigureAwait(false);

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var createAdvertResponse = JsonConvert.DeserializeObject<CreateAdvertResponse>(responseJson);
            var advertResponse = mapper.Map< CreateAdvertResponse, AdvertResponse>(createAdvertResponse);
            return advertResponse;
        }
    }
}
