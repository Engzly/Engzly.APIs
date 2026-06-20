using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Users.Commands.Models;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Specifications.GigSpecifications;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Engzly.Application.Features.Users.Commands.Handlers
{
    public class DeleteUserCommandHandler(
        UserManager<User> userManager,
        IGenericRepository<Gig, string> gigRepository,
        ILogger<DeleteUserCommandHandler> logger)
        : ResponseHandler,
            IRequestHandler<DeleteUserCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "Attempting to delete user with Id {UserId}",
                request.Id
            );

            // Business Rule: User must exist
            var userExist = await userManager.FindByIdAsync(request.Id.ToString());
            if (userExist is null)
            {
                logger.LogWarning(
                    "Delete failed: User with Id {UserId} does not exist",
                    request.Id
                );
                return NotFound<string>($"User with Id {request.Id} does not exist");
            }

            var userGigsSpec = new GigsByOwnerIdSpecification(request.Id.ToString());
            var userGigs = await gigRepository.GetAllAsync(userGigsSpec, cancellationToken);

            foreach (var gig in userGigs)
                gigRepository.Delete(gig);

            var deleteResult = await userManager.DeleteAsync(userExist);

            // Save changes to the database
            await gigRepository.CompleteAsync(cancellationToken);

            if (!deleteResult.Succeeded)
            {
                var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                logger.LogWarning(
                    "Failed to delete user {UserId}. Errors: {Errors}",
                    request.Id,
                    errors
                );
                return BadRequest<string>("Delete failed");
            }

            logger.LogInformation(
                "User {UserId} deleted successfully",
                request.Id
            );

            return Deleted("");
        }
    }
}