/// <reference path="../jquery-1.7.1.js" />

var ui = (function () {
    function buildProductsUI(data) {
        // render the products table

        var html = '<button class="btn btn-primary btn-lg" id="createProductView" onclick="requestProductCreateView(event)">Create a product</button><br/><br/>' +
            '<table class="table table-hover table-striped">' +
                        '<thead>' +
                            '<tr>' +
                                '<th>Image</th>' +
                                '<th>Id</th>' +
                                '<th>Name</th>' +
                                '<th>Category</th>' +
                                '<th>Description</th>' +
                                '<th>Edit</th>' +
                                '<th>Delete</th>' +
                            '</tr>' +
                         '</thead>' +
                         '<tbody>';

        // render row and column for each product 
        $.each(data, function (key, item) {

            if (item.image == null) {
                item.image = "ProductImages/default_product.png";
            }
            html += '<tr>';
            html += '<td class="col-md-1"> <img src="' + item.image + '"/></td>';
            html += '<td class="col-md-1">' + item.id + '</td>';
            html += '<td class="col-md-1">' + item.name + '</td>';
            html += '<td class="col-md-1">' + item.categoryName + '</td>';
            html += '<td class="col-md-4">' + item.description + '</td>';
            html += '<td class="col-md-1"> <button class="btn btn-primary" onclick="requestProductEditView(' + item.id + ', event)">Edit</button></td>';
            html += '<td class="col-md-1"> <button class="btn btn-danger" onclick="deleteProduct(' + item.id + ', event)">Delete</button></td>';
            html += "</tr>";
        });

        html += '</tbody></table>';

        return html;
    }

     function buildProductEditView(data) {
         var htmlEdit =
             '<form id="editProductForm" enctype="multipart/form-data">' +             

              '<div class="form-group">' +
              '<label for="name" class="col-sm-2 control-label">Name</label>' +
              '<div class="col-sm-10">' +
                  '<input type="text" name="name" id="name" class="form-control" value="' + data.name + '"><br>' +
              '</div>' +
              '</div>' +

              '<div class="form-group">' +
              '<label for="description" class="col-sm-2 control-label">Description</label>' +
              '<div class="col-sm-10">' +
                  '<input type="text" name="description" id="description" class="form-control" value="' + data.description + '"><br>' +
              '</div>' +
              '</div>' +

              '<div class="form-group">' +
              '<label for="uploadimage" class="col-sm-2 control-label">Upload image</label>' +
              '<div class="col-sm-10">' +
                  '<input type="file" id="uploadimage" class="form-control btn btn-default btn-file">' +
              '</div>' +
              '</div>' +

              '<div class="form-group">' +
              '<label for="category" class="col-sm-2 control-label">Select Category</label>' +
              '<div class="col-sm-10">' +
                  '<select id="category" class="form-control selectpicker">';

         // render all the categories in a drop down list
         $.each(data.allCategories, function (key, item) {
             // mark the category of the current product as selected
             if (data.categoryName == item.name) {
                 htmlEdit += '<option id="' + item.id + '" selected>' + item.name + '</option>';
             }
             else {
                 htmlEdit += '<option id="' + item.id + '">' + item.name + '</option>';
             }
         });

         htmlEdit += '</select>' +
                      '</div>' +
                     '</div>' +
                 '<input type="submit" id="btnEditProduct" onclick="editProduct(' + data.id + ')" class="btn btn-primary" value="Save product">' +
                 '<input type="submit" class="btn btn-danger" onclick="showAllProducts(event)" value="Cancel">';

         return htmlEdit;
     }

     function buildProductCreateView(data) {
         var htmlCreate =
             '<form id="createProductForm" enctype="application/x-www-form-urlencoded" >' +
              '<h3>Create a product</h3>' +

              '<div class="form-group">' +
              '<label for="productId" class="col-sm-2 control-label">Id</label>' +
              '<div class="col-sm-10">' +
                  '<input type="text" name="productId" id="productId" class="form-control"><br>' +
              '</div>' +
              '</div>' +

              '<div class="form-group">' +
              '<label for="name" class="col-sm-2 control-label">Name</label>' +
              '<div class="col-sm-10">' +
                  '<input type="text" name="name" id="name" class="form-control"><br>' +
              '</div>' +
              '</div>' +

              '<div class="form-group">' +
              '<label for="description" class="col-sm-2 control-label">Description</label>' +
              '<div class="col-sm-10">' +
                  '<input type="text" name="description" id="description" class="form-control"><br>' +
              '</div>' +
              '</div>' +

              '<div class="form-group">' +
              '<label for="uploadimage" class="col-sm-2 control-label">Upload image</label>' +
              '<div class="col-sm-10">' +
                  '<input type="file" id="uploadimage" class="form-control btn btn-default btn-file">' +
              '</div>' +
              '</div>' +

              '<div class="form-group">' +
              '<label for="category" class="col-sm-2 control-label">Select Category</label>' +
              '<div class="col-sm-10">' +
                  '<select id="category" class="form-control selectpicker">';

         // render all the categories in a drop down list
         $.each(data, function (key, item) {             
             htmlCreate += '<option id="' + item.id + '">' + item.name + '</option>';
         });

         htmlCreate += '</select>' +
                      '</div>' +
                     '</div>' +
                 '<input type="submit" id="btnCreateProduct" onclick="createProduct()" class="btn btn-primary" value="Save product">' +
                 '<input type="submit" class="btn btn-danger" onclick="showAllProducts(event)" value="Cancel">';

         return htmlCreate;
     }

     function buildCategories(data) {
         // render the products table
         var htmlCategoryAll = '<button class="btn btn-primary btn-lg" id="createCategoryView" onclick="requestCategoryCreateView()">Create a category</button>' + 
             '<table class="table table-hover table-striped">' +
                         '<thead>' +
                             '<tr>' +
                                 '<th>Name</th>' +
                                 '<th>Description</th>' +
                                 '<th>Edit</th>' +
                                 '<th>Delete</th>' +
                             '</tr>' +
                          '</thead>' +
                          '<tbody>';

         // render row and column for each product 
         $.each(data, function (key, item) {

             htmlCategoryAll += '<tr>';
             htmlCategoryAll += '<td class="col-md-1">' + item.name + '</td>';
             htmlCategoryAll += '<td class="col-md-4">' + item.description + '</td>';
             htmlCategoryAll += '<td class="col-md-1"> <button class="btn btn-primary" onclick="requestCategoryEditView(' + item.id + ')">Edit</button></td>';
             htmlCategoryAll += '<td class="col-md-1"> <button class="btn btn-danger" onclick="deleteCategory(' + item.id + ')">Delete</button></td>';
             htmlCategoryAll += "</tr>";
         });

         htmlCategoryAll += '</tbody></table>';

         return htmlCategoryAll;
     }

     function buildCategoryEditView(data) {
         var htmlEdit =
             '<form id="editCategoryForm">' +            
              '<div class="form-group">' +
              '<label for="name" class="col-sm-2 control-label">Name</label>' +
              '<div class="col-sm-10">' +
                  '<input type="text" name="name" id="name" class="form-control" value="' + data.name + '"><br>' +
              '</div>' +
              '</div>' +

              '<div class="form-group">' +
              '<label for="description" class="col-sm-2 control-label">Description</label>' +
              '<div class="col-sm-10">' +
                  '<input type="text" name="description" id="description" class="form-control" value="' + data.description + '"><br>' +
              '</div>' +
              '</div>' +

              
              '<input type="submit" id="btnEditCategory" onclick="editCategory(' + data.id + ')" class="btn btn-primary" value="Save category">' +
               '<button class="btn btn-danger" id="allCategories" onclick="showAllCategories(event)">Cancel</button>';

         return htmlEdit;
     }

     function buildCategoryCreateView() {
         var htmlcategoryCreate =
             '<form id="createCategoryForm">' +

             '<div class="form-group">' +
              '<label for="categoryId" class="col-sm-2 control-label">Id</label>' +
              '<div class="col-sm-10">' +
                  '<input type="text" name="categoryId" id="categoryId" class="form-control"><br>' +
              '</div>' +
              '</div>' +

              '<div class="form-group">' +
              '<label for="name" class="col-sm-2 control-label">Name</label>' +
              '<div class="col-sm-10">' +
                  '<input type="text" name="name" id="name" class="form-control"><br>' +
              '</div>' +
              '</div>' +

              '<div class="form-group">' +
              '<label for="description" class="col-sm-2 control-label">Description</label>' +
              '<div class="col-sm-10">' +
                  '<input type="text" name="description" id="description" class="form-control"><br>' +
              '</div>' +
              '</div>' +


              '<input type="submit" id="btnCreateCategory" onclick="createCategory()" class="btn btn-primary" value="Save category">' +
              '<button class="btn btn-danger" id="allCategories" onclick="showAllCategories(event)">Cancel</button>';
         
         return htmlcategoryCreate;
     }

     function buildSearchForm() {
         var searchform = '<div class="form-group">' +
                    '<h3>Search for products by ID, Name or Category</h3>' +
                    '<div>' +
                        '<input type="text" name="search" id="search" class="form-control"> <br />' +
                        '<input type="button" id="btnSearch" onclick="searchProducts()" class="btn btn-primary btn-lg" value="Search for products">' +
                    '</div>' +

                '</div>';

         return searchform;
     }

    return {
        productsUI: buildProductsUI,
        buildProductEditView: buildProductEditView,
        buildProductCreateView: buildProductCreateView,
        buildCategories: buildCategories,
        buildCategoryEditView: buildCategoryEditView,
        buildCategoryCreateView: buildCategoryCreateView,
        buildSearchForm: buildSearchForm
    }
}());