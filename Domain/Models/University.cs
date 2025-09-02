using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    [Table ("Tbl_University")]
    public class University
    {
        public Guid UniversityId { get; set; }
        public string? Domains { get; set; }
        public string? WebPages {  get; set; }
        public string? Name { get; set; }
        public string? CountryCode { get; set; }
        public string? StateProvince { get; set; }
    }
}
