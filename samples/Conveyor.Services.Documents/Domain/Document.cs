using Convey.Types;
using System;

namespace Conveyor.Services.Documents.Domain
{
    public abstract class Document : IIdentifiable<Guid>
    {
        public Guid Id { get; private set; }
        protected Document(Guid id)
        {
            Id = id;
        }
    }
}