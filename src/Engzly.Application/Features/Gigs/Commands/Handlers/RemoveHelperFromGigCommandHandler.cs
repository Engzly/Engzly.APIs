using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Common;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Specifications;
using MediatR;

namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    public sealed class RemoveHelperFromGigCommandHandler(
        IUnitOfWork _unitOfWork,
        ICurrentUserService _currentUser,
        IChatNotifier _chatNotifier)
        : ResponseHandler, IRequestHandler<RemoveHelperFromGigCommand, Response<string>>
    {
        public async Task<Response<string>> Handle(
            RemoveHelperFromGigCommand request,
            CancellationToken ct)
        {
            var caller = _currentUser.GetCurrentUser();
            if (caller is null || string.IsNullOrWhiteSpace(caller.Id))
                return Unauthorized<string>("Must login first");

            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest<string>("A removal reason is required");

            if (request.Reason.Length > 500)
                return BadRequest<string>("Reason exceeds 500 characters");

            var gig = await _unitOfWork.Gigs.GetByIdAsync(request.GigId, ct);
            if (gig is null)
                return NotFound<string>("Gig not found");

            if (gig.OwnerId != caller.Id)
                return Forbidden<string>("Only the gig owner can remove helpers");

            var assignments = await _unitOfWork.GigAssignments.GetAllAsync(
                new GigAssignmentForHelperSpec(request.GigId, request.HelperId),
                ct);

            var assignment = assignments.FirstOrDefault();
            if (assignment is null)
                return NotFound<string>("This helper is not assigned to this gig");

            await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                _unitOfWork.GigAssignments.Delete(assignment);
                await _unitOfWork.GigAssignments.CompleteAsync(ct);

                var conversations = await _unitOfWork.Conversations.GetAllAsync(
                    new GigConversationSpec(gig.Id),
                    ct);

                var conversation = conversations.FirstOrDefault();
                List<string> notifyUserIds = new();
                if (conversation is not null)
                {
                    var participation = await _unitOfWork.ConversationParticipants.GetAllAsync(
                        new UserParticipationSpec(conversation.Id, request.HelperId),
                        ct);

                    var helperParticipant = participation.FirstOrDefault();
                    if (helperParticipant is not null && helperParticipant.LeftOn is null)
                    {
                        helperParticipant.LeftOn = DateTime.UtcNow;
                        helperParticipant.LeaveReason = request.Reason;
                        _unitOfWork.ConversationParticipants.Update(helperParticipant);
                        await _unitOfWork.ConversationParticipants.CompleteAsync(ct);
                    }

                    var remaining = await _unitOfWork.ConversationParticipants.GetAllAsync(
                        new ConversationParticipantsSpec(conversation.Id, activeOnly: true),
                        ct);
                    notifyUserIds = remaining.Select(p => p.UserId).Append(request.HelperId).Distinct().ToList();
                }

                await _unitOfWork.CommitTransactionAsync(ct);

                if (conversation is not null)
                {
                    await _chatNotifier.NotifyParticipantLeftAsync(
                        conversation.Id,
                        notifyUserIds,
                        request.HelperId,
                        request.Reason,
                        ct);
                }

                return Success(request.HelperId, "Helper removed from gig");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return InternalServerError<string>(ex.Message);
            }
        }

        private sealed class GigAssignmentForHelperSpec : BaseSpecification<Domain.Entities.Gigs.GigAssignment>
        {
            public GigAssignmentForHelperSpec(string gigId, string helperId)
                : base(a => a.GigId == gigId && a.TaskerId == helperId)
            { }
        }

        private sealed class GigConversationSpec : BaseSpecification<Conversation>
        {
            public GigConversationSpec(string gigId)
                : base(c => c.GigId == gigId && !c.IsBot)
            { }
        }
    }
}
