using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace MyPhotoGallery.Models
{
    public partial class Photos
    {
        public int Id { get; set; }
        public string Email { get; set; }

        [DisplayName("Category")]
        public string Title { get; set; }

        [DisplayName("Title")]
        public string PhotoName { get; set; }
        public DateTime? Date { get; set; }


        [NotMapped]
        [DisplayName("Upload File")]
        public IFormFile PhotoFile { get; set; }
    }
}
