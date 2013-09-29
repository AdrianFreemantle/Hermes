﻿using System;

namespace Hermes.EntityFramework.SagaPersistence
{
    public class SagaEntity
    {
        public Guid Id { get; set; }
        public Byte[] TimeStamp { get; set; }
        public byte[] State { get; set; }
    }
}