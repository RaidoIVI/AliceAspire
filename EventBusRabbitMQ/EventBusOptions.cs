namespace EventBusRabbitMq;

public class EventBusOptions(string subscriptionClientName)
{
    public string SubscriptionClientName { get; init; } = subscriptionClientName;
    public int RetryCount { get; init; } = 10;
}
