using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.AiChat;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/ai-chat/rooms")]
    public class AiChatRoomsController : ControllerBase
    {
        private readonly IChatRoomService _chatRoomService;

        public AiChatRoomsController(IChatRoomService chatRoomService)
        {
            _chatRoomService = chatRoomService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get my AI chat rooms",
            Description = "Paginated list of the current user's conversations with the AI assistant, most recently used first.")]
        public async Task<IActionResult> GetMyRooms([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _chatRoomService.GetMyRoomsAsync(page, pageSize);
            return Ok(ApiResult<Pagination<ChatRoomResponse>>.Success(result));
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create an AI chat room",
            Description = "Creates an empty conversation. Without a title it is named after its first question.")]
        public async Task<IActionResult> Create([FromBody] CreateChatRoomRequest request)
        {
            var result = await _chatRoomService.CreateAsync(request);
            return Ok(ApiResult<ChatRoomResponse>.Success(result));
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Rename an AI chat room", Description = "Title: 1 to 100 characters, one line.")]
        public async Task<IActionResult> Rename(Guid id, [FromBody] RenameChatRoomRequest request)
        {
            var result = await _chatRoomService.RenameAsync(id, request);
            return Ok(ApiResult<ChatRoomResponse>.Success(result));
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Delete an AI chat room", Description = "Soft deletes the room and its messages.")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _chatRoomService.DeleteAsync(id);
            return Ok(ApiResult.Success());
        }

        [HttpGet("{id:guid}/messages")]
        [SwaggerOperation(
            Summary = "Get messages of an AI chat room",
            Description = "Page 1 holds the newest messages; inside a page messages are oldest first. Only final answers are stored (no thinking).")]
        public async Task<IActionResult> GetMessages(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            var result = await _chatRoomService.GetMessagesAsync(id, page, pageSize);
            return Ok(ApiResult<Pagination<ChatMessageResponse>>.Success(result));
        }

        [HttpPost("{id:guid}/messages/{messageId:guid}/rollback")]
        [SwaggerOperation(
            Summary = "Roll an AI chat room back to before a question",
            Description = "Only a user message can be the target. That message and every later one are hidden and no longer sent to the AI, but stay in the database. Returns the number of messages cut.")]
        public async Task<IActionResult> Rollback(Guid id, Guid messageId)
        {
            var result = await _chatRoomService.RollbackAsync(id, messageId);
            return Ok(ApiResult<int>.Success(result));
        }
    }
}
