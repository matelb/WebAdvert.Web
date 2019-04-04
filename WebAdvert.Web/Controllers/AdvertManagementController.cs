using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.ServiceClients;
using WebAdvert.Web.Services.FileUploader.Interface;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagementController : Controller
    {

        private readonly IFileUploader fileUploader;

        private readonly IAdvertApiClient advertApiClient;

        private readonly IMapper mapper;

        public AdvertManagementController(  IFileUploader fileUploader, 
                                            IAdvertApiClient advertApiClient,
                                            IMapper mapper)
        {
            this.fileUploader = fileUploader;
            this.advertApiClient = advertApiClient;
            this.mapper = mapper;
        }


        // GET: AdvertManagement/Create
        public IActionResult Create(CreateAdvertViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAdvertViewModel model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                
                //we must call to AdvertApi,create advertement in database and return id
                    
                var createAdvertModel = mapper.Map<CreateAdvertModel>(model);

                createAdvertModel.FilePath = imageFile.FileName;

                var apiCallResponse = await advertApiClient.Create(createAdvertModel);

                var id = apiCallResponse.Id;

                var fileName = "";
                if(imageFile != null)
                {
                    fileName = !string.IsNullOrEmpty(imageFile.FileName) ? Path.GetFileName(imageFile.FileName) : imageFile.Name;
                    var filePath = $"{id}/{fileName}";

                    try
                    {
                        using(var readStream = imageFile.OpenReadStream())
                        {
                            var result = await fileUploader.UploadFileAsync(filePath, readStream).ConfigureAwait(false);
                            if (!result)
                                throw new System.Exception(message: "Could not load the image into the repository, Please see the logs");
                        }


                        var confirmModel = new ConfirmAdvertRequest
                        {
                            Id = id,
                            Status = AdvertApi.models.AdvertStatus.Active

                        };

                        var canConfirm = await advertApiClient.Confirm(confirmModel);

                        if (!canConfirm)
                        {
                            throw new Exception(message: $"Cannot confirm advert of id = {id}");
                        }

                        return RedirectToAction("Index",controllerName: "Home");
                    }
                    catch (Exception ex)
                    {
                        var confirmModel = new ConfirmAdvertRequest
                        {
                            Id = id,
                            Status = AdvertApi.models.AdvertStatus.Pending

                        };

                        await advertApiClient.Confirm(confirmModel);

                        Console.WriteLine(ex);
                    }
                }
            }

            return View(model);
        }

      
    }
}