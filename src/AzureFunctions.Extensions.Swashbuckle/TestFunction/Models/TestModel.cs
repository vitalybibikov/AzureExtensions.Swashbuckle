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
        /// <example>3</example>
        [Required]
        public int Id { get; set; }

        /// <summary>
        ///     Name (example)
        /// </summary>
        /// <example>John</example>
        [Required]
        [MaxLength(512)]
        public string Name { get; set; }

        /// <summary>
        ///     Description (example)
        /// </summary>
        /// <example>Sometimes human</example>
        [MaxLength(10240)]
        public string Description { get; set; }
    }
}
