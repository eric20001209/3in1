using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace gpos_api_3in1_multi_tenant.Data
{
    public class WebApp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "App Id")]
        public int Id { get; set; }
        public int DbId { get; set; }
        public string Url { get; set; }
    }
}
