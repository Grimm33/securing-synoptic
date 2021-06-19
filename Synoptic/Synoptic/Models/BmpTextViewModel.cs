using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Synoptic.Models
{
    public class BmpTextViewModel
    {
        public IFormFile Image { get; set; }

        public string Text { get; set; }

        public string Password { get; set; }


    }
}
