using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestFunction.Models
{
    public class TestModel
    {
        /// <summary>
        ///     Id (example)
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        ///     Name (example)
        /// </summary>
        [Required]
        [MaxLength(512)]
        public string Name { get; set; }

        /// <summary>
        ///     Description (example)
        /// </summary>
        [MaxLength(10240)]
        public string Description { get; set; }
    }
}
