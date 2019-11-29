﻿using EmailConfirmationServer.Models;
using EmailConfirmationServerCore.Models;
using Excel.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace EmailConfirmationServer.Controllers
{
    [Authorize]
    public class SpreadsheetController : Controller
    {
        private IEmailConfirmationContext context;
        private readonly IHostingEnvironment environment;
        private readonly IConfiguration configuration;
        private readonly ICreateSheet createSheet;

        public SpreadsheetController(IEmailConfirmationContext Context, IHostingEnvironment Environment, IConfiguration configuration,
            ICreateSheet createSheet)
        {
            this.context = Context;
            this.environment = Environment;
            this.configuration = configuration;
            this.createSheet = createSheet;
        }
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Upload()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uploads = context.FindUploadsByUserId(userId).ToList();

            if (uploads == null)
            {
                uploads = new List<SheetUpload>();
            }

            return View(uploads);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upload(IFormFile file)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);            
            var uploads = context.FindUploadsByUserId(userId).ToList();

            if(uploads == null)
            {
                uploads = new List<SheetUpload>();
            }
          
            if (file != null && file.Length > 0)
            {
                try
                {
                    string webRoot = environment.WebRootPath;
                    string path = Path.Combine(webRoot, Path.GetFileName(file.FileName));
                    await saveFileToRootFolder(path, file);
                                        
                    Spreadsheet spreadsheet = new Spreadsheet(path);                    
                    var upload = createNewUpload(userId, spreadsheet, file.FileName);
                    uploads.Add(upload);

                    context.Add<SheetUpload>(upload);
                    context.SaveChanges();

                    var emailService = new Models.EmailService(spreadsheet, configuration);
                    await emailService.sendConfirmationEmails();                                                           
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return View("Upload", uploads);
        }

        private async Task saveFileToRootFolder(string path, IFormFile file)
        {            
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
        }
     
        private SheetUpload createNewUpload(string userId, Spreadsheet sheet, string fileName)
        {            
            SheetUpload upload = new SheetUpload(userId, fileName);
            upload.People = sheet.People;
            return upload;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Download(int id)
        {
            var upload = GetUploadById(id);
            
            if(upload == null)
            {
                return RedirectToAction("Upload");
            }
                
            var people = upload.People;
                                   
            var memoryStream = new MemoryStream();
            createSheet.WriteToStream(people, memoryStream);
            memoryStream.Position = 0;

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var newFile = "EmailConfirmationAssisant_" + upload.Title;
            
            return File(memoryStream, contentType, newFile);
        }

        private SheetUpload GetUploadById(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uploads = context.FindUploadsByUserId(userId).ToList();

            if (uploads == null)
            {
                uploads = new List<SheetUpload>();
            }

            var upload = (from sheetUpload in uploads
                          where sheetUpload.Id == id
                          select sheetUpload).FirstOrDefault();

            return upload;
        }

        public ActionResult LoadSpreadsheet(int id, bool showConfirmed=false)
        {
            var upload = GetUploadById(id);         
            
            if (upload == null)
            {
                return RedirectToAction("Upload");
            }

            ViewBag.ShowConfirmed = showConfirmed;

            return View("_SpreadsheetPartial", upload);
        }
   
        [HttpPost]
        public ActionResult DeleteSpreadsheet(int id)
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uploads = context.FindUploadsByUserId(userId).ToList();

            var upload = (from sheetUpload in uploads
                          where sheetUpload.Id == id
                          select sheetUpload).FirstOrDefault();

            context.Delete(upload);
            context.SaveChanges();


            return RedirectToAction("Upload");
        }
    }
}