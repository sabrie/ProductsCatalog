using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ProductsCatalog.Data;
using System.Net.Http.Headers;
using ProductsCatalog.Models;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Data.Objects.SqlClient;
using ProductsCatalog.Helpers;
using System.Collections.Specialized;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace ProductsCatalog.Controllers
{
    public class ProductsController : ApiController
    {
        private ProductsCatalogEntities db = new ProductsCatalogEntities();
        private string imageFolderVirtualPath = @"~\ProductImages\";
        private string imageSrcFolder = @"ProductImages/";
        private string defaultImageSrc = @"ProductImages/default_product.png";
        private string defaultImageFileName = "default_product.png";

        // GET api/Products
        public IOrderedQueryable<ProductViewModel> GetProducts()
        {      
            var productsModel = AllProducts(db);

            return productsModel;
        }

        // GET api/Products/GetProductsBy?search=
        public IOrderedQueryable<ProductViewModel> GetProductsBy(string search)
        {
            if (String.IsNullOrEmpty(search))
            {
                return AllProducts(db);
            }

            var searchTerms = search.ToLower();

            var productsModelFiltered = AllProducts(db)
                .Where(p => p.Name.ToLower().Contains(searchTerms) ||
                p.CategoryName.ToLower().Contains(searchTerms) ||
                SqlFunctions.StringConvert((double)p.Id).Contains(searchTerms));

            if (productsModelFiltered == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return productsModelFiltered.OrderBy(pm => pm.Name);
        }

        // GET api/Products/5
        //[HttpGet]
        public ProductEditViewModel GetProduct(int id)
        {
            //Product product = db.Products.Find(id);
            
            //var allCategories = from c in db.Categories
            //               select new CategoryViewModel
            //               {
            //                   Id = c.Id,
            //                   Name = c.Name,
            //               };

            //var productEditViewModel = from p in db.Products
            //                  where p.Id == id
            //                  join cat in db.Categories on p.CategoryId equals cat.Id
            //                  select new ProductEditViewModel 
            //                  {
            //                      Id = p.Id,
            //                      Name = p.Name,
            //                      Description = p.Description,
            //                      Image = p.Image,
            //                      CategoryName = p.Category.Name,
            //                      AllCategories = allCategories
            //                  };

            var productEditViewModel = db.Products
                .Where(p => p.Id == id)
                .Select(p =>
                    new ProductEditViewModel()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Image = p.Image,
                        CategoryName = p.Category.Name,
                        AllCategories = db.Categories
                        .Select(category =>
                            new CategoryViewModel()
                            {
                                Id = category.Id,
                                Name = category.Name,
                            }
                        )
                    }
                ).FirstOrDefault();

            if (productEditViewModel == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return productEditViewModel;
        }

        // PUT api/Products/5
        [System.Web.Http.AcceptVerbs("GET", "PUT")]
        [System.Web.Http.HttpPut]
        public async Task<HttpResponseMessage> PutProduct(string id)
        {
            if (Request.Content.IsMimeMultipartContent("form-data"))
            {
                if (Request.Content.IsMimeMultipartContent())
                {
                    // Read the file and form data.
                    var provider = new MultipartFormDataMemoryStreamProvider();
                    await Request.Content.ReadAsMultipartAsync(provider);

                    string imageFileName = ReadFilesFromFormDataAndUploadIfAny(provider);

                    // Extract the other fields from the form data.
                    string name = provider.FormData["name"];
                    string description = provider.FormData["description"];
                    string categoryIdString = provider.FormData["categoryId"];

                    if (!String.IsNullOrEmpty(name) &&
                        !String.IsNullOrEmpty(categoryIdString))
                    {
                        // TODO: additional exception handling 
                        int idParsed = Convert.ToInt32(id);
                        int categoryId = Convert.ToInt32(categoryIdString);

                        Product editedProduct = db.Products.Where(p => p.Id == idParsed).FirstOrDefault();

                        if (editedProduct != null)
                        {
                            editedProduct.Name = name;
                            editedProduct.Description = description;
                            editedProduct.CategoryId = categoryId;

                            string currentImage = editedProduct.Image;

                            // if there is a new image selected, delete the old image from file system 
                            // if it is not the default image
                            if (currentImage != this.defaultImageFileName && imageFileName != this.defaultImageFileName)
                            {
                                DeleteImageFromFileSystem(currentImage);
                            }

                            // if a new image is uploaded for the product
                            // set the product image to it
                            if (imageFileName != this.defaultImageFileName)
                            {
                                editedProduct.Image = imageFileName;
                            }

                            //db.Products.Add(product);
                            db.Entry(editedProduct).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error: The product cannot be found");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Error: The following fields are required: Name, Category");
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Product has been successfully edited");
        }

        // POST api/Products
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> PostProduct()
        {
            if (Request.Content.IsMimeMultipartContent("form-data"))
            {
                if (Request.Content.IsMimeMultipartContent())
                {
                    // Read the file and form data.
                    var provider = new MultipartFormDataMemoryStreamProvider();
                    await Request.Content.ReadAsMultipartAsync(provider);

                    string imageFileName = ReadFilesFromFormDataAndUploadIfAny(provider);
                    
                    // Extract the other fields from the form data.
                    string idString = provider.FormData["id"];
                    string name = provider.FormData["name"];
                    string description = provider.FormData["description"];
                    string categoryIdString = provider.FormData["categoryId"];

                    if (!String.IsNullOrEmpty(idString) &&
                        !String.IsNullOrEmpty(name) &&
                        !String.IsNullOrEmpty(categoryIdString))
                    {
                        // TODO: additional exception handling 
                        int id = Convert.ToInt32(idString);
                        int categoryId = Convert.ToInt32(categoryIdString);

                        Product newProduct = db.Products.Where(p => p.Id == id).FirstOrDefault();

                        if (newProduct == null)
                        {
                            newProduct = new Product();
                            newProduct.Id = id;
                            newProduct.Name = name;
                            newProduct.Description = description;
                            newProduct.CategoryId = categoryId;
                            newProduct.Image = imageFileName;

                            db.Products.Add(newProduct);
                            db.SaveChanges();
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Error: A product with the same Id already exists");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Error: The following fields are required: Id, Name, Category");
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Product has been successfully created");
        }

        // DELETE api/Products/5
        public HttpResponseMessage DeleteProduct(int id)
        {
            Product product = db.Products.Where(p => p.Id == id).FirstOrDefault();

            if (product == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Error: The product could not be found and could not be deleted!");
            }
            else
            {
                string imageFile = product.Image;

                if (imageFile != this.defaultImageFileName)
                {
                    DeleteImageFromFileSystem(imageFile);
                }

                db.Products.Remove(product);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "The product has been successfully deleted!");
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private static IOrderedQueryable<ProductViewModel> AllProducts(ProductsCatalogEntities db)
        {
            //var products = from p in db.Products.Include(p => p.Category)
            //               select new ProductViewModel
            //               {
            //                   Id = p.Id,
            //                   Name = p.Name,
            //                   Description = p.Description,
            //                   Image = p.Image,
            //                   CategoryName = p.Category.Name
            //               };

            //return products.AsEnumerable();

            var productsModel = db.Products
                .Select(product =>
                    new ProductViewModel()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Image = product.Image,
                        CategoryName = product.Category.Name
                    }
                );

            return productsModel.OrderBy(pm => pm.Name);
        }

        private string ReadFilesFromFormDataAndUploadIfAny(MultipartFormDataMemoryStreamProvider provider)
        {
            string imageFileName = this.defaultImageFileName;

            // Check if files are on the request.
            if (provider.FileStreams.Any())
            {
                foreach (KeyValuePair<string, Stream> file in provider.FileStreams)
                {
                    var fileName = file.Key;
                    var stream = file.Value;

                    string virtualPath = UploadFile(stream, fileName);

                    if (!String.IsNullOrEmpty(virtualPath))
                    {
                        imageFileName = fileName;
                    }
                }
            }

            return imageFileName;
        }

        public string UploadFile(Stream stream, string fileName)
        {
            var virtualPath = this.imageFolderVirtualPath + fileName;
            var filepath = HttpContext.Current.Server.MapPath(virtualPath);

            var path = Path.GetDirectoryName(filepath);

            if (path != null && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (Stream file = File.OpenWrite(filepath))
            {
                CopyStream(stream, file);
            }

            return virtualPath;
        }

        public static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        private void DeleteImageFromFileSystem(string imageName)
        {
            var virtualPath = this.imageFolderVirtualPath + imageName;
            var filepath = HttpContext.Current.Server.MapPath(virtualPath);

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        //private void ValidateProductCreateModel(ProductEditViewModel model)
        //{
        //    if (model == null)
        //    {
        //        throw new ServerErrorException(
        //            "Product cannot be null",
        //            HttpStatusCode.BadRequest);
        //    }

        //    if (string.IsNullOrEmpty(model.Name))
        //    {
        //        throw new ServerErrorException(
        //            "Product name must be set",
        //            HttpStatusCode.BadRequest);
        //    }

        //    if (string.IsNullOrEmpty(model.CategoryName))
        //    {
        //        throw new ServerErrorException(
        //            "Product category must be set",
        //            HttpStatusCode.BadRequest);
        //    }
        //}
    }
}