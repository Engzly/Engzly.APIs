using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Payments.Queries.Models;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.PaymentsResponse;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Engzly.Application.Features.Payments.Queries.Handlers
{
    public class GetAllPaymentsHandler
        (
        IGenericRepository<Payment, string> _paymentRepo,
        ICurrentUserService _currentUser,
        UserManager<User> _userManger
        )
        : ResponseHandler,
       IRequestHandler<GetAllPaymentsQuery,
       Response<List<PaymentListItemResponse>>>
    {


        public async Task<Response<List<PaymentListItemResponse>>> Handle(
            GetAllPaymentsQuery request,
            CancellationToken cancellationToken)
        {
            var user = _currentUser.GetCurrentUser();
            //var _user = await _userManger.FindByIdAsync(user.Id);
            if (user is null && user.AccountType.ToString() != AccountType.Admin.ToString())
                return Unauthorized<List<PaymentListItemResponse>>("You Are Not Authorized Brooooooo");


            var payments =
                await _paymentRepo.GetAllAsync(cancellationToken);

            if (payments.Count > 0)
            {
                var result = payments.Select(x =>
                    new PaymentListItemResponse
                    {
                        PaymentId = x.Id,
                        GigId = x.GigId,
                        Amount = x.Amount,
                        Currency = x.Currency,
                        Status = x.Status,
                        CreatedAtUtc = x.CreatedAtUtc
                    }).ToList();

                return Success(result);
            }
            else
                return Success<List<PaymentListItemResponse>>([]);
        }
    }
}
