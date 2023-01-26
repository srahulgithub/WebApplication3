using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using System.Drawing;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AddImage()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddImage(UploadImage img)
        {
            MongoClient Client = new MongoClient("mongodb://localhost:27017");
            var db = Client.GetDatabase("ImageDB");
            var doc = new UploadImage();
            doc.Title = img.Title;
            doc.Description = img.Description;
            doc.Image = img.Image;
            doc.Id = img.Id;
            IGridFSBucket bucket = new GridFSBucket(db);
            var options = new GridFSUploadOptions
            {
                ChunkSizeBytes = 64512, // 63KB
                Metadata = new BsonDocument
    {
        { "resolution", "1080P" },
        { "copyrighted", true }
    }
            };

            using var stream = await bucket.OpenUploadStreamAsync(img.Title, options);
            var id = stream.Id;
            doc.Image.CopyTo(stream);
            await stream.CloseAsync();

            return View("Index");

        }
       


    }
}
