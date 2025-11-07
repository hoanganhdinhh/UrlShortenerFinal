//using System.ComponentModel.DataAnnotations;
//using URLShortener.MVC.Commons;

//namespace URLShortener.MVC.Data.Entities
//{
//    public class Url
//    {
//        //=== Properties ===//
//        #region Properties
//        public int Id { get; set; }
//        [MaxLength(MaxLengths.OriginalUrl)]
//        public string OriginalUrl { get; set; } = string.Empty;
//        [MaxLength(MaxLengths.ShortCode)]
//        public string ShortCode { get; set; } = string.Empty;
//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//        #endregion
//    }
//}


using System.ComponentModel.DataAnnotations;

namespace URLShortener.MVC.Data.Entities
{
    public class Url
    {
        //=== Properties ===//
        #region Properties
        public int Id { get; set; }
        [MaxLength()]
        public string OriginalUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "Short code must be between 4 and 10 characters.")]
        public string ShortCode { get; set; } = string.Empty;

        [Range(0, long.MaxValue)]
        public long ClickCount { get; set; } = 0;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(450)]
        public string? UserId { get; set; }   // khóa ngoại nếu dùng Microsoft Identity
        #endregion
    }
}
