//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using TaskManagerAPI.Application.Interfaces;
//using TaskManagerAPI.Controllers;
//using TaskManagerAPI.Dtos.Task;
//using TaskManagerAPI.Dtos.TaskList;

//namespace TaskManagerAPI.Tests.Controllers;

//public class TaskListControllerTests
//{
//    private readonly Mock<ITaskListService> _mockTaskListService;
//    private readonly Mock<IUserContext> _mockUserContext;
//    private readonly TaskListController _controller;

//    public TaskListControllerTests()
//    {
//        _mockTaskListService = new Mock<ITaskListService>();
//        _mockUserContext = new Mock<IUserContext>();
//        _controller = new TaskListController(_mockTaskListService.Object, _mockUserContext.Object);
//    }

//    [Fact]
//    public async Task CreateTaskList_WhenAuthorized_ReturnsCreatedAtActionResult()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var createDto = new CreateTaskListDto { Title = "New Task List" };
//        var expectedTaskList = new TaskListDto
//        {
//            Id = 1,
//            Title = "New Task List",
//            BoardId = boardId,
//            Order = 1
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskListService.Setup(x => x.CreateTaskListAsync(createDto, boardId, userId))
//            .ReturnsAsync(expectedTaskList);

//        // Act
//        var result = await _controller.CreateTaskList(boardId, createDto);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<TaskListDto>>(result);
//        var createdAtResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
//        Assert.Equal(nameof(TaskListController.GetTaskListTasks), createdAtResult.ActionName);
//        Assert.Equal(expectedTaskList.Id, createdAtResult.RouteValues["id"]);

//        var taskList = Assert.IsType<TaskListDto>(createdAtResult.Value);
//        Assert.Equal("New Task List", taskList.Title);
//        Assert.Equal(boardId, taskList.BoardId);
//    }

//    [Fact]
//    public async Task CreateTaskList_WhenNotAuthorized_ReturnsForbid()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var createDto = new CreateTaskListDto { Title = "New Task List" };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskListService.Setup(x => x.CreateTaskListAsync(createDto, boardId, userId))
//            .ThrowsAsync(new UnauthorizedAccessException());

//        // Act
//        var result = await _controller.CreateTaskList(boardId, createDto);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<TaskListDto>>(result);
//        Assert.IsType<ForbidResult>(actionResult.Result);
//    }

//    [Fact]
//    public async Task UpdateTaskList_WhenAuthorized_ReturnsNoContent()
//    {
//        // Arrange
//        var taskListId = 1;
//        var userId = 1;
//        var updateDto = new UpdateTaskListDto { Title = "Updated Task List" };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskListService.Setup(x => x.UpdateTaskListAsync(taskListId, updateDto, userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _controller.UpdateTaskList(taskListId, updateDto);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task UpdateTaskList_WhenNotFound_ReturnsNotFound()
//    {
//        // Arrange
//        var taskListId = 1;
//        var userId = 1;
//        var updateDto = new UpdateTaskListDto { Title = "Updated Task List" };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskListService.Setup(x => x.UpdateTaskListAsync(taskListId, updateDto, userId))
//            .ThrowsAsync(new KeyNotFoundException());

//        // Act
//        var result = await _controller.UpdateTaskList(taskListId, updateDto);

//        // Assert
//        Assert.IsType<NotFoundResult>(result);
//    }

//    [Fact]
//    public async Task DeleteTaskList_WhenAuthorized_ReturnsNoContent()
//    {
//        // Arrange
//        var taskListId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskListService.Setup(x => x.DeleteTaskListAsync(taskListId, userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _controller.DeleteTaskList(taskListId);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task GetTaskListTasks_WhenAuthorized_ReturnsOkResultWithTasks()
//    {
//        // Arrange
//        var taskListId = 1;
//        var userId = 1;
//        var expectedTasks = new List<TaskShortDto>
//            {
//                new TaskShortDto { Id = 1, Title = "Task 1", TaskListId = taskListId },
//                new TaskShortDto { Id = 2, Title = "Task 2", TaskListId = taskListId }
//            };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskListService.Setup(x => x.GetTaskListTasksAsync(taskListId, userId))
//            .ReturnsAsync(expectedTasks);

//        // Act
//        var result = await _controller.GetTaskListTasks(taskListId);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskShortDto>>>(result);
//        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
//        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskShortDto>>(okResult.Value);
//        Assert.Equal(2, tasks.Count());
//    }

//    [Fact]
//    public async Task GetTaskListTasks_WhenNotFound_ReturnsNotFound()
//    {
//        // Arrange
//        var taskListId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskListService.Setup(x => x.GetTaskListTasksAsync(taskListId, userId))
//            .ThrowsAsync(new KeyNotFoundException());

//        // Act
//        var result = await _controller.GetTaskListTasks(taskListId);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskShortDto>>>(result);
//        Assert.IsType<NotFoundResult>(actionResult.Result);
//    }

//    [Fact]
//    public async Task ReorderTaskLists_WhenAuthorized_ReturnsNoContent()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var reorderDto = new ReorderTaskListsDto
//        {
//            ListsOrder = new List<ListOrderUpdateDto>
//                {
//                    new ListOrderUpdateDto { ListId = 1, NewPosition = 2 },
//                    new ListOrderUpdateDto { ListId = 2, NewPosition = 1 }
//                }
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskListService.Setup(x => x.ReorderTaskListsAsync(boardId, reorderDto, userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _controller.ReorderTaskLists(boardId, reorderDto);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task ReorderTaskLists_WhenNotFound_ReturnsNotFound()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var reorderDto = new ReorderTaskListsDto
//        {
//            ListsOrder = new List<ListOrderUpdateDto>
//                {
//                    new ListOrderUpdateDto { ListId = 1, NewPosition = 2 }
//                }
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskListService.Setup(x => x.ReorderTaskListsAsync(boardId, reorderDto, userId))
//            .ThrowsAsync(new KeyNotFoundException());

//        // Act
//        var result = await _controller.ReorderTaskLists(boardId, reorderDto);

//        // Assert
//        Assert.IsType<NotFoundResult>(result);
//    }

//    [Fact]
//    public async Task ReorderTaskLists_WhenNotAuthorized_ReturnsForbid()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var reorderDto = new ReorderTaskListsDto
//        {
//            ListsOrder = new List<ListOrderUpdateDto>
//                {
//                    new ListOrderUpdateDto { ListId = 1, NewPosition = 2 }
//                }
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskListService.Setup(x => x.ReorderTaskListsAsync(boardId, reorderDto, userId))
//            .ThrowsAsync(new UnauthorizedAccessException());

//        // Act
//        var result = await _controller.ReorderTaskLists(boardId, reorderDto);

//        // Assert
//        Assert.IsType<ForbidResult>(result);
//    }
//}