using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarroAPI.Models
{
    public class ItemDto
    {
        public int Code { get; set; }

        // Description is the column called "name" in "code_relations" table
        public string Description { get; set; }

        // code_relations.name_cn
        public string Name { get; set; }

        // code_relations.cat
        public string Category { get; set; }
    }
}
