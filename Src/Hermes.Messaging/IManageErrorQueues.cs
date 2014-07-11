using System;
using Hermes.Messaging.Management;
using Hermes.Queries;

namespace Hermes.Messaging
{
    public interface IManageErrorQueues
    {
        int GetErrorCount();
        void Delete(Guid id);
        void Resend(TransportMessageDto dto);
        PagedResult<TransportMessageDto> GetErrorMessages(int pageNumber, int resultsPerPage);
    }
}
