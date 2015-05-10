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

namespace ProductsCatalog.Controllers
{
    public class CategoriesController : ApiController
    {
        private ProductsCatalogEntities db = new ProductsCatalogEntities();

        // GET api/Categories
        public IOrderedQueryable<CategoryViewModel> GetCategories()
        {            
            var categoriesModel = AllCategories(db);

            return categoriesModel;
        }

        // GET api/Categories/5
        public CategoryViewModel GetCategory(int id)
        {
            //Category category = db.Categories.Find(id);

            var categoryEditViewModel = db.Categories
                .Where(c => c.Id == id)
                .Select(c =>
                    new CategoryViewModel()
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description                        
                    }
                ).FirstOrDefault();

            if (categoryEditViewModel == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return categoryEditViewModel;
        }

        // PUT api/Categories/5
        [System.Web.Http.AcceptVerbs("GET", "PUT")]
        [System.Web.Http.HttpPut]
        public HttpResponseMessage PutCategory([FromBody] CategoryViewModel category)
        {
            if (ModelState.IsValid)
            {
                //this.ValidateProductCreateModel(category);

                Category editedCategory = db.Categories.Where(c => c.Id == category.Id).FirstOrDefault();

                if (editedCategory != null)
                {
                    editedCategory.Name = category.Name;
                    editedCategory.Description = category.Description;
                }

                db.Entry(editedCategory).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // POST api/Categories
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpPost]
        public string PostCategory([FromBody] CategoryViewModel category)
        {
            string result = "";
            
            if (ModelState.IsValid)
            {
                //this.ValidateProductCreateModel(category);

                var newCategory = db.Categories.Where(c => c.Id == category.Id).FirstOrDefault();

                if (newCategory == null)
                {
                    newCategory = new Category();
                    newCategory.Id = category.Id;
                    newCategory.Name = category.Name;
                    newCategory.Description = category.Description;

                    db.Categories.Add(newCategory);
                    db.SaveChanges();

                    result = "The category was successfully created";

                    //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, category);
                    //response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = category.Id }));
                }
                else
                {
                    //return Request.CreateResponse(HttpStatusCode.BadRequest);

                    result = "Category with the same id already exists.";
                }

               // return Request.CreateResponse(HttpStatusCode.Created, category);
            }            

            return result;
        }

        // DELETE api/Categories/5
        [System.Web.Http.AcceptVerbs("GET", "DELETE")]
        [System.Web.Http.HttpDelete]
        public string DeleteCategory(int id)
        {
            string result = "";

            Category category = db.Categories.Where(c => c.Id == id).FirstOrDefault();

            if (category == null)
            {
                result = "The category cannot be found";
            }
            else
            {
                var productsWhereCategoryIsUsed = db.Products.Where(p => p.CategoryId == id).ToList();

                if (productsWhereCategoryIsUsed.Count() == 0)
                {
                    db.Categories.Remove(category);
                    db.SaveChanges();

                    result = "The category " + category.Name + " is successfully deleted";
                }
                else
                {
                    result = "Error: The category " + category.Name + " cannot be deleted since it is applied to at least one product";
                }
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private static IOrderedQueryable<CategoryViewModel> AllCategories(ProductsCatalogEntities db)
        {
            var categoriesModel = db.Categories
                .Select(category =>
                    new CategoryViewModel()
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Description = category.Description                        
                    }
                );

            return categoriesModel.OrderBy(c => c.Name);
        }

        //private void ValidateProductCreateModel(CategoryViewModel model)
        //{
        //    if (model == null)
        //    {
        //        throw new ServerErrorException(
        //            "Category cannot be null",
        //            HttpStatusCode.BadRequest);
        //    }

        //    if (string.IsNullOrEmpty(model.Name))
        //    {
        //        throw new ServerErrorException(
        //            "Category name must be set",
        //            HttpStatusCode.BadRequest);
        //    }
        //}
    }
}