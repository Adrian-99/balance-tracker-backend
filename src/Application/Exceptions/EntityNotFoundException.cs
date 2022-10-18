using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class EntityNotFoundException : ResponseStatusException
    {
        public EntityNotFoundException(string entityTypeName, string entityId)
            : base(StatusCodes.Status404NotFound, $"{entityTypeName} with id {entityId} not found")
        { }
    }
}
