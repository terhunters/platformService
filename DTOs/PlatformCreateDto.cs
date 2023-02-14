using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs
{
    public class PlatformCreateDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Publisher { get; set; }
        [Required]
        public string Version { get; set; }
    }
}