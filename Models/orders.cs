using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace project_web2.Models
{
    public class Orders
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [BindProperty, DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Total { get; set; }
    }
}
