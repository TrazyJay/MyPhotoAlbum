using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyPhotoGallery.Models;

namespace MyPhotoGallery.Controllers
{
    [Authorize]
    public class PhotosController : Controller
    {
        private readonly PhotoDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private IHostingEnvironment _oIHostingEnvironment;

        public PhotosController(PhotoDbContext context, IWebHostEnvironment hostEnvironment, IHostingEnvironment oIHostingEnvironment)
        {
            _context = context;
            this._hostEnvironment = hostEnvironment;
            _oIHostingEnvironment = oIHostingEnvironment;
        }

        // GET: Photos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Photos.ToListAsync());
        }

        // GET: Photos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photos = await _context.Photos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (photos == null)
            {
                return NotFound();
            }

            return View(photos);
        }

        // GET: Photos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Photos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,Title,PhotoFile,Date")] Photos photos)
        {
            if (ModelState.IsValid)
            {
                //Save image to wwwroot/image
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(photos.PhotoFile.FileName);
                string extension = Path.GetExtension(photos.PhotoFile.FileName);
                photos.PhotoName = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string path = Path.Combine(wwwRootPath + "/uploads/", fileName);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await photos.PhotoFile.CopyToAsync(fileStream);
                }

                //Insert record
                _context.Add(photos);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(photos);
        }

        // GET: Photos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photos = await _context.Photos.FindAsync(id);
            if (photos == null)
            {
                return NotFound();
            }
            return View(photos);
        }

        // POST: Photos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,Title,PhotoName,Date")] Photos photos)
        {
            if (id != photos.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(photos);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhotosExists(photos.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(photos);
        }

        // GET: Photos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photos = await _context.Photos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (photos == null)
            {
                return NotFound();
            }

            return View(photos);
        }

        // POST: Photos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var photos = await _context.Photos.FindAsync(id);

            //delete image from wwwroot/image
            var imagePath = Path.Combine(_hostEnvironment.WebRootPath,"uploads",photos.PhotoName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);

            //delete the record
            _context.Photos.Remove(photos);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhotosExists(int id)
        {
            return _context.Photos.Any(e => e.Id == id);
        }

        public FileResult GenerateAndDownloadZip()
        {
            var webRoot = _oIHostingEnvironment.WebRootPath;
            var fileName = "MyZip.zip";
            var tempOutput = webRoot + "/uploads/" + fileName;

            using (ZipOutputStream oZipOutputStream = new ZipOutputStream(System.IO.File.Create(tempOutput)))
            {
                oZipOutputStream.SetLevel(9);

                byte[] buffer = new byte[4096];

                var ImageList = new List<string>();

                ImageList.Add(webRoot + "/display/Friends.jpg");
                ImageList.Add(webRoot + "/display/Lion.jpg");
                ImageList.Add(webRoot + "/display/Man City.jpg");
                ImageList.Add(webRoot + "/display/Nature.jpg");
                ImageList.Add(webRoot + "/display/Nikon.jpg");

                for (int i = 0; i < ImageList.Count; i++)
                {
                    ZipEntry entry = new ZipEntry(Path.GetFileName(ImageList[i]));
                    entry.DateTime = DateTime.Now;
                    entry.IsUnicodeText = true;
                    oZipOutputStream.PutNextEntry(entry);

                    using (FileStream oFileStream = System.IO.File.OpenRead(ImageList[i]))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = oFileStream.Read(buffer, 0, buffer.Length);
                            oZipOutputStream.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }
                oZipOutputStream.Finish();
                oZipOutputStream.Flush();
                oZipOutputStream.Close();
            }
            byte[] finalResult = System.IO.File.ReadAllBytes(tempOutput);
            if(System.IO.File.Exists(tempOutput))
            {
                System.IO.File.Delete(tempOutput);
            }

            if (finalResult == null || finalResult.Any())
            {
                throw new Exception(String.Format("Nothing Found"));
            }

            return File(finalResult, "application/zip", fileName);
        }
    }
}
