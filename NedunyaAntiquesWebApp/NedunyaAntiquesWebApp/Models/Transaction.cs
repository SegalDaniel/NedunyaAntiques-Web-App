﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NedunyaAntiquesWebApp.Models
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        public bool Delivery { get; set; }

        // TODO: Lets see how we can secure this prop
        public bool Paid { get; set; }

        [DataType(DataType.Date)]
        public DateTime TransDate { get; set; }

        public Decimal Amount { get; set; }

        public virtual Customer customer { get; set; }

        public virtual ShoppingCart Cart { get; set; }
    }
}