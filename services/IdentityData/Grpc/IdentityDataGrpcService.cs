using Grpc.Core;
using IdentityData.Grpc;

namespace IdentityData.GrpcServices;

sealed class IdentityDataGrpcService : IdentityDataGrpc.IdentityDataGrpcBase
{
    public override Task<GetStatusResponse> GetStatus(GetStatusRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetStatusResponse
        {
            Status = "ok"
        });
    }
}
