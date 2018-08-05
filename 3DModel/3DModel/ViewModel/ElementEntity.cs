using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DModel.ViewModel
{
    public class ElementEntity
    {
        public ElementEntity() { }

        public Guid Id { get; set; }

        public string ElementName { get; set; }

        public string ElementDescription { get; set; }

        public string  ElementParent { get; set; }

        public Guid? ParentId { get; set; }


    }
}
