using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Payments;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using Engzly.Domain.Specifications.GigSpecifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{

    public sealed class DeleteTaskCommandHandler(IGenericRepository<Gig, string> _gigRepo,
                                                 ICurrentUserService _currentUser,
                                                IGenericRepository<Payment, string> _paymentRepo,
                                                 INotificationService _notificationService
                                                ) : ResponseHandler, IRequestHandler<DeleteTaskCommand, Response<string>>
    {

        public async Task<Response<string>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var gig = await _gigRepo.GetByIdAsync(
                request.Id,
                new GigWithMediasByIdSpecification(),
                cancellationToken);

            if (gig == null)
            {
                return NotFound<string>("Task not found");
            }
            var currentUser = _currentUser.GetCurrentUser();
            if (currentUser == null)
                return Unauthorized<string>("User not found");

            var currentUserId = currentUser.Id;
            if (gig.OwnerId != currentUserId)
            {
                return Unauthorized<string>("You are not authorized to delete this task");
            }

            var allowedTime = calcAllawedTimeToDeleteTask(gig);

            var spec = new PaymentsByGigIdSpecification(gig.Id);

            var payments = await _paymentRepo.GetAllAsync(spec);
            if (payments.Any())
            {


                // Check for Task Status before deleting events
                switch (gig.Status)
                {
                    // TODO We Must Give The Client Option When Delete The Task and it was Skips the Allowed Time (Discount From his Wallet Salary or Not Cancle the Task)
                    case GigStatus.Published:
                    case GigStatus.PendingVerification:
                    case GigStatus.InProgress:
                        if (DateTime.UtcNow > allowedTime)
                            return BadRequest<string>("Cannot delete gig with associated payments events while the gig is published");
                        break;
                }



                foreach (var payment in payments)
                    _paymentRepo.Delete(payment);

            }



            // Safe Set And Clear for All Related Data To The Task (Medias - Category - TaskersAssignments) First 
            gig.Medias.Clear();
            gig.Category = null;
            gig.TaskersAssignments.Clear();




            // ??? ??? taskers ???? ??? proposals ??? ??? gig ??
            var taskerIds = gig.TaskersAssignments
                .Select(a => a.TaskerId)
                .Distinct()
                .ToList();

            // ??? ??? device tokens — ????? ???? query ?? ??? db ??? DeviceToken entity
            // (???? DeviceToken entity ?? Domain)
            //var deviceTokens = await _notificationService.SendNotificationAsync(
            //    new ActiveDeviceTokensByUserIdsSpec(taskerIds));

            //var tokens = deviceTokens.Select(t => t.DeviceTokenValue).ToList();

            //if (tokens.Any())
            //{
            //    await _notificationService.SendNotificationAsync(
            //        tokens,
            //        "Gig Cancelled",
            //        $"The gig '{gig.Title}' has been deleted by the client."
            //    );
            //}

            _gigRepo.Delete(gig);
            await _gigRepo.CompleteAsync(cancellationToken);

            return Deleted<string>("Gig Deleted Successfully");
        }

        private static DateTime calcAllawedTimeToDeleteTask(Gig gig)
        {
            var duration = gig.DueDate - gig.StartDate;
            var allowedMinutes = duration.TotalMinutes * 0.25;
            return gig.StartDate.AddMinutes(allowedMinutes);
        }
    }
}
