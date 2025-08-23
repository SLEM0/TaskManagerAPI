//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using TaskManagerAPI.Application.Interfaces;
//using TaskManagerAPI.Controllers;
//using TaskManagerAPI.Dtos.Task;

//namespace TaskManagerAPI.Tests.Controllers;

//public class TaskControllerTests
//{
//    private readonly Mock<ITaskService> _mockTaskService;
//    private readonly Mock<IUserContext> _mockUserContext;
//    private readonly TaskController _controller;

//    public TaskControllerTests()
//    {
//        _mockTaskService = new Mock<ITaskService>();
//        _mockUserContext = new Mock<IUserContext>();
//        _controller = new TaskController(_mockTaskService.Object, _mockUserContext.Object);
//    }

//    [Fact]
//    public async Task CreateTask_WhenAuthorized_ReturnsCreatedAtActionResult()
//    {
//        // Arrange
//        var listId = 1;
//        var userId = 1;
//        var createDto = new CreateTaskDto
//        {
//            Title = "New Task",
//            Description = "Task Description",
//            DueDate = DateTime.UtcNow.AddDays(7)
//        };
//        var expectedTask = new TaskDetailsDto
//        {
//            Id = 1,
//            Title = "New Task",
//            Description = "Task Description"
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.CreateTaskAsync(createDto, listId, userId))
//            .ReturnsAsync(expectedTask);

//        // Act
//        var result = await _controller.CreateTask(listId, createDto);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<TaskDetailsDto>>(result);
//        var createdAtResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
//        Assert.Equal(nameof(TaskController.GetTask), createdAtResult.ActionName);
//        Assert.Equal(expectedTask.Id, createdAtResult.RouteValues["id"]);

//        var task = Assert.IsType<TaskDetailsDto>(createdAtResult.Value);
//        Assert.Equal("New Task", task.Title);
//    }

//    [Fact]
//    public async Task CreateTask_WhenNotAuthorized_ReturnsForbid()
//    {
//        // Arrange
//        var listId = 1;
//        var userId = 1;
//        var createDto = new CreateTaskDto
//        {
//            Title = "New Task",
//            Description = "Task Description"
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.CreateTaskAsync(createDto, listId, userId))
//            .ThrowsAsync(new UnauthorizedAccessException());

//        // Act
//        var result = await _controller.CreateTask(listId, createDto);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<TaskDetailsDto>>(result);
//        Assert.IsType<ForbidResult>(actionResult.Result);
//    }

//    [Fact]
//    public async Task GetTask_WhenExistsAndAuthorized_ReturnsOkResult()
//    {
//        // Arrange
//        var taskId = 1;
//        var userId = 1;
//        var expectedTask = new TaskDetailsDto
//        {
//            Id = taskId,
//            Title = "Test Task",
//            Description = "Test Description"
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.GetTaskDetailsAsync(taskId, userId))
//            .ReturnsAsync(expectedTask);

//        // Act
//        var result = await _controller.GetTask(taskId);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<TaskDetailsDto>>(result);
//        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
//        var task = Assert.IsType<TaskDetailsDto>(okResult.Value);
//        Assert.Equal(taskId, task.Id);
//    }

//    [Fact]
//    public async Task GetTask_WhenNotFound_ReturnsNotFound()
//    {
//        // Arrange
//        var taskId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.GetTaskDetailsAsync(taskId, userId))
//            .ThrowsAsync(new KeyNotFoundException());

//        // Act
//        var result = await _controller.GetTask(taskId);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<TaskDetailsDto>>(result);
//        Assert.IsType<NotFoundResult>(actionResult.Result);
//    }

