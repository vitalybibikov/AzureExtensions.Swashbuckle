using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestFunction.Models
{
    public class TestModel
    {
        /// <summary>
        ///     Id
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        ///     Name
        /// </summary>
        [Required]
        [MaxLength(512)]
        public string Name { get; set; }

        /// <summary>
        ///     Description
        /// </summary>
        [MaxLength(10240)]
        public string Description { get; set; }
    }
}
