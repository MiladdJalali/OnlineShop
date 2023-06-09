﻿using System;

namespace Project.Application.Aggregates.Orders.Queries
{
    public class OrderQueryResult
    {
        public Guid Id { get; set; }

        public string Status { get; set; }
        
        public string Address { get; set; }
        
        public string PostType { get; set; }

        public decimal TotalPrice { get; set; }

        public string Description { get; set; }

        public string Creator { get; set; }

        public string Updater { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Updated { get; set; }
    }
}