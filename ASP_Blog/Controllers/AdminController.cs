﻿using ASP_Blog.Models.Home;
using ASP_Blog.Models.Service;
using ASP_Blog.ViewModels.Admin;
using ASP_Blog.Models.Identity;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ASP_Blog.Controllers
{
    public class AdminController : Controller
    {
        WebsiteContext websiteDB;
        IWebHostEnvironment _appEnvironment;
        private readonly UserManager<User> _userManager;

        public AdminController(WebsiteContext webContext, IWebHostEnvironment appEnvironment,UserManager<User> userManager)
        {
            websiteDB = webContext;
            _appEnvironment = appEnvironment;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddNews()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNews(AddNewsViewModel model, IFormFileCollection uploads)
        {
            // Проверяем размер каждого изображения
            foreach (var image in uploads)
            {
                if (image.Length > 2097152)
                {
                    ModelState.AddModelError("Image", "Размер изображения должен быть не более 2МБ.");
                    break;
                }
            }

            if (ModelState.IsValid)
            {
                News news = new News()
                {
                    Id = Guid.NewGuid(),
                    NewsTitle = model.NewsTitle,
                    NewsBody = model.NewsBody,
                    NewsDate = DateTime.Now,
                    UserName = User.Identity.Name
                };

                // Создаем список изображений
                List<ImageFile> images = new List<ImageFile>();
                foreach (var uploadedImage in uploads)
                {
                    // Присваиваем загружаемому файлу уникальное имя на основе Guid
                    string imageName = Guid.NewGuid() + "_" + uploadedImage.FileName;
                    // Путь сохранения файла
                    string pathNormal = "/files/images/normal/" + imageName; // изображение исходного размера
                    string pathScaled = "/files/images/scaled/" + imageName; // уменьшенное изображение
                    // сохраняем файл в папку files в каталоге wwwroot
                    using (FileStream file = new FileStream(_appEnvironment.WebRootPath + pathNormal, FileMode.Create))
                    {
                        await uploadedImage.CopyToAsync(file);
                    }
                    // Создаем объект класса Image со всеми параметрами
                    ImageFile image = new ImageFile { Id = Guid.NewGuid(), ImageName = imageName, ImagePathNormal = pathNormal, ImagePathScaled = pathScaled, TargetId = news.Id };
                    // Добавляем объект класса Image в ранее созданный список images
                    images.Add(image);

                    // Делаем уменьшенную копию изображения
                    var img = Image.FromFile(_appEnvironment.WebRootPath + pathNormal);
                    var scaledImage = ImageResize.Scale(img, 300, 300);
                    scaledImage.SaveAs(_appEnvironment.WebRootPath + pathScaled, 50);
                }
                // Сохраняем новые объекты в БД
                await websiteDB.Images.AddRangeAsync(images);
                await websiteDB.News.AddAsync(news);
                await websiteDB.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }

            // Редирект на случай невалидности модели
            return View(model);
        }

        public async Task<IActionResult> DeleteNews(Guid newsId)
        {
            News news = new News()
            {
                Id = newsId
            };

            List<ImageFile> images = await websiteDB.Images.Where(i => i.TargetId == newsId).ToListAsync();

            websiteDB.Images.RemoveRange(images);
            websiteDB.News.Remove(news);
            await websiteDB.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AddGallery()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddGallery(AddGalleryViewModel model)
        {
            if (ModelState.IsValid)
            {
                Gallery gallery = new Gallery()
                {
                    Id = Guid.NewGuid(),
                    GalleryTitle = model.GalleryTitle,
                    GalleryDescription = model.GalleryDescription,
                    GalleryDate = DateTime.Now
                };

                await websiteDB.Galleries.AddAsync(gallery);
                await websiteDB.SaveChangesAsync();

                return RedirectToAction("Gallery", "Home", new { galleryId = gallery.Id });
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteGallery(Guid galleryId)
        {
            Gallery gallery = new Gallery()
            {
                Id = galleryId
            };

            List<ImageFile> images = await websiteDB.Images.Where(i => i.TargetId == galleryId).ToListAsync();

            websiteDB.Images.RemoveRange(images);
            websiteDB.Galleries.Remove(gallery);
            await websiteDB.SaveChangesAsync();

            return RedirectToAction("Galleries", "Home");
        }

        public async Task<IActionResult> AddImageToGallery(Guid galleryId, IFormFileCollection uploads)
        {
            // Проверяем размер каждого изображения
            foreach (var image in uploads)
            {
                if (image.Length > 2097152)
                {
                    ModelState.AddModelError("Image", "Размер изображения должен быть не более 2МБ.");
                    break;
                }
            }

            List<ImageFile> images = new List<ImageFile>();
            foreach (var uploadedImage in uploads)
            {
                // Присваиваем загружаемому файлу уникальное имя на основе Guid
                string imageName = Guid.NewGuid() + "_" + uploadedImage.FileName;
                // Путь сохранения файла
                string pathNormal = "/files/images/normal/" + imageName; // изображение исходного размера
                string pathScaled = "/files/images/scaled/" + imageName; // уменьшенное изображение
                // сохраняем файл в папку files в каталоге wwwroot
                using (FileStream file = new FileStream(_appEnvironment.WebRootPath + pathNormal, FileMode.Create))
                {
                    await uploadedImage.CopyToAsync(file);
                }
                // Создаем объект класса Image со всеми параметрами
                ImageFile image = new ImageFile { Id = Guid.NewGuid(), ImageName = imageName, ImagePathNormal = pathNormal, ImagePathScaled = pathScaled, TargetId = galleryId };
                // Добавляем объект класса Image в ранее созданный список images
                images.Add(image);

                // Делаем уменьшенную копию изображения
                var img = Image.FromFile(_appEnvironment.WebRootPath + pathNormal);
                var scaledImage = ImageResize.Scale(img, 300, 300);
                scaledImage.SaveAs(_appEnvironment.WebRootPath + pathScaled, 50);
            }

            await websiteDB.Images.AddRangeAsync(images);
            await websiteDB.SaveChangesAsync();

            return RedirectToAction("Gallery", "Home", new { galleryId });
        }

        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            Comment news = new Comment()
            {
                Id = commentId
            };

            websiteDB.Comments.Remove(news);
            await websiteDB.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> DeleteUser(string userid)
        {
            User news = await _userManager.FindByIdAsync(userid);


            await _userManager.DeleteAsync(news);


            return RedirectToAction("Index", "Home");
        }
    }
}
