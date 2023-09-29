using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomatizerInterop.WebApi.Models
{
    public class Blob
    {
        [Key]
        public long Id { get; set; }

        public byte[] Data { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long Length { get; set; }
        public string Extension { get; set; }
        [Required(AllowEmptyStrings =false)]
        public string HashString { get; set; }
        public DateTimeOffset UploadDT { get; set; }
    }
}
