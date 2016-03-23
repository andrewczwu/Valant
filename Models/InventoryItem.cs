using System;
using System.ComponentModel.DataAnnotations;

namespace ValantInv.Models
{
    public class InventoryItem
    {
        public int ID { get; set; }
       
        public string Label { get; set; }

        public DateTime Expiration { get; set; }

        private bool _notificationSent = false;
    }
}