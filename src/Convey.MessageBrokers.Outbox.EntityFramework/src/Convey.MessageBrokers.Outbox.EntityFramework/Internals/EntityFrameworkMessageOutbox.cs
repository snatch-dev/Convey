using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Convey.MessageBrokers.Outbox.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Convey.MessageBrokers.Outbox.EntityFramework.Internals;

internal sealed class EntityFrameworkMessageOutbox<TContext> : IMessageOutbox, IMessageOutboxAccessor
    where TContext : DbContext
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private const string EmptyJsonObject = "{}";
    private readonly TContext _dbContext;
    private readonly ILogger<EntityFrameworkMessageOutbox<TContext>> _logger;
    private readonly DbSet<InboxMessage> _inboxMessagesSet;

    public bool Enabled { get; }

    public EntityFrameworkMessageOutbox(TContext dbContext, OutboxOptions options,
        ILogger<EntityFrameworkMessageOutbox<TContext>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _inboxMessagesSet = _dbContext.Set<InboxMessage>();
        Enabled = options.Enabled;
    }

    public async Task HandleAsync(string messageId, Func<Task> handler)
    {
        if (!Enabled)
        {
            _logger.LogWarning("Outbox is disabled, incoming messages won't be processed.");
            return;
        }

        if (string.IsNullOrWhiteSpace(messageId))
        {
            throw new ArgumentException("Message ID to be processed cannot be empty.", nameof(messageId));
        }
            
        _logger.LogTrace($"Received a message with ID: '{messageId}' to be processed.");
        if (await _inboxMessagesSet.AnyAsync(m => m.Id == messageId))
        {
            _logger.LogTrace($"Message with ID: '{messageId}' was already processed.");
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            _logger.LogTrace($"Processing a message with ID: '{messageId}'...");
            await handler();
            await _inboxMessagesSet.AddAsync(new InboxMessage
            {
                Id = messageId,
                ProcessedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogTrace($"Processed a message with ID: '{messageId}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"There was an error when processing a message with ID: '{messageId}'.");
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task SendAsync<T>(T message, string originatedMessageId = null, string messageId = null,
        string correlationId = null, string spanContext = null, object messageContext = null,
        IDictionary<string, object> headers = null) where T : class
    {
        if (!Enabled)
        {
            _logger.LogWarning("Outbox is disabled, outgoing messages won't be saved into the storage.");
            return;
        }

        var outboxMessagesSet = _dbContext.Set<OutboxMessage>();
        var outboxMessage = new OutboxMessage
        {
            Id = string.IsNullOrWhiteSpace(messageId) ? Guid.NewGuid().ToString("N") : messageId,
            OriginatedMessageId = originatedMessageId,
            CorrelationId = correlationId,
            SpanContext = spanContext,
            SerializedMessageContext = messageContext is null
                ? EmptyJsonObject
                : JsonSerializer.Serialize(messageContext, SerializerOptions),
            MessageContextType = messageContext?.GetType().AssemblyQualifiedName,
            Headers = (Dictionary<string, object>)headers,
            SerializedMessage = message is null
                ? EmptyJsonObject
                : JsonSerializer.Serialize((object)message, SerializerOptions),
            MessageType = message?.GetType().AssemblyQualifiedName,
            SentAt = DateTime.UtcNow
        };
        await outboxMessagesSet.AddAsync(outboxMessage);
        await _dbContext.SaveChangesAsync();
    }

    async Task<IReadOnlyList<OutboxMessage>> IMessageOutboxAccessor.GetUnsentAsync()
    {
        var outboxMessagesSet = _dbContext.Set<OutboxMessage>();
        var outboxMessages = await outboxMessagesSet.Where(om => om.ProcessedAt == null).ToListAsync();
        return outboxMessages.Select(om =>
        {
            if (om.MessageContextType is not null)
            {
                var messageContextType = Type.GetType(om.MessageContextType);
                om.MessageContext = JsonSerializer.Deserialize(om.SerializedMessageContext, messageContextType,
                    SerializerOptions);
            }

            if (om.MessageType is not null)
            {
                var messageType = Type.GetType(om.MessageType);
                om.Message = JsonSerializer.Deserialize(om.SerializedMessage, messageType, SerializerOptions);
            }

            return om;
        }).ToList();
    }

    Task IMessageOutboxAccessor.ProcessAsync(OutboxMessage message)
    {
        UpdateMessage(_dbContext.Set<OutboxMessage>(), message);

        return _dbContext.SaveChangesAsync();
    }

    Task IMessageOutboxAccessor.ProcessAsync(IEnumerable<OutboxMessage> outboxMessages)
    {
        var set = _dbContext.Set<OutboxMessage>();
        foreach (var message in outboxMessages)
        {
            UpdateMessage(set, message);
        }

        return _dbContext.SaveChangesAsync();
    }

    private static void UpdateMessage(DbSet<OutboxMessage> set, OutboxMessage message)
    {
        message.ProcessedAt = DateTime.UtcNow;
        set.Update(message);
    }
}