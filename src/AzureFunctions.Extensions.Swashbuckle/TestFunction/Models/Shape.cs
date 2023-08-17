using System;
using System.ComponentModel.DataAnnotations;

namespace TestFunction.Models
{
    /// <summary>
    /// An abstract class for different shapes
    /// <remarks>
    /// $type: ["Circle", "Square", "Rectangle"]
    /// </remarks>
    /// </summary>
    public abstract class Shape
    {
        /// <summary>
        /// A globally unique identifier
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Circle
    /// <remarks>
    ///  $type: "Circle"
    /// </remarks>
    /// </summary>
    public class Circle : Shape
    {
        public double Radius { get; set; }
    }

    /// <summary>
    /// Square
    /// <remarks>
    ///  $type: "Square"
    /// </remarks>
    /// </summary>
    public class Square : Shape
    {
        public double Side { get; set; }
    }

    /// <summary>
    /// Square
    /// <remarks>
    ///  $type: "Rectangle"
    /// </remarks>
    /// </summary>
    public class Rectangle : Shape
    {
        public double Length { get; set; }
        public double Breadth { get; set; }
    }
}
