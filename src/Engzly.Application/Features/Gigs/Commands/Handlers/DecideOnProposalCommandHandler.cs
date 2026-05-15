using Engzly.Application.Common.Bases;
using Engzly.Application.Features.Chat.Common;
using Engzly.Application.Features.Chat.Responses;
using Engzly.Application.Features.Gigs.Commands.Models;
using Engzly.Application.Interfaces;
using Engzly.Application.Interfaces.Authentication;
using Engzly.Application.Interfaces.Repositories;
using Engzly.Application.Responses.GigsResponse;
using Engzly.Domain.Entities.Chat;
using Engzly.Domain.Entities.Gigs;
using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using Engzly.Domain.Specifications;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace Engzly.Application.Features.Gigs.Commands.Handlers
{
    public class DecideOnProposalCommandHandler(
        IUnitOfWork _unitOfWork,
        ICurrentUserService _currentUser,
        UserManager<User> _userManager,
        IChatNotifier _chatNotifier)
        : ResponseHandler, IRequestHandler<DecideOnProposalCommand, Response<DecideOnProposalResponse>>
    {

        public async Task<Response<DecideOnProposalResponse>> Handle(DecideOnProposalCommand request, CancellationToken ct)
        {
            var _proposalsRepo = _unitOfWork.Proposals;
            var _gigsRepo = _unitOfWork.Gigs;
            var _gigAssignmentRepo = _unitOfWork.GigAssignments;
            var _conversationsRepo = _unitOfWork.Conversations;
            var _participantsRepo = _unitOfWork.ConversationParticipants;


            var currentUser = _currentUser.GetCurrentUser();
            if (string.IsNullOrWhiteSpace(currentUser.Id))
                return Unauthorized<DecideOnProposalResponse>(" Must Login First ");

            var proposal = await _unitOfWork.Proposals.GetByIdAsync(request.ProposalId, ct);
            if (proposal is null)
                return NotFound<DecideOnProposalResponse>($"اRequest with Id {request.ProposalId} Not Exist  ");

            var gig = await _gigsRepo.GetByIdLockedAsync(proposal.GigId, ct);
            if (gig is null)
                return NotFound<DecideOnProposalResponse>($"Task with Id {gig?.Id} Not Exist ");

            var currentUserId = currentUser.Id;
            if (currentUserId != gig.OwnerId)
                return Forbidden<DecideOnProposalResponse>("You Don't Have Permision to Make Any Changes on this Task ");

            if (gig.Status != GigStatus.Published)
                return Gone<DecideOnProposalResponse>($"Task is {gig.Status.ToString()}");


            if (proposal.Status != ProposalStatus.Pending)
                return Conflict<DecideOnProposalResponse>($"The Proposal is {proposal.Status.ToString()}");

            Conversation? conversationToNotify = null;
            ConversationParticipantItem? joinedParticipant = null;
            List<string> notifyUserIds = new();

            await _unitOfWork.BeginTransactionAsync(ct);
            try
            {

                switch (request.Decision)
                {
                    case ProposalStatus.Approved:
                        var helper = await _userManager.FindByIdAsync(proposal.TaskerId);
                        if (helper is null || !helper.IsIdentityVerified)
                            return Forbidden<DecideOnProposalResponse>("The helper is no longer identity verified and cannot be approved");
                        proposal.Approve();
                        var assignment = new GigAssignment
                        {
                            Id = Guid.NewGuid().ToString(),
                            GigId = gig.Id,
                            TaskerId = proposal.TaskerId,
                            AssignedOn = DateTime.UtcNow,
                            ClientId = gig.OwnerId
                        };
                        await _gigAssignmentRepo.AddAsync(assignment);
                        gig.Status = GigStatus.HelpersAssigned;

                        var now = DateTime.UtcNow;
                        var existingConvs = await _conversationsRepo.GetAllAsync(
                            new GigConversationSpec(gig.Id),
                            ct);

                        var conversation = existingConvs.FirstOrDefault();
                        if (conversation is null)
                        {
                            conversation = new Conversation
                            {
                                Id = Guid.NewGuid().ToString(),
                                GigId = gig.Id,
                                OwnerId = null,
                                IsBot = false,
                                CreatedOn = now,
                                LastMessageOn = now
                            };
                            await _conversationsRepo.AddAsync(conversation, ct);
                            //await _conversationsRepo.CompleteAsync(ct);

                            var clientParticipant = new ConversationParticipant
                            {
                                Id = Guid.NewGuid().ToString(),
                                ConversationId = conversation.Id,
                                UserId = gig.OwnerId,
                                Role = ConversationParticipantRole.Client,
                                JoinedOn = now
                            };
                            await _participantsRepo.AddAsync(clientParticipant, ct);
                            //await _participantsRepo.CompleteAsync(ct);
                        }

                        var existingHelperParticipation = await _participantsRepo.GetAllAsync(
                            new UserParticipationSpec(conversation.Id, proposal.TaskerId),
                            ct);

                        var helperParticipant = existingHelperParticipation.FirstOrDefault();
                        if (helperParticipant is null)
                        {
                            helperParticipant = new ConversationParticipant
                            {
                                Id = Guid.NewGuid().ToString(),
                                ConversationId = conversation.Id,
                                UserId = proposal.TaskerId,
                                Role = ConversationParticipantRole.Helper,
                                JoinedOn = now
                            };
                            await _participantsRepo.AddAsync(helperParticipant, ct);
                        }
                        else
                        {
                            helperParticipant.LeftOn = null;
                            helperParticipant.LeaveReason = null;
                            helperParticipant.JoinedOn = now;
                            _participantsRepo.Update(helperParticipant);
                        }
                        //await _participantsRepo.CompleteAsync(ct);

                        conversationToNotify = conversation;
                        joinedParticipant = new ConversationParticipantItem(
                            proposal.TaskerId,
                            helper.UserName ?? "Helper",
                            ConversationParticipantRole.Helper,
                            now,
                            null,
                            null);

                        var allParticipants = await _participantsRepo.GetAllAsync(
                            new ConversationParticipantsSpec(conversation.Id, activeOnly: true),
                            ct);
                        notifyUserIds = allParticipants.Select(p => p.UserId).Distinct().ToList();

                        break;

                    case ProposalStatus.Rejected:
                        proposal.Reject();
                        break;

                    default:
                        return BadRequest<DecideOnProposalResponse>("You can't Apply This Decision You Must Choose between (Approved: 1 | Rejected: 2 ) ");
                }

                _proposalsRepo.Update(proposal);
                _gigsRepo.Update(gig);

                await _proposalsRepo.CompleteAsync(ct);

                await _unitOfWork.CommitTransactionAsync(ct);

                if (conversationToNotify is not null && joinedParticipant is not null)
                {
                    await _chatNotifier.NotifyParticipantJoinedAsync(
                        conversationToNotify.Id,
                        notifyUserIds,
                        joinedParticipant,
                        ct);
                }

                var res = new DecideOnProposalResponse(
                    Message: " congrats Approved on The Helper Request  ",
                    ProposalId: proposal.Id,
                    Decision: request.Decision,
                    TaskId: gig.Id,
                    TaskStatus: gig.Status.ToString(),
                    ClientId: gig.OwnerId,
                    TaskerId: proposal.TaskerId
                );
                return Success(res);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return InternalServerError<DecideOnProposalResponse>($" {ex.Message} ");
            }
        }

        private sealed class GigConversationSpec : BaseSpecification<Conversation>
        {
            public GigConversationSpec(string gigId)
                : base(c => c.GigId == gigId && !c.IsBot)
            { }
        }
    }
}
