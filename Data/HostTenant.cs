using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce_API_RST_Multi.Data
{
    public class HostTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Tenant Id")]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Connstr { get; set; }
        public bool? Removeable { get; set; }
        public int? InstallDbId { get; set; }
        public int? Creator { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

    }
}
