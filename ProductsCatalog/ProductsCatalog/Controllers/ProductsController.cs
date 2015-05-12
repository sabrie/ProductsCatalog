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

        // PUT api/Products/5 FormDataCollection 
        [System.Web.Http.AcceptVerbs("GET", "PUT")]
        [System.Web.Http.HttpPut]
        public string PutProduct([FromBody] ProductEditViewModel product)
        {
            string result = "";
            if (ModelState.IsValid)
            {
                //this.ValidateProductCreateModel(product);

                Product editedProduct = new Product();

                editedProduct.Id = product.Id;
                editedProduct.Name = product.Name;
                editedProduct.Description = product.Description;
                editedProduct.CategoryId = product.CategoryId;
                //editedProduct.Image = product.Image;

                //if (HttpContext.Current.Request.Files.AllKeys.Any())
                //{
                //    // Get the uploaded image from the Files collection
                //    var httpPostedFile = HttpContext.Current.Request.Files["image"];

                //    if (httpPostedFile != null)
                //    {
                //        // Validate the uploaded image(optional)

                //        // Get the complete file path
                //        var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/ProductImages"), httpPostedFile.FileName);

                //        // Save the uploaded file to "UploadedFiles" folder
                //        httpPostedFile.SaveAs(fileSavePath);
                //    }
                //}


                db.Entry(editedProduct).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    result = "An error occured while editing the product - from the WEB API";
                }

                result = "The product was successfully edited";
            }
            else
            {
                result = "An error occured while editing the product - from the WEB API";
            }

            return result;
        }

        public string UploadFile(Stream stream, string fileName)
        {
            var virtualPath = @"~\ProductImages\" + fileName;
            var filepath = HttpContext.Current.Server.MapPath(virtualPath);// + fileInfo.FileName);

            var path = Path.GetDirectoryName(filepath);

            if (path != null && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //using (Stream file = File.OpenWrite(filepath))
            //{
                int length = 256;
                int bytesRead = 0;
                Byte[] buffer = new Byte[length];

                // write the required bytes
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    do
                    {
                        bytesRead = stream.Read(buffer, 0, length);
                        fs.Write(buffer, 0, bytesRead);
                    }
                    while (bytesRead == length);
                }

                stream.Dispose();
                
               //System.Data.Common.Utils.CopyStream(stream, file);
            //}
            return filepath;
        }

        // POST api/Products
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> PostProduct()
        {
            if (Request.Content.IsMimeMultipartContent("form-data"))
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                // Read the file and form data.
                var provider = new MultipartFormDataMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // Check if files are on the request.
                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                // Extract the fields from the form data.
                string idString = provider.FormData["id"];
                string name = provider.FormData["name"];
                string description = provider.FormData["description"];
                string categoryIdString = provider.FormData["categoryId"];
                string imagePath = "";

                // TODO: exception handling 
                int id = Convert.ToInt32(idString);
                int categoryId = Convert.ToInt32(categoryIdString);

                IDictionary<string, string> uploadedFiles = new Dictionary<string, string>();
                foreach (KeyValuePair<string, Stream> file in provider.FileStreams)
                {
                    var fileName = file.Key;
                    var stream = file.Value;

                    imagePath = UploadFile(stream, fileName);


                    // Keep track of the filename for the response
                    uploadedFiles.Add(fileName, "Result");
                }

                Product newProduct = db.Products.Where(p => p.Id == id).FirstOrDefault();

                if (newProduct == null)
                {
                    newProduct = new Product();
                    newProduct.Id = id;
                    newProduct.Name = name;
                    newProduct.Description = description;
                    newProduct.CategoryId = categoryId;
                    newProduct.Image = imagePath;

                    db.Products.Add(newProduct);
                    db.SaveChanges();                   
                }
                else
                {
                    Request.CreateResponse(HttpStatusCode.BadRequest, "A product with the same Id already exists");
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Product is successfully created");
        }

        // DELETE api/Products/5
        public string DeleteProduct(int id)
        {
            string result = "";
            
            Product product = db.Products.Where(p => p.Id == id).FirstOrDefault();

            if (product == null)
            {
                result = "The product cannot be found";
            }
            else
            {
                db.Products.Remove(product);
                db.SaveChanges();

                result = "The product " + product.Name + " is successfully deleted";
            }

            return result;
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