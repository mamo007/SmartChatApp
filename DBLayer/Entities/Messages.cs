using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Entities
{
    public class Messages
    {
        [Key]
        public int MessageId { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        [Column(TypeName = "utf8mb4")]
        public string Message { get; set; }
        public string Date { get; set; }
        public int Read { get; set; }
        public int Delete { get; set; }

        public override string ToString()
        {
            return $"{MessageId} : {Sender} : {Receiver} : " +
                $"{Message} : {Date} : {Read} : {Delete}";
        }
    }
}
