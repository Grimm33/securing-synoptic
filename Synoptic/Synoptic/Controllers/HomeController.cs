using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Synoptic.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Synoptic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message")) ViewBag.Message = TempData["message"].ToString();
            if (TempData.ContainsKey("error")) ViewBag.Error = TempData["error"].ToString();
            if (TempData.ContainsKey("mode")) ViewBag.Mode = TempData["mode"].ToString();

            return View();
        }

        [HttpPost]
        [Route("Hide")]
        public IActionResult Hide(BmpTextViewModel model)
        {
            string message = "", error = "";

            if (ModelState.IsValid && model != null)
            {
                try
                {
                    string encryptdText = EncryptionHelper.Encrypt(EncryptionHelper.Hash(model.Password), model.Text);

                    Bitmap embedded;

                    if (model.Image != null)
                    {
                        if (model.Image.OpenReadStream().Length > 0 && model.Image.OpenReadStream().Length < 10485760)
                        {
                            byte[] readBytes = new byte[2];
                            model.Image.OpenReadStream().Read(readBytes, 0, 2);
                            model.Image.OpenReadStream().Position = 0;
                            if (readBytes[0] == 66 && readBytes[1] == 77)
                            {
                                Bitmap image = new Bitmap(Image.FromStream(model.Image.OpenReadStream()));

                                embedded = SteganographyHelper.embedText(encryptdText, image);

                                using (var stream = new MemoryStream())
                                {
                                    embedded.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

                                    return File(stream.ToArray(), "application/octet-stream", 
                                        model.Image.FileName.IndexOf(".bmp") != 0 
                                        ? model.Image.FileName.Substring(0, model.Image.FileName.Length -4) + "_Encrypted.bmp"
                                        : model.Image.FileName + "_Encrypted.bmp");
                                }

                                message = "File uploaded successfully";
                            }
                            else error = "Only BITMAP images are allowed!";
                        }
                        else error = "File must be smaller than 10MB";
                    }
                }
                catch (Exception)
                {
                    error = "File failed to be added";
                }
            }

            if (message.Length > 0) TempData["message"] = message;
            if (error.Length > 0) TempData["error"] = error;
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Extract")]
        public IActionResult Extract(BmpTextViewModel model)
        {
            string message = "", error = "";

            if (ModelState.IsValid && model != null)
            {
                try
                {
                    Bitmap embedded;

                    if (model.Image != null)
                    {
                        if (model.Image.OpenReadStream().Length > 0 && model.Image.OpenReadStream().Length < 10485760)
                        {
                            byte[] readBytes = new byte[2];
                            model.Image.OpenReadStream().Read(readBytes, 0, 2);
                            model.Image.OpenReadStream().Position = 0;
                            if (readBytes[0] == 66 && readBytes[1] == 77)
                            {
                                Bitmap image = new Bitmap(Image.FromStream(model.Image.OpenReadStream()));

                                string encryptedText = SteganographyHelper.extractText(image);

                                string decryptdText = EncryptionHelper.Decrypt(EncryptionHelper.Hash(model.Password), encryptedText);

                                message = decryptdText;
                            }
                            else error = "Only BITMAP images are allowed!";
                        }
                        else error = "File must be smaller than 10MB";
                    }
                }
                catch (Exception)
                {
                    error = "Wrong password";
                }
            }

            if (message.Length > 0) TempData["message"] = message;
            if(error.Length > 0) TempData["error"] = error;
            TempData["mode"] = "extract";
            return RedirectToAction("Index");
        }
    }
}
