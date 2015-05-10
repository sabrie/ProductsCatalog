using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ProductsCatalog.Models
{
    [DataContract(IsReference = true)]
    public class ProductEditViewModel : ProductViewModel
    {       
        [DataMember(Name = "allCategories")]
        public IEnumerable<CategoryViewModel> AllCategories { get; set; }

        [DataMember(Name = "categoryId")]
        public int CategoryId { get; set; }
    }
}