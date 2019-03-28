using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using WebAdvert.Web.Models.AdvertManagement;
using WebAdvert.Web.Services.FileUploader.Interface;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagementController : Controller
    {

        private readonly IFileUploader fileUploader;

        public AdvertManagementController(IFileUploader fileUploader)
        {
            this.fileUploader = fileUploader;
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
                var id = "1111";
                //we must call to AdvertApi,create advertement in database and return id

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
                        return RedirectToAction("Index",controllerName: "Home");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            return View(model);
        }

      
    }
}