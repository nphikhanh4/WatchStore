//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApplication1.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Images_Certification
    {
        public int ImageID { get; set; }
        public int BrandID { get; set; }
        public string URL_Images_Certification { get; set; }
    
        public virtual Brand Brand { get; set; }
    }
}
