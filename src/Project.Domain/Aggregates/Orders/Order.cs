﻿using System;
using System.Collections.Generic;
using Project.Domain.Aggregates.Orders.Enums;
using Project.Domain.Aggregates.Orders.Events;
using Project.Domain.Aggregates.Orders.Rules;
using Project.Domain.Aggregates.Orders.Services;
using Project.Domain.Aggregates.Orders.ValueObjects;
using Project.Domain.Aggregates.Users.ValueObjects;
using Project.Domain.ValueObjects;

namespace Project.Domain.Aggregates.Orders
{
    public class Order : Entity, IAggregateRoot
    {
        private readonly List<OrderItem> orderItems;

        private Order()
        {
            orderItems = new List<OrderItem>();
        }

        public OrderId Id { get; private set; }

        public Description Description { get; private set; }

        public UserAddress Address { get; private set; }

        public OrderStatus Status { get; private set; }

        public OrderPostType? PostType { get; private set; }

        public IEnumerable<OrderItem> OrderItems => orderItems.AsReadOnly();

        public static Order Create(
            OrderId id,
            Guid creatorId,
            IOrderTimeValidator orderTimeValidator)
        {
            var order = new Order {Id = id, Status = OrderStatus.Received};

            order.AddEvent(new OrderCreatedEvent(order.Id, order.Status));
            order.TrackCreate(creatorId);

            CheckRule(new OrderTimeMustBeValidRule(order.Created, orderTimeValidator));

            return order;
        }

        public void ChangeItems(
            OrderItem[] items,
            IGoodsTotalPriceValidator validator)
        {
            CheckRule(new ItemsTotalPriceMustBeValidRule(items, validator));

            orderItems.Clear();

            foreach (var item in items)
                AddItem(item);
        }

        public void ChangeStatus(OrderStatus status, Guid updaterId)
        {
            if (Status == status)
                return;

            AddEvent(new OrderStatusChangedEvent(Id, Status, status));
            TrackUpdate(updaterId);

            Status = status;
        }

        public void ChangPostType(bool containsFragileItem, Guid updaterId)
        {
            switch (containsFragileItem)
            {
                case true
                    when PostType is OrderPostType.SpecialPost:
                case false
                    when PostType is OrderPostType.OrdinaryPost:
                    return;
            }

            var postType =
                containsFragileItem ? OrderPostType.SpecialPost : OrderPostType.OrdinaryPost;

            AddEvent(new OrderPostTypeChangedEvent(Id, PostType, postType));
            TrackUpdate(updaterId);

            PostType = postType;
        }

        public void ChangeAddress(UserAddress address, Guid updaterId)
        {
            if (Address == address)
                return;

            AddEvent(new OrderAddressChangedEvent(Id, Address, address));
            TrackUpdate(updaterId);

            Address = address;
        }

        public void ChangeDescription(Description description, Guid updaterId)
        {
            if (Description == description)
                return;

            AddEvent(new OrderDescriptionChangedEvent(Id, Description, description));
            TrackUpdate(updaterId);

            Description = description;
        }

        public void Delete()
        {
            if (CanBeDeleted())
                throw new InvalidOperationException();

            AddEvent(new OrderDeletedEvent(Id));
            MarkAsDeleted();
        }

        private void AddItem(OrderItem item)
        {
            if (orderItems.Contains(item))
                return;

            AddEvent(new OrderItemAddedEvent(Id, item.GoodId, item.Count));

            orderItems.Add(item);
        }
    }
}