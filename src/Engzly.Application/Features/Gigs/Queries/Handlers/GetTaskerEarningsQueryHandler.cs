//using Engzly.Application.Common.Bases;
//using Engzly.Application.Features.Gigs.Queries.Models;
//using Engzly.Application.Interfaces.Authentication;
//using Engzly.Application.Interfaces.Repositories;
//using Engzly.Application.Responses.GigsResponse;
//using Engzly.Domain.Entities.Payments;
//using Engzly.Domain.Specifications.GigSpecifications;
//using MediatR;

//namespace Engzly.Application.Features.Gigs.Queries.Handlers
//{
//    public class GetTaskerEarningsQueryHandler(
//     ICurrentUserService currentUser,
//     IGenericRepository<Payment, string> payments)
//     : ResponseHandler,
//       IRequestHandler<GetTaskerEarningsQuery,
//       Response<TaskerEarningsResponse>>
//    {
//        public async Task<Response<TaskerEarningsResponse>> Handle(
//            GetTaskerEarningsQuery request,
//            CancellationToken ct)
//        {
//            var user = currentUser.GetCurrentUser();

//            if (user == null || string.IsNullOrWhiteSpace(user.Id))
//                return Unauthorized<TaskerEarningsResponse>();

//            var allPayments = await payments.GetAllAsync(
//                new TaskerPaymentsSpec(user.Id),
//                ct);

//            var now = DateTime.UtcNow;

//            var today = now.Date;

//            var weekStart = today.AddDays(-(int)today.DayOfWeek);

//            var monthStart = new DateTime(
//                now.Year,
//                now.Month,
//                1);

//            var todayAmount = allPayments
//                .Where(x => x.UpdatedAtUtc.Date == today)
//                .Sum(x => x.HelperAmount);

//            var weekAmount = allPayments
//                .Where(x => x.UpdatedAtUtc >= weekStart)
//                .Sum(x => x.HelperAmount);

//            var monthAmount = allPayments
//                .Where(x => x.UpdatedAtUtc >= monthStart)
//                .Sum(x => x.HelperAmount);

//            var totalAmount = allPayments
//                .Sum(x => x.HelperAmount);

//            return Success(
//                new TaskerEarningsResponse(
//                    todayAmount,
//                    weekAmount,
//                    monthAmount,
//                    totalAmount,
//                    "EGP"));
//        }
//    }
//}


using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Queries.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications.GigSpecifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Queries.Handlers
{
    public class GetTaskerEarningsQueryHandler(
        ICurrentUserService currentUser,
        IGenericRepository<Payment, string> payments)
        : ResponseHandler,
          IRequestHandler<GetTaskerEarningsQuery,
          Response<TaskerEarningsResponse>>
    {
        public async Task<Response<TaskerEarningsResponse>> Handle(
            GetTaskerEarningsQuery request,
            CancellationToken ct)
        {
            var user = currentUser.GetCurrentUser();

            if (user == null || string.IsNullOrWhiteSpace(user.Id))
                return Unauthorized<TaskerEarningsResponse>();

            var allPayments = await payments.GetAllAsync(
                new TaskerPaymentsSpec(user.Id),
                ct);

            var releasedPayments = allPayments
                .Where(p =>
                    p.Status == PaymentStatus.Released &&
                    p.ReleasedAtUtc.HasValue)
                .ToList();

            var now = DateTime.UtcNow;
            var today = now.Date;

            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            var monthStart = new DateTime(
                now.Year,
                now.Month,
                1);

            var todayAmount = releasedPayments
                .Where(x => x.ReleasedAtUtc!.Value.Date == today)
                .Sum(x => x.HelperAmount);

            var weekAmount = releasedPayments
                .Where(x => x.ReleasedAtUtc!.Value >= weekStart)
                .Sum(x => x.HelperAmount);

            var monthAmount = releasedPayments
                .Where(x => x.ReleasedAtUtc!.Value >= monthStart)
                .Sum(x => x.HelperAmount);

            var totalAmount = releasedPayments
                .Sum(x => x.HelperAmount);

            return Success(
                new TaskerEarningsResponse(
                    todayAmount,
                    weekAmount,
                    monthAmount,
                    totalAmount,
                    "EGP"));
        }
    }
}