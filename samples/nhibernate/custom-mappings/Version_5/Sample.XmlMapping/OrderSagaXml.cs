﻿using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Saga;

public class OrderSagaXml : Saga<OrderSagaDataXml>,
    IAmStartedByMessages<StartOrder>,
    IHandleMessages<CompleteOrder>
{
    static ILog logger = LogManager.GetLogger(typeof(OrderSagaXml));

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OrderSagaDataXml> mapper)
    {
        mapper.ConfigureMapping<StartOrder>(m => m.OrderId)
                .ToSaga(s => s.OrderId);
        mapper.ConfigureMapping<CompleteOrder>(m => m.OrderId)
                .ToSaga(s => s.OrderId);
    }

    public void Handle(StartOrder message)
    {
        Data.OrderId = message.OrderId;
        logger.Info(string.Format("Saga with OrderId {0} received StartOrder with OrderId {1} (Saga version: {2})", Data.OrderId, message.OrderId, Data.Version));

        if (Data.From == null) Data.From = new OrderSagaDataXml.Location();
        if (Data.To == null) Data.To = new OrderSagaDataXml.Location();

        Data.From.Lat = 51.9166667;
        Data.From.Long = 4.5;

        Data.To.Lat = 51.51558;
        Data.To.Long = -0.12085;
    }

    public void Handle(CompleteOrder message)
    {
        logger.Info(string.Format("Saga with OrderId {0} received CompleteOrder with OrderId {1}", Data.OrderId, message.OrderId));
        MarkAsComplete();
    }


}
