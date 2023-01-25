using Microsoft.AspNetCore.Mvc;
using MongoAndMVC.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace WebApplication3.Controllers
{
    public class GalleryController : Controller
    {
        public ActionResult Index()
        {
            var theModel = GetThePictures();
            return View(theModel);
        }

        public ActionResult AddPicture()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPicture(HttpPostedFileBase theFile)
        {
            if (theFile.ContentLength > 0)
            {
                
                string theFileName = Path.GetFileName(theFile.FileName);

                
                byte[] thePictureAsBytes = new byte[theFile.ContentLength];
                using (BinaryReader theReader = new BinaryReader(theFile.InputStream))
                {
                    thePictureAsBytes = theReader.ReadBytes(theFile.ContentLength);
                }


                string thePictureDataAsString = Convert.ToBase64String(thePictureAsBytes);

          
                MongoPictureModel thePicture = new MongoPictureModel()
                {
                    FileName = theFileName,
                    PictureDataAsString = thePictureDataAsString
                };

         
                bool didItInsert = InsertPictureIntoDatabase(thePicture);

                if (didItInsert)
                    ViewBag.Message = "The image was updated successfully";
                else
                    ViewBag.Message = "A database error has occurred";
            }
            else
                ViewBag.Message = "You must upload an image";

            return View();
        }

     
        private bool InsertPictureIntoDatabase(MongoPictureModel thePicture)
        {
            var thePictureColleciton = GetPictureCollection();
            var theResult = thePictureColleciton.Insert(thePicture);
            return theResult.OK;
        }

       
        private List<MongoPictureModel> GetThePictures()
        {
            var thePictureColleciton = GetPictureCollection();
            var thePictureCursor = thePictureColleciton.FindAll();


            thePictureCursor.SetFields(Fields.Include("_id", "FileName"));

            return thePictureCursor.ToList() ?? new List<MongoPictureModel>();
        }

       
        public FileContentResult ShowPicture(string id)
        {
            var thePictureColleciton = GetPictureCollection();

    
            var thePicture = thePictureColleciton.FindOneById(new ObjectId(id));

          
            var thePictureDataAsBytes = Convert.FromBase64String(thePicture.PictureDataAsString);

            
            return new FileContentResult(thePictureDataAsBytes, "image/jpeg");
        }

       
        private MongoCollection<MongoPictureModel> GetPictureCollection()
        {
          
            var theConnectionString = "mongodb://localhost";

           
            var theDBClient = new MongoClient(theConnectionString);

      
            var theServer = theDBClient.GetServer();

           
            string databaseName = "PictureApplication";
            var thePictureDB = theServer.GetDatabase(databaseName);

            
            string theCollectionName = "pictures";
            var thePictureColleciton = thePictureDB.GetCollection<MongoPictureModel>(theCollectionName);

            return thePictureColleciton;
        }
    }
}