//    [Fact]
//    public async Task UpdateTask_WhenAuthorized_ReturnsNoContent()
//    {
//        // Arrange
//        var taskId = 1;
//        var userId = 1;
//        var updateDto = new UpdateTaskDto
//        {
//            Title = "Updated Task",
//            Description = "Updated Description"
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.UpdateTaskAsync(taskId, updateDto, userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _controller.UpdateTask(taskId, updateDto);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task DeleteTask_WhenAuthorized_ReturnsNoContent()
//    {
//        // Arrange
//        var taskId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.DeleteTaskAsync(taskId, userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _controller.DeleteTask(taskId);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task ToggleTaskCompletion_WhenAuthorized_ReturnsNoContent()
//    {
//        // Arrange
//        var taskId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.ToggleTaskCompletionAsync(taskId, userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _controller.ToggleTaskCompletion(taskId);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task MoveTask_WhenAuthorized_ReturnsNoContent()
//    {
//        // Arrange
//        var taskId = 1;
//        var userId = 1;
//        var moveDto = new MoveTaskDto { NewListId = 2 };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.MoveTaskAsync(taskId, moveDto, userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _controller.MoveTask(taskId, moveDto);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task AddLabelToTask_WhenAuthorized_ReturnsNoContent()
//    {
//        // Arrange
//        var taskId = 1;
//        var userId = 1;
//        var addLabelDto = new AddLabelToTaskDto { LabelId = 1 };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.AddLabelToTaskAsync(taskId, addLabelDto, userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _controller.AddLabelToTask(taskId, addLabelDto);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task AddLabelToTask_WhenLabelNotFound_ReturnsNotFoundWithMessage()
//    {
//        // Arrange
//        var taskId = 1;
//        var userId = 1;
//        var addLabelDto = new AddLabelToTaskDto { LabelId = 999 };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.AddLabelToTaskAsync(taskId, addLabelDto, userId))
//            .ThrowsAsync(new KeyNotFoundException("Label not found"));

//        // Act
//        var result = await _controller.AddLabelToTask(taskId, addLabelDto);

//        // Assert
//        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//        Assert.Equal("Label not found", notFoundResult.Value);
//    }

//    [Fact]
//    public async Task AddLabelToTask_WhenLabelAlreadyAdded_ReturnsBadRequest()
//    {
//        // Arrange
//        var taskId = 1;
//        var userId = 1;
//        var addLabelDto = new AddLabelToTaskDto { LabelId = 1 };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.AddLabelToTaskAsync(taskId, addLabelDto, userId))
//            .ThrowsAsync(new InvalidOperationException("Label already added to task"));

//        // Act
//        var result = await _controller.AddLabelToTask(taskId, addLabelDto);

//        // Assert
//        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//        Assert.Equal("Label already added to task", badRequestResult.Value);
//    }

//    [Fact]
//    public async Task RemoveLabelFromTask_WhenAuthorized_ReturnsNoContent()
//    {
//        // Arrange
//        var taskId = 1;
//        var labelId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.RemoveLabelFromTaskAsync(taskId, labelId, userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _controller.RemoveLabelFromTask(taskId, labelId);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task RemoveLabelFromTask_WhenLabelNotFound_ReturnsNotFoundWithMessage()
//    {
//        // Arrange
//        var taskId = 1;
//        var labelId = 999;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.RemoveLabelFromTaskAsync(taskId, labelId, userId))
//            .ThrowsAsync(new KeyNotFoundException("Label not found on this task"));

//        // Act
//        var result = await _controller.RemoveLabelFromTask(taskId, labelId);

//        // Assert
//        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//        Assert.Equal("Label not found on this task", notFoundResult.Value);
//    }

//    [Fact]
//    public async Task GetFilteredTasks_WhenAuthorized_ReturnsOkResultWithTasks()
//    {
//        // Arrange
//        var userId = 1;
//        var expectedTasks = new List<TaskShortDto>
//            {
//                new TaskShortDto { Id = 1, Title = "Task 1" },
//                new TaskShortDto { Id = 2, Title = "Task 2" }
//            };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.GetFilteredTasksAsync(null, null, 1, 20, userId))
//            .ReturnsAsync(expectedTasks);

//        // Act
//        var result = await _controller.GetFilteredTasks(null, null);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskShortDto>>>(result);
//        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
//        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskShortDto>>(okResult.Value);
//        Assert.Equal(2, tasks.Count());
//    }

//    [Fact]
//    public async Task GetFilteredTasks_WithFilters_ReturnsFilteredTasks()
//    {
//        // Arrange
//        var userId = 1;
//        var dueDate = DateTime.UtcNow.AddDays(1);
//        var label = "urgent";
//        var expectedTasks = new List<TaskShortDto>
//            {
//                new TaskShortDto { Id = 1, Title = "Urgent Task" }
//            };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.GetFilteredTasksAsync(dueDate, label, 1, 20, userId))
//            .ReturnsAsync(expectedTasks);

//        // Act
//        var result = await _controller.GetFilteredTasks(dueDate, label);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskShortDto>>>(result);
//        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
//        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskShortDto>>(okResult.Value);
//        var task = Assert.Single(tasks);
//        Assert.Equal("Urgent Task", task.Title);
//    }

//    [Fact]
//    public async Task GetFilteredTasks_WhenNotAuthorized_ReturnsForbid()
//    {
//        // Arrange
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockTaskService.Setup(x => x.GetFilteredTasksAsync(null, null, 1, 20, userId))
//            .ThrowsAsync(new UnauthorizedAccessException());

//        // Act
//        var result = await _controller.GetFilteredTasks(null, null);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskShortDto>>>(result);
//        Assert.IsType<ForbidResult>(actionResult.Result);
//    }
//}