using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace ProductsCatalog.Models
{
    [DataContract(IsReference = true)]
    public class ProductViewModel
    {
        //[Required]
        [DataMember(Name = "id")]
        public int Id { get; set; }

        //[Required]
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "image")]
        public string Image { get; set; }

        //[Required]
        [DataMember(Name = "categoryName")]
        public string CategoryName { get; set; }        
    }
}