﻿using System.Text.Json.Serialization;

namespace EventBus.Events;

public record IntegrationEvent
{
    [JsonInclude]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonInclude]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
}
